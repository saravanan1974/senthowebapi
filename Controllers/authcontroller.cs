using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SENTOSIAH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _pgConnectionString;
        private readonly IConfiguration _config;

        // Thread-safe refresh token store (username -> token)
        private static readonly ConcurrentDictionary<string, string> RefreshTokens = new();

        public AuthController(IConfiguration config)
        {
            _config = config;
            _pgConnectionString = config.GetConnectionString("Postgres") ?? "";
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test() => Ok("Welcome to Aarel");

        [HttpGet("companies")]
        public IActionResult GetCompanies()
        {
            try
            {
                var dt = new DataTable();
                using var pgCon = new NpgsqlConnection(_pgConnectionString);
                pgCon.Open();

                const string sql = "SELECT * FROM company";
                using var cmd = new NpgsqlCommand(sql, pgCon);
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);

                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var d = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                        d[col.ColumnName] = row[col];
                    list.Add(d);
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("accyear")]
        public IActionResult GetAccYear(short compid)
        {
            try
            {
                var dt = new DataTable();
                using var pgCon = new NpgsqlConnection(_pgConnectionString);
                pgCon.Open();

                const string sql = "SELECT * FROM accyear WHERE compid=@compid";
                using var cmd = new NpgsqlCommand(sql, pgCon);
                cmd.Parameters.AddWithValue("@compid", compid);

                using var reader = cmd.ExecuteReader();
                dt.Load(reader);

                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var d = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                        d[col.ColumnName] = row[col];
                    list.Add(d);
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("usertype")]
        public IActionResult GetUserType()
        {
            try
            {
                string jsonResult = "[]";
                using var pgCon = new NpgsqlConnection(_pgConnectionString);
                pgCon.Open();

                const string sql =
                    "SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM usertype) t";
                using var cmd = new NpgsqlCommand(sql, pgCon);
                var result = cmd.ExecuteScalar();
                jsonResult = result?.ToString() ?? "[]";

                // return raw JSON (avoid double serialization)
                return Content(jsonResult, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class AddUserDet
        {
            public string username { get; set; } = "";
            public string password1 { get; set; } = "";

            [System.Text.Json.Serialization.JsonIgnore]
            public string password =>
                Convert.ToBase64String(Encoding.UTF8.GetBytes(username + password1));

            public string firstname { get; set; } = "";
            public string usertype { get; set; } = "";
            public short usertypeid { get; set; }
        }

        [HttpPost("usercreation")]
        public IActionResult UserCreation([FromBody] List<AddUserDet> adduserdetlist, string username)
        {
            try
            {
                var json = JsonConvert.SerializeObject(adduserdetlist);

                using var pgCon = new NpgsqlConnection(_pgConnectionString);
                pgCon.Open();

                const string sql = "CALL usercreation_json(@p_json, @username)";
                using var cmd = new NpgsqlCommand(sql, pgCon);
                cmd.Parameters.Add("@p_json", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = json;
                cmd.Parameters.Add("@username", NpgsqlTypes.NpgsqlDbType.Text).Value = username;
                cmd.ExecuteNonQuery();

                return Ok("Stored procedure executed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class RefreshRequest
        {
            public string AccessToken { get; set; } = "";
            public string RefreshToken { get; set; } = "";
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal?.Identity?.Name is not string username || string.IsNullOrWhiteSpace(username))
                return Unauthorized();

            if (!RefreshTokens.TryGetValue(username, out var storedRefresh) ||
                storedRefresh != request.RefreshToken)
            {
                return Unauthorized();
            }

            var newAccessToken = GenerateJwtToken(username, isAccessToken: true);
            var newRefreshToken = GenerateJwtToken(username, isAccessToken: false);

            RefreshTokens[username] = newRefreshToken;
            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }

        private string GenerateJwtToken(string username, bool isAccessToken)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = isAccessToken
                ? DateTime.UtcNow.AddMinutes(double.Parse(jwt["AccessTokenExpirationMinutes"]!))
                : DateTime.UtcNow.AddDays(double.Parse(jwt["RefreshTokenExpirationDays"]!));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            var parms = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false, // allow expired
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"]
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, parms, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public class LoginPayload
        {
            public string uname { get; set; } = "";
            public string pwd { get; set; } = "";
            public int compid { get; set; }
        }

        [HttpPost("loginvalidation")]
        [AllowAnonymous]
        public IActionResult LoginValidation([FromBody] LoginPayload payload)
        {
            try
            {
                string jsonResult = "[]";

                using var pgCon = new NpgsqlConnection(_pgConnectionString);
                pgCon.Open();

                string raw = payload.uname + payload.pwd;
                string encodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

                const string sql =
                    "SELECT json_agg(row_to_json(t)) FROM (SELECT * FROM sechdr WHERE username=@p1 AND password=@p2) t";
                using var cmd = new NpgsqlCommand(sql, pgCon);
                cmd.Parameters.AddWithValue("@p1", payload.uname);
                cmd.Parameters.AddWithValue("@p2", encodedPassword);

                var result = cmd.ExecuteScalar();
                jsonResult = result?.ToString() ?? "[]";

                // no user found?
                if (jsonResult == "[]" || string.IsNullOrWhiteSpace(jsonResult))
                    return Unauthorized("Invalid login");

                // issue tokens
                var accessToken = GenerateJwtToken(payload.uname, true);
                var refreshToken = GenerateJwtToken(payload.uname, false);
                RefreshTokens[payload.uname] = refreshToken;

                return Ok(new { accessToken, refreshToken, data = JsonConvert.DeserializeObject(jsonResult) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

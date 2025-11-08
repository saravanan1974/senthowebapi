using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Text;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace  SENTOSIAH.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    //[AllowAnonymous]
    public class authController : ControllerBase
    {
        private readonly string _pgConnectionString;
        private readonly IConfiguration _config;
        private static readonly Dictionary<string, string> RefreshTokens = new();

        public authController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
            _config = config;
        }


       
   [HttpGet("test")]
    public IActionResult test()
    {
        return Ok("Welcome to Aarel");
    }


        [HttpGet("companies")]
        public IActionResult getCompanies()
        {
            try
            {
                var dt = new DataTable();

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "SELECT * FROM company  "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        //pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        using (var reader = pgCmd.ExecuteReader())
                        {
                            dt.Load(reader); // Fills the DataTable
                        }
                    }
                    pgCon.Close();
                }


                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    list.Add(dict);
                }

                return Ok(list); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        
        [HttpGet("accyear")]
        public IActionResult getaccyear(short compid)
        {
            try
            {
                var dt = new DataTable();

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "SELECT * FROM accyear where compid=@compid  "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@compid", compid);
                        using (var reader = pgCmd.ExecuteReader())
                        {
                            dt.Load(reader); // Fills the DataTable
                        }
                    }
                    pgCon.Close();
                }


                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    list.Add(dict);
                }

                return Ok(list); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("usertypeold")]
        public IActionResult getusertypeold()
        {
            try
            {
                var dt = new DataTable();

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "SELECT * FROM usertype  "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        //pgCmd.Parameters.AddWithValue("@clientid", clientid);
                        using (var reader = pgCmd.ExecuteReader())
                        {
                            dt.Load(reader); // Fills the DataTable
                        }
                    }
                    pgCon.Close();
                }


                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    list.Add(dict);
                }

                Console.WriteLine(list.ToString());
                return Ok(list); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("usertype")]
        public IActionResult getusertype()
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT * FROM usertype) t  "; // use your correct table

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class adduserdet
        {
            public string username { get; set; }
            public string password1 { get; set; }

            [System.Text.Json.Serialization.JsonIgnore] // ignore during serialization
            public string password =>
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + password1));
            public string firstname { get; set; }
            public string usertype { get; set; }
            public short usertypeid { get; set; }
        }

        [HttpPost("usercreation")]
        public IActionResult usercreation([FromBody] List<adduserdet> adduserdetlist, string username)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    jsonResult = JsonConvert.SerializeObject(adduserdetlist);

                    Console.WriteLine(jsonResult);

                    pgCon.Open();
                    string selectQuery = "call usercreation_json (@p_json,@username) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.Add("@p_json", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = jsonResult;
                        pgCmd.Parameters.Add("@username", NpgsqlTypes.NpgsqlDbType.Text).Value = username;

                        pgCmd.ExecuteNonQuery();
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                    pgCon.Close();
                }

                return Ok("Stored procedure executed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //"Key": "ThisIsASuperSecretKeyForJwtAuth1234567890!!",  

        public class RefreshRequest
        {
            public string AccessToken { get; set; } = "";
            public string RefreshToken { get; set; } = "";
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            Console.WriteLine(request.AccessToken);
            Console.WriteLine(principal);
            if (principal == null)
                return Unauthorized();

            var username = principal.Identity?.Name;
            
            Console.WriteLine(RefreshTokens[username]);
            Console.WriteLine(username);

            if (username == null ||
                !RefreshTokens.ContainsKey(username) ||
                RefreshTokens[username] != request.RefreshToken)
            {
                return Unauthorized();
            }

            var newAccessToken = GenerateJwtToken(username, true);
            var newRefreshToken = GenerateJwtToken(username, false);

            // Replace old refresh token
            RefreshTokens[username] = newRefreshToken;

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }

        // ✅ Generate Access or Refresh Token
        private string GenerateJwtToken(string username, bool isAccessToken)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = isAccessToken
                ? DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["AccessTokenExpirationMinutes"]!))
                : DateTime.UtcNow.AddDays(double.Parse(jwtSection["RefreshTokenExpirationDays"]!));

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            Console.WriteLine(creds);
            Console.WriteLine(expires);
            Console.WriteLine(token.ToString());
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ Validate expired token to extract claims
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false, // ⚡ allow expired token to be read
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
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
            public string uname { get; set; }
            public string pwd { get; set; }
            public int compid { get; set; }
        }

        [HttpPost("loginvalidation")]

        public IActionResult loginvalidation([FromBody] LoginPayload payload)
        {
            try
            {


                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    string raw = payload.uname + payload.pwd;
                    string encodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                    Console.WriteLine(payload.uname);
                    Console.WriteLine(encodedPassword);

                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM ( (SELECT * FROM sechdr where username=@p1 and password=@p2)) t "; // use your correct table
                    //string selectQuery = "SELECT * FROM sechdr where  password='YWRtaW4xMjM=' "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@p1", payload.uname);
                        pgCmd.Parameters.AddWithValue("@p2", encodedPassword);
                        var result = pgCmd.ExecuteScalar();
                        jsonResult = result?.ToString() ?? "[]";
                        Console.WriteLine(jsonResult);
                        Console.WriteLine("result");
                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }


                if (jsonResult == "")
                {
                    return StatusCode(500, $"Invalid login ");
                }
                else
                {
                    //  return Ok("OK");
                    Console.WriteLine(payload.uname);
                     var accessToken = GenerateJwtToken(payload.uname, true);
                    var refreshToken = GenerateJwtToken(payload.uname, false);

                    // Save refresh token (username → token)
                    RefreshTokens[payload.uname] = refreshToken;

                    return Ok(new { accessToken, refreshToken ,jsonResult});
                   // return Content(jsonResult, "application/json");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }
        


    }
}

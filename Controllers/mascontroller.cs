using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Text;
//using Newtonsoft.Json;
using System.Text.Json;
using NpgsqlTypes;
using Microsoft.AspNetCore.Authorization;
using System.Numerics;
namespace SENTOSIAH.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class masController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public masController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

#region  "Category"
        [HttpGet("getcategorymasvw")]
        public IActionResult getcategorymasvw(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT catid,catcode as code,catname as category,ishide  FROM category where compid=@compid order by catname) t  "; // use your correct table

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public class catpayload
        {
            public short mode { get; set; }
            public int catid { get; set; }
            public string catcode { get; set; }
            public string category { get; set; }
            public bool Ishide { get; set; } = false;
            public int userid { get; set; }
            public int compid { get; set; }
        }
        [HttpPost("categorysp")]
        public IActionResult categorysp([FromBody] catpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.catid);
                    Console.WriteLine(payload.catcode);
                    Console.WriteLine(payload.category);
                    Console.WriteLine(payload.Ishide);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);

                    pgCon.Open();
                    string selectQuery = "select categorysp(@mode,@p_ctid,@ctcode,@ctname,@hide,@uid,@cid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", payload.mode);
                        pgCmd.Parameters.AddWithValue("@p_ctid", payload.catid);
                        pgCmd.Parameters.AddWithValue("@ctcode", payload.catcode);
                        pgCmd.Parameters.AddWithValue("@ctname", payload.category);
                        pgCmd.Parameters.AddWithValue("@hide", payload.Ishide);
                        pgCmd.Parameters.AddWithValue("@uid", payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", payload.compid);

                        try
                        {
                            using (var reader = pgCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    jsonResult = reader.GetString(0);
                                }
                            }
                            Console.WriteLine(jsonResult);
                            //jsonResult = pgCmd.ExecuteReader()?.ToString();

                            var response = JsonSerializer.Deserialize<masterResponse>(jsonResult);

                            if (response.status && response.id.HasValue)
                            {
                                Console.WriteLine($"Inserted CatId: {response.id.Value}");
                            }
                            if (response.status == false)
                            {
                                return BadRequest(response);
                            }
                            else
                            {
                                return Ok(response);
                            }

                        }
                        catch (PostgresException ex)
                        {
                            Console.WriteLine($"Postgres error: {ex.MessageText}");
                            // ex.SqlState has the SQLSTATE code
                            return BadRequest(ex.MessageText);

                        }

                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, error = ex.Message });
            }
        }
        #endregion

#region "Unit"
        [HttpGet("getunitvw")]
        public IActionResult getunitvw(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT unitid,unitname as unit,iswgtbased  FROM unitmaster where compid=@compid order by unit) t  "; // use your correct table

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class unitpayload
        {
            public short mode { get; set; }
            public int unitid { get; set; }
            public string unitname { get; set; }
            public bool iswgtbased { get; set; } = false;
            public int userid { get; set; }
            public int compid { get; set; }
        }
        [HttpPost("unitsp")]
        public IActionResult unitsp([FromBody] unitpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.unitid);
                    Console.WriteLine(payload.unitname);
                    Console.WriteLine(payload.iswgtbased);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);

                    pgCon.Open();
                    string selectQuery = "select unitmastersp(@mode,@u_id,@uname,@wgtbased,@uid,@cid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", payload.mode);
                        pgCmd.Parameters.AddWithValue("@u_id", payload.unitid);
                        pgCmd.Parameters.AddWithValue("@uname", payload.unitname);
                        pgCmd.Parameters.AddWithValue("@wgtbased", payload.iswgtbased);
                        pgCmd.Parameters.AddWithValue("@uid", payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", payload.compid);

                        try
                        {
                            using (var reader = pgCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    jsonResult = reader.GetString(0);
                                }
                            }
                            Console.WriteLine(jsonResult);
                            //jsonResult = pgCmd.ExecuteReader()?.ToString();

                            var response = JsonSerializer.Deserialize<masterResponse>(jsonResult);

                            if (response.status && response.id.HasValue)
                            {
                                Console.WriteLine($"Inserted UnitId: {response.id.Value}");
                            }
                            if (response.status == false)
                            {
                                return BadRequest(response);
                            }
                            else
                            {
                                return Ok(response);
                            }

                        }
                        catch (PostgresException ex)
                        {
                            Console.WriteLine($"Postgres error: {ex.MessageText}");
                            // ex.SqlState has the SQLSTATE code
                            return BadRequest(ex.MessageText);

                        }

                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, error = ex.Message });
            }
        }
#endregion

#region "Area"
        [HttpGet("getareaview")]
        public IActionResult getareaview(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT areaid,areaname as area FROM areamaster where compid=@compid order by areaname) t  "; // use your correct table

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class areapayload
        {
            public short mode { get; set; }
            public int areaid { get; set; }
            public string areaname { get; set; }
            public int userid { get; set; }
            public int compid { get; set; }
        }
        [HttpPost("areasp")]
        public IActionResult areasp([FromBody] areapayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.areaid);
                    Console.WriteLine(payload.areaname);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);

                    pgCon.Open();
                    string selectQuery = "select areamastersp(@mode,@a_id,@aname,@uid,@cid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", payload.mode);
                        pgCmd.Parameters.AddWithValue("@a_id", payload.areaid);
                        pgCmd.Parameters.AddWithValue("@aname", payload.areaname);
                        pgCmd.Parameters.AddWithValue("@uid", payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", payload.compid);

                        try
                        {
                            using (var reader = pgCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    jsonResult = reader.GetString(0);
                                }
                            }
                            Console.WriteLine(jsonResult);
                            //jsonResult = pgCmd.ExecuteReader()?.ToString();

                            var response = JsonSerializer.Deserialize<masterResponse>(jsonResult);

                            if (response.status && response.id.HasValue)
                            {
                                Console.WriteLine($"Inserted areaId: {response.id.Value}");
                            }
                            if (response.status == false)
                            {
                                return BadRequest(response);
                            }
                            else
                            {
                                return Ok(response);
                            }

                        }
                        catch (PostgresException ex)
                        {
                            Console.WriteLine($"Postgres error: {ex.MessageText}");
                            // ex.SqlState has the SQLSTATE code
                            return BadRequest(ex.MessageText);

                        }

                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, error = ex.Message });
            }
        }
        #endregion

#region "item"


        [HttpGet("getitemmasview")]
        public IActionResult getitemmaster(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT itemid,itemcode,itemname,shortname,catname category,i.unitid,unitname,noofpack,wgtperpack,rate,i.iswgtbased FROM itemmaster i,category c,unitmaster u where i.catid=c.catid and i.unitid=u.unitid and i.compid=@compid order by itemid desc) t  "; // use your correct table

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("getitemfill")]
        public IActionResult getitemfill(int itemid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT itemid,itemcode,itemname,shortname,i.catid,u.unitid as untid,catname category,unitname,noofpack,wgtperpack,rate,i.iswgtbased FROM itemmaster i,category c,unitmaster u where i.catid=c.catid and i.unitid=u.unitid and i.compid=@compid and i.itemid=@itemid ) t  "; // use your correct table

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@itemid", itemid);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public class itempayload
        {
            public short mode { get; set; }
            public int itemid { get; set; }
            public string itemcode { get; set; }
            public string itemname { get; set; }
            public string shortname { get; set; }
            public int catid { get; set; }
            public short untid { get; set; }
            public short noofpack { get; set; }
            public decimal wgtperpack { get; set; }
            public decimal rate { get; set; }
            public Boolean iswgtbased { get; set; }
            public int userid { get; set; }
            public int compid { get; set; }
        }
        [HttpPost("itemsp")]
        public IActionResult itemsp([FromBody] itempayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.itemid);
                    Console.WriteLine(payload.itemcode);
                    Console.WriteLine(payload.itemname);
                    Console.WriteLine(payload.shortname);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);

                    pgCon.Open();
                    string selectQuery = "select itemmastersp(@mode,@i_id,@i_code,@i_name,@i_short,@i_cid,@i_uid,@i_noofpack,@i_wgtperpack,@i_rate,@i_iswgtbased,@uid,@cid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", payload.mode);
                        pgCmd.Parameters.AddWithValue("@i_id", payload.itemid);
                        pgCmd.Parameters.AddWithValue("@i_code", payload.itemcode);
                        pgCmd.Parameters.AddWithValue("@i_name", payload.itemname);
                        pgCmd.Parameters.AddWithValue("@i_short", payload.shortname);
                        pgCmd.Parameters.AddWithValue("@i_cid", payload.catid);
                        pgCmd.Parameters.AddWithValue("@i_uid", payload.untid);
                        pgCmd.Parameters.AddWithValue("@i_noofpack", payload.noofpack);
                        pgCmd.Parameters.AddWithValue("@i_wgtperpack", payload.wgtperpack);
                        pgCmd.Parameters.AddWithValue("@i_rate", payload.rate);
                        pgCmd.Parameters.AddWithValue("@i_iswgtbased", payload.iswgtbased);
                        pgCmd.Parameters.AddWithValue("@uid", payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", payload.compid);

                        try
                        {
                            using (var reader = pgCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    jsonResult = reader.GetString(0);
                                }
                            }
                            Console.WriteLine(jsonResult);
                            //jsonResult = pgCmd.ExecuteReader()?.ToString();

                            var response = JsonSerializer.Deserialize<masterResponse>(jsonResult);

                            if (response.status && response.id.HasValue)
                            {
                                Console.WriteLine($"Inserted ItemId: {response.id.Value}");
                            }
                            if (response.status == false)
                            {
                                return BadRequest(response);
                            }
                            else
                            {
                                return Ok(response);
                            }

                        }
                        catch (PostgresException ex)
                        {
                            Console.WriteLine($"Postgres error: {ex.MessageText}");
                            // ex.SqlState has the SQLSTATE code
                            return BadRequest(ex.MessageText);

                        }

                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, error = ex.Message });
            }
        }
        #endregion

#region "customer"


        [HttpGet("getcustomermasview")]
        public IActionResult getcustomermasview(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT *,subgrpname,grpname from customermas m,subgroup s,groupmas p where m.subgrpid=s.subgrpid and m.grpid=p.grpid and m.compid=@compid order by custid desc) t  "; // use your correct table

                    Console.WriteLine(selectQuery);
                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("getcustomerfill")]
        public IActionResult getcustomerfill(int custid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT *,subgrpname,grpname from customermas m,subgroup s,groupmas p where m.subgrpid=s.subgrpid and m.grpid=p.grpid and m.custid=@custid and  m.compid=@compid order by custid desc) t  "; // use your correct table

                    Console.WriteLine(selectQuery);
                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@custid", custid);
                    pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

  [HttpGet("getgroupmas")]
        public IActionResult getgroupmas(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT * from groupmas p order by grpname) t  "; // use your correct table

                    Console.WriteLine(selectQuery);
                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                   // pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


  [HttpGet("getsubgroupmas")]
        public IActionResult getsubgroupmas(int grpid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT * from subgroup s where  s.grpid=@grpid  order by subgrpname ) t  "; // use your correct table

                    Console.WriteLine(selectQuery);
                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@grpid", grpid);
                    //pgCmd.Parameters.AddWithValue("@compid", compid);

                    var result = pgCmd.ExecuteScalar();
                    jsonResult = result?.ToString() ?? "[]";
                    Console.WriteLine(jsonResult);
                    pgCon.Close();
                }
                if (jsonResult == "")
                {
                    jsonResult = "[]";
                }

                return Ok(jsonResult); // Automatically serializes to JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class customerpayload
        {
            public short mode { get; set; }
            public int custid { get; set; }
            public string custcode { get; set; }
            public string custname { get; set; }
            public int subgrpid { get; set; }
            public int grpid { get; set; }
            public string undergrp { get; set; }
            public string grptype { get; set; }
            public short areaid { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public string pincode { get; set; }
            public string cellno { get; set; }
            public string mailid { get; set; }
            public short userid { get; set; }
            public short compid { get; set; }
        }

        [HttpPost("customermassp")]
        public IActionResult customermassp([FromBody] customerpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.custid);
                    Console.WriteLine(payload.custcode);
                    Console.WriteLine(payload.custname);
                    Console.WriteLine(payload.cellno);
                    Console.WriteLine(payload.areaid);
                    Console.WriteLine(payload.subgrpid);
                    Console.WriteLine(payload.grpid);
                    Console.WriteLine(payload.address1);
                    Console.WriteLine(payload.address2);
                    Console.WriteLine(payload.city);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);
                    ////custid, custcode, custname, subgrpid,grpid,undergrp,grptype,areaid,address1,address2,city,pincode,cellno,mailid, userid, compid
                    pgCon.Open();
                    string selectQuery = "select customermassp(@mode,@p_custid,@p_custcode,@p_custname,@p_subgrpid,@p_grpid,@p_undergrp,@p_grptype,@p_areaid,@p_address1,@p_address2,@p_city,@p_pincode,@p_cellno,@p_mailid,@uid,@cid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", payload.mode);
                        pgCmd.Parameters.AddWithValue("@p_custid", payload.custid);
                        pgCmd.Parameters.AddWithValue("@p_custcode", payload.custcode);
                        pgCmd.Parameters.AddWithValue("@p_custname", payload.custname);
                        pgCmd.Parameters.AddWithValue("@p_subgrpid", payload.subgrpid);
                        pgCmd.Parameters.AddWithValue("@p_grpid", payload.grpid);
                        pgCmd.Parameters.AddWithValue("@p_undergrp", payload.undergrp);
                        pgCmd.Parameters.AddWithValue("@p_grptype", payload.grptype);
                        pgCmd.Parameters.AddWithValue("@p_areaid", payload.areaid);
                        pgCmd.Parameters.AddWithValue("@p_address1", payload.address1);
                        pgCmd.Parameters.AddWithValue("@p_address2", payload.address2);
                        pgCmd.Parameters.AddWithValue("@p_city", payload.city);
                        pgCmd.Parameters.AddWithValue("@p_pincode", payload.pincode);
                        pgCmd.Parameters.AddWithValue("@p_cellno", payload.cellno);
                        pgCmd.Parameters.AddWithValue("@p_mailid", payload.mailid);
                        pgCmd.Parameters.AddWithValue("@uid", payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", payload.compid);

                        try
                        {
                            using (var reader = pgCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    jsonResult = reader.GetString(0);
                                }
                            }
                            Console.WriteLine(jsonResult);
                            //jsonResult = pgCmd.ExecuteReader()?.ToString();

                            var response = JsonSerializer.Deserialize<masterResponse>(jsonResult);

                            if (response.status && response.id.HasValue)
                            {
                                Console.WriteLine($"Inserted customer id: {response.id.Value}");
                            }
                            if (response.status == false)
                            {
                                return BadRequest(response);
                            }
                            else
                            {
                                return Ok(response);
                            }

                        }
                        catch (PostgresException ex)
                        {
                            Console.WriteLine($"Postgres error: {ex.MessageText}");
                            // ex.SqlState has the SQLSTATE code
                            return BadRequest(ex.MessageText);

                        }

                        // using (var reader = pgCmd.ExecuteReader())
                        // {
                        //     dt.Load(reader); // Fills the DataTable
                        // }
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, error = ex.Message });
            }
        }

        #endregion


        public class masterResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int? id { get; set; }
        }

    }

}
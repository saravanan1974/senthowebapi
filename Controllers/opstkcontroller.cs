using Microsoft.AspNetCore.Mvc;
using System.Data;
using Npgsql;
using System.Text;
//using Newtonsoft.Json;
using System.Text.Json;
using NpgsqlTypes;
using System.Numerics;
namespace SENTOSIAH.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class opstkController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public opstkController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

        public class opstkpayload
        {
            public short mode { get; set; }
            public int opid { get; set; }
            public int itemid { get; set; }
             public int unitid { get; set; }
            public short noofpack { get; set; }
            public decimal wtperpack { get; set; }
            public decimal wt { get; set; }
            public int qty { get; set; }
            public short userid { get; set; }
            public short compid { get; set; }
            
        }


        [HttpPost("opstksavesp")]
        public IActionResult opstksavesp([FromBody] opstkpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.opid);
                    Console.WriteLine(payload.itemid);
                    Console.WriteLine(payload.wtperpack);
                    Console.WriteLine(payload.noofpack);
                    Console.WriteLine(payload.qty);
                    Console.WriteLine(payload.wt);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);
                    

                    pgCon.Open();

                    //mode smallint,	op_id integer,	op_itemid integer,	op_unitid integer,	op_noofpack integer,	op_wtperpack numeric,	op_qty integer,	op_wt numeric,	uid smallint,	cid smallint)
                    string selectQuery = "select opstksp (@mode::smallint,@op_id,@op_itemid,@op_unitid,@op_noofpack,@op_wtperpack,@op_qty,@op_wt,@uid,@cid) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", NpgsqlDbType.Smallint, payload.mode);
                        pgCmd.Parameters.AddWithValue("@op_id", NpgsqlDbType.Integer, payload.opid);
                        pgCmd.Parameters.AddWithValue("@op_itemid", NpgsqlDbType.Integer, payload.itemid);
                        pgCmd.Parameters.AddWithValue("@op_unitid", NpgsqlDbType.Integer, payload.unitid);
                        pgCmd.Parameters.AddWithValue("@op_noofpack", NpgsqlDbType.Integer, payload.noofpack);
                        pgCmd.Parameters.AddWithValue("@op_wtperpack", NpgsqlDbType.Numeric, payload.wtperpack);
                        pgCmd.Parameters.AddWithValue("@op_qty", NpgsqlDbType.Integer, payload.qty);
                        pgCmd.Parameters.AddWithValue("@op_wt", NpgsqlDbType.Numeric, payload.wt);
                        pgCmd.Parameters.AddWithValue("@uid", NpgsqlDbType.Smallint, payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", NpgsqlDbType.Smallint, payload.compid);
                        

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

                            var response = JsonSerializer.Deserialize<OpstkResponse>(jsonResult);

                            if (response.status && response.id.HasValue)
                            {
                                Console.WriteLine($"Inserted opening no: {response.id.Value}");
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
        public class OpstkResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int? id { get; set; }
        }



        [HttpGet("getopstkview")]
        public IActionResult getsoview(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
                
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT opid,h.itemid,itemname,itemcode,h.noofpack,h.wtperpack,h.qty,h.wt  from  opstk h,itemmaster i where h.itemid=i.itemid and i.compid=@compid  order by itemname) t"; // use your correct table
                    
                    Console.WriteLine(compid);
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
                return StatusCode(500, $"{ex.Message}");
            }
        }




    }

}


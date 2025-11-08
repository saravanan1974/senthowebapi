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
    public class orderController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public orderController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

        public class ordpayload
        {
            public short mode { get; set; }
            public int soid { get; set; }
            public int sono { get; set; }
            public DateTime sodt { get; set; }
            public int custid { get; set; }
            public DateTime deliverydt { get; set; }
            public string narrtion { get; set; }
            public decimal total { get; set; }
            public Boolean isopening { get; set; }
            public short userid { get; set; }
            public short compid { get; set; }
            public List<orddetpayload> orddetail { get; set; } = new List<orddetpayload>();
            public class orddetpayload
            {
                public int soid { get; set; }
                public Guid sodetgid { get; set; }
                public int itemid { get; set; }
                public int unitid { get; set; }
                public short noofpack { get; set; }
                public decimal wtperpack { get; set; }
                public decimal wt { get; set; }
                public int qty { get; set; }
                public decimal rate { get; set; }
                public decimal amount { get; set; }
                public decimal slno { get; set; }
                public Boolean isitemcancelled { get; set; }
                public Boolean isbilled { get; set; }
            }

        }


        [HttpPost("sosavesp")]
        public IActionResult sosavesp([FromBody] ordpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                Console.WriteLine(JsonSerializer.Serialize(payload.orddetail));
                //var spec = new ordpayload.orddetpayload();
                string jsonstring = JsonSerializer.Serialize(payload.orddetail);



                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.soid);
                    Console.WriteLine(payload.sono);
                    Console.WriteLine(payload.sodt);
                    Console.WriteLine(payload.deliverydt);
                    Console.WriteLine(payload.narrtion);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);
                    Console.WriteLine(jsonstring);

                    pgCon.Open();

                    //mode smallint,	o_id integer,	so_no integer,	so_dt date,	cust_id integer,	delivery_dt date,	
                    // nar_text text,	totamt numeric,	is_opening boolean,	uid smallint,	cid smallint,	orddet_json jsonb                    
                    string selectQuery = "select sosp_json (@mode::smallint,@o_id,@so_no,@so_dt,@cust_id,@delivery_dt,@nar_text,@totamt,@is_opening,@uid,@cid,@orddet_json) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", NpgsqlDbType.Smallint, payload.mode);
                        pgCmd.Parameters.AddWithValue("@o_id", NpgsqlDbType.Integer, payload.soid);
                        pgCmd.Parameters.AddWithValue("@so_no", NpgsqlDbType.Integer, payload.sono);
                        pgCmd.Parameters.AddWithValue("@so_dt", NpgsqlDbType.Date, Convert.ToDateTime(payload.sodt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@cust_id", NpgsqlDbType.Integer, payload.custid);
                        pgCmd.Parameters.AddWithValue("@delivery_dt", NpgsqlDbType.Date, Convert.ToDateTime(payload.deliverydt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@nar_text", NpgsqlDbType.Text, payload.narrtion);
                        pgCmd.Parameters.AddWithValue("@totamt", NpgsqlDbType.Numeric, payload.total);
                        pgCmd.Parameters.AddWithValue("@is_opening", NpgsqlDbType.Boolean, payload.isopening);
                        pgCmd.Parameters.AddWithValue("@uid", NpgsqlDbType.Smallint, payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", NpgsqlDbType.Smallint, payload.compid);
                        pgCmd.Parameters.Add("@orddet_json", NpgsqlDbType.Jsonb).Value = jsonstring;

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

                            var response = JsonSerializer.Deserialize<OrderResponse>(jsonResult);

                            if (response.status && response.sono.HasValue)
                            {
                                Console.WriteLine($"Inserted order no: {response.sono.Value}");
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
        public class OrderResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int? sono { get; set; }
        }



        [HttpGet("getsoview")]
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
                    selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT soid,sono,sodt,custname,cellno,address1 as address,city,deliverydt,total,iscancelled  from  sohdr h,customermas c where h.custid=c.custid and h.compid=@compid  order by sono desc) t"; // use your correct table
                    
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



        [HttpGet("getsohdrfill")]
        public IActionResult getsohdrfill(int soid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
             
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                   

                   selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT soid,sono,sodt,h.custid,custname as customer,address1,address2,city,pincode,cellno,deliverydt,remarks,total,isopening,iscancelled from  sohdr h,customermas c where h.custid=c.custid and h.soid=@soid and h.compid=@compid  order by sono desc) t "; // use your correct table
                   

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@soid", soid);
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


 [HttpGet("getsodetailfill")]
        public IActionResult getsodetailfill(int soid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
              
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                   
                   selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT h.soid,d.itemid,itemname,d.unitid,unitname,d.noofpack,d.wtperpack,qty,d.wt,d.rate,amount,d.slno,isitemcancelled,h.sogid,sodetgid,i.iswgtbased from  sohdr h,sodet d,itemmaster i,unitmaster u where h.soid=d.soid  and d.itemid=i.itemid and d.unitid=u.unitid and h.soid=@soid and h.compid=@compid order by slno) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@soid", soid);
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


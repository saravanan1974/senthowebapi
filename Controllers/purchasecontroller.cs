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
    public class purchaseController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public purchaseController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }

        //mode smallint,	pur_id integer,	pur_no integer,	pur_dt date,	inv_no text,	inv_dt date,	pty_id integer,
        //cash_mode boolean,	termsdet_txt text,	tot_amt numeric,	paid_amt numeric,	remarks_txt text,
	    //note_txt text,	uid smallint,	cid smallint,	purdet_json jsonb
        public class purpayload
        {
            public short mode { get; set; }
            public int purid { get; set; }
            public int purno { get; set; }
            public DateTime purdt { get; set; }
            public string invno { get; set; }
            public DateTime invdt { get; set; }
            public int ptyid { get; set; }
            public Boolean cashmode { get; set; }
            public string termsdet{ get; set; }
            
            public decimal totamt { get; set; }
            public decimal paidamt{ get; set; }
            public string remarks { get; set; }
            public string notedet { get; set; }
            public short userid { get; set; }
            public short compid { get; set; }
            public List<purdetpayload> purdetail { get; set; } = new List<purdetpayload>();
            public class purdetpayload
            {
                public int purid { get; set; }
                public int itemid { get; set; }
                public int unitid { get; set; }
                
                public int qty { get; set; }
                public decimal wt { get; set; }
                public decimal rate { get; set; }
                public decimal amount { get; set; }
                public decimal wtperpack { get; set; }
                public short noofpack { get; set; }
                public decimal slno { get; set; }
            }

        }


        [HttpPost("pursavesp")]
        public IActionResult pursavesp([FromBody] purpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                Console.WriteLine(JsonSerializer.Serialize(payload.purdetail));
                //var spec = new ordpayload.orddetpayload();
                string jsonstring = JsonSerializer.Serialize(payload.purdetail);



                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.purid);
                    Console.WriteLine(payload.purno);
                    Console.WriteLine(payload.purdt);
                    Console.WriteLine(payload.invno);
                    Console.WriteLine(payload.invdt);
                    Console.WriteLine(payload.termsdet);
                    Console.WriteLine(payload.notedet);
                    Console.WriteLine(payload.remarks);
                    Console.WriteLine(payload.totamt);
                    Console.WriteLine(payload.paidamt);
                    Console.WriteLine(payload.ptyid);
                    
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);
                    Console.WriteLine(jsonstring);

                    pgCon.Open();

        //mode smallint,	pur_id integer,	pur_no integer,	pur_dt date,	inv_no text,	inv_dt date,	pty_id integer,
        //cash_mode boolean,	termsdet_txt text,	tot_amt numeric,	paid_amt numeric,	remarks_txt text,
	    //note_txt text,	uid smallint,	cid smallint,	purdet_json jsonb

                    string selectQuery = "select pursp_json (@mode::smallint,@pur_id,@pur_no,@pur_dt,@inv_no,@inv_dt,@pty_id,@cash_mode,@termsdet_txt,@tot_amt,@paid_amt,@remarks_txt,@note_txt,@uid,@cid,@purdet_json) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", NpgsqlDbType.Smallint, payload.mode);
                        pgCmd.Parameters.AddWithValue("@pur_id", NpgsqlDbType.Integer, payload.purid);
                        pgCmd.Parameters.AddWithValue("@pur_no", NpgsqlDbType.Integer, payload.purno);
                        pgCmd.Parameters.AddWithValue("@pur_dt", NpgsqlDbType.Date, Convert.ToDateTime(payload.purdt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@inv_no", NpgsqlDbType.Text, payload.invno);
                        pgCmd.Parameters.AddWithValue("@inv_dt", NpgsqlDbType.Date, Convert.ToDateTime(payload.invdt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@pty_id", NpgsqlDbType.Integer, payload.ptyid);
                        pgCmd.Parameters.AddWithValue("@cash_mode", NpgsqlDbType.Boolean, payload.cashmode);
                        pgCmd.Parameters.AddWithValue("@termsdet_txt", NpgsqlDbType.Text, payload.termsdet);
                        pgCmd.Parameters.AddWithValue("@tot_amt", NpgsqlDbType.Numeric, payload.totamt);
                        pgCmd.Parameters.AddWithValue("@paid_amt", NpgsqlDbType.Numeric, payload.paidamt);
                        pgCmd.Parameters.AddWithValue("@remarks_txt", NpgsqlDbType.Text, payload.remarks);
                        pgCmd.Parameters.AddWithValue("@note_txt", NpgsqlDbType.Text, payload.notedet);
                        pgCmd.Parameters.AddWithValue("@uid", NpgsqlDbType.Smallint, payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", NpgsqlDbType.Smallint, payload.compid);
                        pgCmd.Parameters.Add("@purdet_json", NpgsqlDbType.Jsonb).Value = jsonstring;

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

                            var response = JsonSerializer.Deserialize<purResponse>(jsonResult);

                            if (response.status && response.purno.HasValue)
                            {
                                Console.WriteLine($"Inserted order no: {response.purno.Value}");
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
        public class purResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int? purno { get; set; }
        }



        [HttpGet("getpurview")]
        public IActionResult getpurview(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
                
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT purid,purno,purdt,invno,invdt,custname supplier,cellno,case when cashmode=true then 'CASH' else 'CREDIT' end as cashmode,totamt,paidamt  from  purhdr h,customermas c where h.ptyid=c.custid and h.compid=@compid  order by purno desc) t"; // use your correct table
                    
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



        [HttpGet("getpurhdrfill")]
        public IActionResult getpurhdrfill(int purid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
             
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                   

                   selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT purid,purno,purdt,invno,invdt,h.ptyid,custname as supplier,address1,address2,city,pincode,cellno,termsdet,remarks,note,totamt,paidamt,cashmode from  purhdr h,customermas c where h.ptyid=c.custid and h.purid=@purid and h.compid=@compid  order by purno desc) t "; // use your correct table
                   

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@purid", purid);
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


 [HttpGet("getpurdetailfill")]
        public IActionResult getpurdetailfill(int purid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
              
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                   
                   selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT h.purid,d.itemid,itemname,d.unitid,unitname,d.noofpack,d.wtperpack,qty,d.wt,d.rate,amount,d.slno from  purhdr h,purdet d,itemmaster i,unitmaster u where h.purid=d.purid  and d.itemid=i.itemid and d.unitid=u.unitid and h.purid=@purid and h.compid=@compid order by slno) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@purid", purid);
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


        #region "suppler fill"

        [HttpGet("getsupplier")]
        public IActionResult getsupplier(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT * from customervw m where m.subgrpid in (1) and m.compid=@compid order by custname) t  "; // use your correct table

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


        #endregion


    }

}


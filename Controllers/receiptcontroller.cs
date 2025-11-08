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
    public class receiptController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public receiptController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }
        
        //  mode smallint,	rec_id integer,	rec_no integer,	rec_dt date,	rec_refno text,	cust_id integer,	cash_mode boolean,
	    //bank_id integer,	bank_type text,	bank_det text,	nar_text text,	rec_amt numeric,	disc_amt numeric,
	    //uid smallint,	cid smallint,	recdet_json jsonb
        public class recpayload
        {
            public short mode { get; set; }
            public int recid { get; set; }
            public int recno { get; set; }
            public DateTime recdt { get; set; }
            public string rec_refno { get; set; }
            public int custid { get; set; }
            public Boolean cashmode { get; set; }
            public int bankid { get; set; }
            public string banktype { get; set; }
            public string bankdet{ get; set; }
            public string narrtion { get; set; }
            public decimal recamt { get; set; }
            public decimal discamt { get; set; }
            public short userid { get; set; }
            public short compid { get; set; }
            public List<recdetpayload> recdetail { get; set; } = new List<recdetpayload>();
            //recid,docref,docrefid,docrefno,docrefdt,billamt,paidamt,discount
            public class recdetpayload
            {
                public int recid { get; set; }
                public string docref { get; set; }
                public int docrefid { get; set; }
                public string docrefno { get; set; }
                 public DateTime docrefdt { get; set; }
                public decimal billamt { get; set; }
                public decimal paidamt { get; set; }
                public decimal discount { get; set; }
                
            }

        }


        [HttpPost("recsavesp")]
        public IActionResult recsavesp([FromBody] recpayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                Console.WriteLine(JsonSerializer.Serialize(payload.recdetail));
                

                //var spec = new ordpayload.orddetpayload();
                string jsonstring = JsonSerializer.Serialize(payload.recdetail);
                

                Console.WriteLine(jsonstring);
                

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.recid);
                    Console.WriteLine(payload.recno);
                    Console.WriteLine(payload.recdt);
                    Console.WriteLine(payload.cashmode);
                    Console.WriteLine(payload.recamt);
                    Console.WriteLine(payload.custid);
                    Console.WriteLine(payload.discamt);
                    Console.WriteLine(payload.narrtion);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);

                    pgCon.Open();

                    //	mode smallint,	rec_id integer,	rec_no integer,	rec_dt date,	rec_refno text,	cust_id integer,	cash_mode boolean,
	                //bank_id integer,	bank_type text,	bank_det text,	nar_text text,	rec_amt numeric,	disc_amt numeric,
	                //uid smallint,	cid smallint,	recdet_json jsonb)
                    string selectQuery = "select receiptsp_json (@mode::smallint,@rec_id,@rec_no,@rec_dt,@rec_refno,@cust_id,@cash_mode,@bank_id,@bank_type,@bank_det,@nar_text,@rec_amt,@disc_amt,@uid,@cid,@recdet_json) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", NpgsqlDbType.Smallint, payload.mode);
                        pgCmd.Parameters.AddWithValue("@rec_id", NpgsqlDbType.Integer, payload.recno);
                        pgCmd.Parameters.AddWithValue("@rec_no", NpgsqlDbType.Integer, payload.recno);
                        pgCmd.Parameters.AddWithValue("@rec_dt", NpgsqlDbType.Date, Convert.ToDateTime(payload.recdt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@rec_refno", NpgsqlDbType.Text, payload.rec_refno);
                        pgCmd.Parameters.AddWithValue("@cust_id", NpgsqlDbType.Integer, payload.custid);
                        pgCmd.Parameters.AddWithValue("@cash_mode", NpgsqlDbType.Boolean, payload.cashmode);
                        pgCmd.Parameters.AddWithValue("@bank_id", NpgsqlDbType.Integer, payload.bankid);
                        pgCmd.Parameters.AddWithValue("@bank_type", NpgsqlDbType.Text, payload.banktype);
                        pgCmd.Parameters.AddWithValue("@bank_det", NpgsqlDbType.Text, payload.bankdet);
                        pgCmd.Parameters.AddWithValue("@nar_text", NpgsqlDbType.Text, payload.narrtion);
                        pgCmd.Parameters.AddWithValue("@rec_amt", NpgsqlDbType.Numeric, payload.recamt);
                        pgCmd.Parameters.AddWithValue("@disc_amt", NpgsqlDbType.Numeric, payload.discamt);
                        pgCmd.Parameters.AddWithValue("@uid", NpgsqlDbType.Smallint, payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", NpgsqlDbType.Smallint, payload.compid);
                        pgCmd.Parameters.Add("@recdet_json", NpgsqlDbType.Jsonb).Value = jsonstring;
                        

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

                            var response = JsonSerializer.Deserialize<receiptResponse>(jsonResult);

                            if (response.status && response.recno.HasValue)
                            {
                                Console.WriteLine($"Inserted rec no: {response.recno.Value}");
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
        public class receiptResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int? recno { get; set; }
        }



        [HttpGet("getrecview")]
        public IActionResult getrecview(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT recid,recno,recdt,recrefno,custname,cellno,address1 as address,city,case when cashmode=true then 'CASH' else 'BANK' end cashmode,recamt,cashdiscamt from  rechdr h,customermas c where  h.custid=c.custid and h.compid=@compid  order by recno desc) t"; // use your correct table

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

        [HttpGet("getrechdrfill")]
        public IActionResult getrechdrfill(int recid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();


                    selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT recid,recno,recdt,recrefno,h.custid,custname,cellno,address1 as address,city,case when cashmode=true then 'CASH' else 'BANK' end cashmode, bankid, banktype, bankdet,recamt,cashdiscamt from  rechdr h,customermas c  where  h.custid=c.custid and recid=@recid and h.compid=@compid    ) t "; // use your correct table                   

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@recid", recid);
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

        [HttpGet("getrecdetfill")]
        public IActionResult getrecdetfill(int recid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();

                    selectQuery = "select json_agg(row_to_json(t)) FROM (select recid,d.docref,d.docrefid,d.docrefno,d.docrefdt,d.billamt,paidamt,discount,amt+paidamt as balance  from recdet d,acc_adjustmentpendvw p where d.docrefid=p.docrefid and d.docref=p.docref and d.recid=@recid order by d.docrefno) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@recid", recid);
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
                return StatusCode(500, $"{ex.Message}");
            }
        }


        #region "customer fill"

        [HttpGet("getcustomer")]
        public IActionResult getcustomer(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT * from customermas m where m.subgrpid in (2) and m.compid=@compid order by custname desc) t  "; // use your correct table

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

        #region "bank fill"

        [HttpGet("getbank")]
        public IActionResult getbank(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT custid as accid,custname as bankname from customermas m where m.subgrpid in (7) and compid=@compid order by custname desc) t  "; // use your correct table

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

        #region  "bill Pending"

        [HttpGet("getbillpending")]
        public IActionResult getbillpending(int custid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (select p.docrefid,p.docref,p.docrefno,p.docrefdt,billamt,amt as balance from acc_adjustmentpendvw  p where p.custid=@custid and p.compid=@compid  and amt>0 order by docrefdt desc) t  "; // use your correct table
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

        #endregion

    }

}


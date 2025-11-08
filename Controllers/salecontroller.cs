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
    public class saleController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public saleController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }
        //	mode smallint,	sal_id integer,	bl_no integer,	bl_dt date,	cust_id integer,	so_id integer,
        //	so_gid uuid,	cash_mode boolean,	nar_text text,	tot_amt numeric,	disc_amt numeric,
        //cash_recamt numeric,	bank_recamt numeric,	uid smallint,	cid smallint,	saldet_json jsonb,
        // salbankdet_json jsonb
        public class salespayload
        {
            public short mode { get; set; }
            public int salid { get; set; }
            public int billno { get; set; }
            public DateTime billdt { get; set; }
            public int custid { get; set; }

            public int soid { get; set; }
            public Guid sogid { get; set; }
            public Boolean cashmode { get; set; }
            public string narrtion { get; set; }
            public decimal total { get; set; }
            public decimal discper { get; set; }
            public decimal discamt { get; set; }
            public decimal cashrecamt { get; set; }
            public decimal banhrecamt { get; set; }
            public short userid { get; set; }
            public short compid { get; set; }
            public List<saledetpayload> saledetail { get; set; } = new List<saledetpayload>();
            //salid, itemid,sodetgid,unitid,noofpacket,wtperpack,qty,wt,rate,amount,sogid,slno,sgid,saldetgid
            public class saledetpayload
            {
                public int salid { get; set; }
                public int itemid { get; set; }
                public Guid sogid { get; set; }
                public Guid sodetgid { get; set; }
                public int unitid { get; set; }
                public short noofpack { get; set; }
                public decimal wtperpack { get; set; }
                public decimal wt { get; set; }
                public int qty { get; set; }
                public decimal rate { get; set; }
                public decimal amount { get; set; }
                public Guid saldetgid { get; set; }
                public decimal slno { get; set; }

            }
            public List<salebankdetpayload> salebankdetail { get; set; } = new List<salebankdetpayload>();
            //salid, itemid,sodetgid,unitid,noofpack,wtperpack,qty,wt,rate,amount,sogid,slno,sgid,saldetgid
            public class salebankdetpayload
            {
                public int salid { get; set; }
                public int bankid { get; set; }
                public Guid salgid { get; set; }
                public decimal amount { get; set; }

            }

        }


        [HttpPost("salsavesp")]
        public IActionResult salsavesp([FromBody] salespayload payload)
        {
            try
            {

                var dt = new DataTable();
                string jsonResult = string.Empty;

                Console.WriteLine(JsonSerializer.Serialize(payload.saledetail));
                Console.WriteLine(JsonSerializer.Serialize(payload.salebankdetail));

                //var spec = new ordpayload.orddetpayload();
                string jsonstring = JsonSerializer.Serialize(payload.saledetail);
                string jsonbankstring = JsonSerializer.Serialize(payload.salebankdetail);

                Console.WriteLine(jsonstring);
                Console.WriteLine(jsonbankstring);

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {

                    Console.WriteLine(payload.salid);
                    Console.WriteLine(payload.soid);
                    Console.WriteLine(payload.billno);
                    Console.WriteLine(payload.billdt);
                    Console.WriteLine(payload.cashmode);
                    Console.WriteLine(payload.cashrecamt);
                    Console.WriteLine(payload.banhrecamt);

                    Console.WriteLine(payload.custid);
                    Console.WriteLine(payload.discper);
                    Console.WriteLine(payload.discamt);
                    Console.WriteLine(payload.narrtion);
                    Console.WriteLine(payload.userid);
                    Console.WriteLine(payload.compid);

                    pgCon.Open();

                    //mode smallint,	sal_id integer,	bl_no integer,	bl_dt date,	cust_id integer,	so_id integer,	so_gid uuid,	cash_mode boolean,
                    //nar_text text,tot_amt numeric,	disc_amt numeric,	cash_recamt numeric,	bank_recamt numeric,	uid smallint,	cid smallint,	saldet_json jsonb,	salbankdet_json jsonb
                    string selectQuery = "select salesp_json (@mode::smallint,@sal_id,@bl_no,@bl_dt,@cust_id,@so_id,@so_gid,@cash_mode,@nar_text,@tot_amt,@disc_per,@disc_amt,@cash_recamt,@bank_recamt,@uid,@cid,@saldet_json,@salbankdet_json) "; // use your correct table
                    using (var pgCmd = new NpgsqlCommand(selectQuery, pgCon))
                    {
                        pgCmd.Parameters.AddWithValue("@mode", NpgsqlDbType.Smallint, payload.mode);
                        pgCmd.Parameters.AddWithValue("@sal_id", NpgsqlDbType.Integer, payload.salid);
                        pgCmd.Parameters.AddWithValue("@bl_no", NpgsqlDbType.Integer, payload.billno);
                        pgCmd.Parameters.AddWithValue("@bl_dt", NpgsqlDbType.Date, Convert.ToDateTime(payload.billdt.ToString("dd/MMM/yyyy")));
                        pgCmd.Parameters.AddWithValue("@cust_id", NpgsqlDbType.Integer, payload.custid);
                        pgCmd.Parameters.AddWithValue("@so_id", NpgsqlDbType.Integer, payload.soid);
                        pgCmd.Parameters.AddWithValue("@so_gid", NpgsqlDbType.Uuid, payload.sogid);
                        pgCmd.Parameters.AddWithValue("@cash_mode", NpgsqlDbType.Boolean, payload.cashmode);
                        pgCmd.Parameters.AddWithValue("@nar_text", NpgsqlDbType.Text, payload.narrtion);
                        pgCmd.Parameters.AddWithValue("@tot_amt", NpgsqlDbType.Numeric, payload.total);
                        pgCmd.Parameters.AddWithValue("@disc_per", NpgsqlDbType.Numeric, payload.discper);
                        pgCmd.Parameters.AddWithValue("@disc_amt", NpgsqlDbType.Numeric, payload.discamt);
                        pgCmd.Parameters.AddWithValue("@cash_recamt", NpgsqlDbType.Numeric, payload.cashrecamt);
                        pgCmd.Parameters.AddWithValue("@bank_recamt", NpgsqlDbType.Numeric, payload.banhrecamt);
                        pgCmd.Parameters.AddWithValue("@uid", NpgsqlDbType.Smallint, payload.userid);
                        pgCmd.Parameters.AddWithValue("@cid", NpgsqlDbType.Smallint, payload.compid);
                        pgCmd.Parameters.Add("@saldet_json", NpgsqlDbType.Jsonb).Value = jsonstring;
                        pgCmd.Parameters.Add("@salbankdet_json", NpgsqlDbType.Jsonb).Value = jsonbankstring;

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

                            var response = JsonSerializer.Deserialize<salesResponse>(jsonResult);

                            if (response.status && response.billno.HasValue)
                            {
                                Console.WriteLine($"Inserted bill no: {response.billno.Value}");
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
        public class salesResponse
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int? billno { get; set; }
        }



        [HttpGet("getsaleview")]
        public IActionResult getsaleview(short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT salid,billno,billdt,custname,cellno,address1 as address,city,sono,case when cashmode=true then 'CASH' else 'CREDIT' end cashmode,totamt,discamt,(totamt-discamt-cashrecamt-bankrecamt) balance,h.sogid from  salhdr h,customermas c,sohdr s where  h.sogid=s.sogid and h.custid=c.custid and h.compid=@compid  order by billno desc) t"; // use your correct table

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

        [HttpGet("getsalehdrfill")]
        public IActionResult getsalehdrfill(int salid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();


                    selectQuery = "select json_agg(row_to_json(t)) FROM (select salid,billno,billdt,h.custid,custname,cellno,address1 as address,areaname,city,h.soid,sono,sodt,h.sogid,cashmode,totamt,h.discper,discamt,cashrecamt,bankrecamt,(totamt-discamt-cashrecamt-bankrecamt) balance,h.remarks from salhdr h,customervw c,sohdr s where h.sogid=s.sogid and h.custid=c.custid and h.salid=@salid and h.compid=@compid  ) t "; // use your correct table                   

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@salid", salid);
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

        [HttpGet("getsaledetailfill")]
        public IActionResult getsaledetailfill(int salid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();

                    selectQuery = "select json_agg(row_to_json(t)) FROM (select salid,d.itemid,itemcode,itemname,d.unitid,unitname,d.noofpack,d.wtperpack,qty,wt,d.rate,amount,d.sogid,sodetgid,slno,sgid,saldetgid,i.iswgtbased from saldet d,itemmaster i ,unitmaster u where d.itemid=i.itemid and d.unitid=u.unitid and d.salid=@salid order by slno) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@salid", salid);
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

        [HttpGet("getsalebankdetailfill")]
        public IActionResult getsalebankdetailfill(int salid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();

                    selectQuery = "select json_agg(row_to_json(t)) FROM (select salid,d.bankid,custname as bankname,amount,sgid from salbankdet d,customermas c  where d.bankid=c.custid and d.salid=@salid  order by custname) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@salid", salid);
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
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (SELECT * from customervw m where m.subgrpid in (2) and m.compid=@compid order by custname) t  "; // use your correct table

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

        #region  "SO Pending"

        [HttpGet("getsobillnopending")]
        public IActionResult getsobillnopending(int custid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (select p.sogid,s.soid,sono,sodt,p.custid,custname,cellno,address1 as address from sodetpendvw p,sohdr s,customermas m where p.sogid=s.sogid and p.custid=m.custid and s.iscancelled=false and (p.custid=@custid or p.custid=-1) and s.compid=@compid  group by p.sogid,s.soid,sono,sodt,p.custid,custname,cellno,address1  having (sum(qty)>0 or sum(wt)>0) order by custname desc) t  "; // use your correct table
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

 [HttpGet("getsoitempending")]
        public IActionResult getsoitempending(Guid sogid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";

                Console.WriteLine(sogid);
                Console.WriteLine(compid);

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                    string selectQuery = "select json_agg(row_to_json(t)) FROM (select * from sodetpendvw p  where p.sogid=@sogid  and p.compid=@compid   order by itemname desc) t  "; // use your correct table
                    Console.WriteLine(selectQuery);
                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);
                    pgCmd.Parameters.AddWithValue("@sogid", sogid);
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


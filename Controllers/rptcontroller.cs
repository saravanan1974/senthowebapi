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
    public class rptController : ControllerBase
    {

        private readonly string _pgConnectionString;

        public rptController(IConfiguration config)
        {
            _pgConnectionString = config.GetConnectionString("Postgres");
        }


        [HttpGet("getcustweeklyrpt")]
        public IActionResult getcustweeklyrpt(DateTime frmdt, DateTime todt, int custid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();

                    selectQuery = "select json_agg(row_to_json(t)) FROM (select p.custid,docrefid,docrefno,docrefdt,billamt,amt as balance ,custname,address1,city from acc_adjustmentpendvw p,customermas c where p.custid=c.custid and amt>0 and subgrpid in (2) and (p.custid=@custid or @custid=0) and docrefdt between @frmdt and @todt order by custname) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@frmdt", NpgsqlDbType.Date, Convert.ToDateTime(frmdt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@todt", NpgsqlDbType.Date, Convert.ToDateTime(todt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@custid", NpgsqlDbType.Integer, custid);
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

        [HttpGet("getstockrpt")]
        public IActionResult getstockrpt(DateTime frmdt, DateTime todt, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();

                    selectQuery = "select json_agg(row_to_json(t)) FROM (select itid,itemname,opwt as oldstk,salwt as delivery,(opwt-salwt) as balancestk,purwt as newstock,(opwt+purwt-salwt) as totalstk from fn_rptstockdt(@frmdt,@todt,@compid) r,itemmaster i where r.itid=i.itemid  order by itemname) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@frmdt", NpgsqlDbType.Date, Convert.ToDateTime(frmdt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@todt", NpgsqlDbType.Date, Convert.ToDateTime(todt.ToString("dd/MMM/yyyy")));
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



    [HttpGet("getordstkpendingrpt")]
        public IActionResult getordstkpendingrpt(DateTime updt,int areaid,short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
              
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                   
                   selectQuery = "select json_agg(row_to_json(t)) FROM (select s.sogid,sono,sodt,custname,areaid,areaname,cellno,areaname,itemcode,itemname,ordwt,wt from public.sodetpendvw s,customervw c,sohdr h where h.sogid=s.sogid and h.custid=c.custid and s.custid=c.custid and sodt<=@updt and (areaid=@areaid or @areaid=0) and wt>0 ) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@updt", NpgsqlDbType.Date, Convert.ToDateTime(updt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@areaid", areaid);
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


        [HttpGet("getsalesrpt")]
        public IActionResult getsalesrpt(DateTime frmdt, DateTime todt, int areaid, short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";

                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();

                    selectQuery = "select json_agg(row_to_json(t)) FROM (select  billdt, h.custid,custname,sum(wt) as totwt,sum(totamt-discamt) as billamt from  salhdr  h,saldet d, customervw c  where h.salid=d.salid and h.custid=c.custid and billdt between @frmdt and @todt and areaid=@areaid  group by h.custid,custname,billdt order by billdt,custname) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@frmdt", NpgsqlDbType.Date, Convert.ToDateTime(frmdt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@todt", NpgsqlDbType.Date, Convert.ToDateTime(todt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@areaid", areaid);
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

    [HttpGet("getreceiptrpt")]
        public IActionResult getreceiptrpt(DateTime frmdt,DateTime todt,int areaid,short compid)
        {
            try
            {
                var dt = new DataTable();
                string jsonResult = "";
                string selectQuery = "";
              
                using (var pgCon = new NpgsqlConnection(_pgConnectionString))
                {
                    pgCon.Open();
                   
                   selectQuery = "select json_agg(row_to_json(t)) FROM (select  recdt, h.custid,custname,sum(recamt) as recamt, paymode from  custcollectionvw   h, customervw c  where  h.custid=c.custid and recdt between @frmdt and @todt and areaid=@areaid  group by h.custid,custname,recdt,paymode order by recdt,custname) t  "; // use your correct table

                    Console.WriteLine(selectQuery);

                    var pgCmd = new NpgsqlCommand(selectQuery, pgCon);

                    pgCmd.Parameters.AddWithValue("@frmdt", NpgsqlDbType.Date, Convert.ToDateTime(frmdt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@todt", NpgsqlDbType.Date, Convert.ToDateTime(todt.ToString("dd/MMM/yyyy")));
                    pgCmd.Parameters.AddWithValue("@areaid", areaid);
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


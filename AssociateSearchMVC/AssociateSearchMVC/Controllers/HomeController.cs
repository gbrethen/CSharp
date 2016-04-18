using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AssociateSearchMVC.Models;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace AssociateSearchMVC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        string _AAConn = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        private SqlConnection GetAssociateSearchConnection()
        {
            string str = _AAConn;

            SqlConnection connection = new SqlConnection(str);

            if (str == null || str.Length <= 0)
                throw (new Exception("Incorrect ConnectionString to AssociateSearch Database!"));
            else
            {
                try
                {
                    connection.Open();
                }
                catch (InvalidOperationException ex)
                {
                    connection.Dispose();
                    throw new Exception(ex.Message);
                }
            }

            return connection;
        }

        public ActionResult Index(string campusName, string departmentName, string LastName, string FirstName, string MiddleName)
        {
            DataSet ds = new DataSet();
            DataSet dCampus = LoadCampuses();
            DataSet dDepartment = new DataSet();

            List<Associate> result = new List<Associate>();
            List<string> _campusName = new List<string>();
            List<string> _department = new List<string>();

            if (campusName != null)
            {
                
                ds = ListAssociates(campusName, departmentName, LastName, FirstName, MiddleName);

                int i = 0;
                foreach (DataRow dr in ds.Tables["Associate"].Rows)
                {
                    i++;
                    result.Add(new Associate() { Name = dr["FullName"].ToString(), EmployeeId = dr["EmployeeId"].ToString(), Title = dr["Title"].ToString(), Company = dr["Company"].ToString(), Phone = dr["Phone"].ToString(), Email = dr["Email"].ToString(), Department = dr["Department"].ToString(), Location = dr["Location"].ToString(), SupervisorName = dr["SupervisorName"].ToString(), RowNum = i.ToString() });
                }
            }

            foreach (DataRow dr in dCampus.Tables["Campus"].Rows)
            {
                _campusName.Add(dr["Campus"].ToString());
            }

            if (campusName != null && campusName != "")
            {
                dDepartment = dsLoadDepartments(campusName);
                foreach (DataRow dr in dDepartment.Tables["Department"].Rows)
                {
                    _department.Add(dr["Department"].ToString());
                }
            }

            ViewBag.campusName = new SelectList(_campusName);
            ViewBag.departmentName = new SelectList(_department);
            ViewBag.Msg = "Your search returned " + result.Count + " record(s)";
            ViewBag.Count = result.Count;

            return View(result);
            
        }

        public DataSet LoadCampuses()
        {
            SqlConnection ASConn = GetAssociateSearchConnection();
            const string cmdStoredProc = "spLoadCampus";

            SqlCommand dbCmd = ASConn.CreateCommand();
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.CommandText = cmdStoredProc;


            // pass parameters in, and retrieve nReturnValue from stored procedure

            try
            {
                dbCmd.Connection = ASConn;
                SqlDataAdapter da = new SqlDataAdapter(dbCmd);
                DataSet ds = new DataSet();
                da.Fill(ds, "Campus");

                return ds;
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
            finally
            {
                if (ASConn != null)
                    ASConn.Dispose();
            }

        }

        public DataSet dsLoadDepartments(string sCampus)
        {
            SqlConnection ASConn = GetAssociateSearchConnection();
            const string cmdStoredProc = "spLoadDepartment";

            SqlCommand dbCmd = ASConn.CreateCommand();
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.CommandText = cmdStoredProc;


            // pass parameters in, and retrieve nReturnValue from stored procedure
            if (sCampus == "-- Select --")
                dbCmd.Parameters.AddWithValue("@Campus", DBNull.Value);
            else
                dbCmd.Parameters.AddWithValue("@Campus", sCampus);

            try
            {
                dbCmd.Connection = ASConn;
                SqlDataAdapter da = new SqlDataAdapter(dbCmd);
                DataSet ds = new DataSet();
                da.Fill(ds, "Department");

                return ds;
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
            finally
            {
                if (ASConn != null)
                    ASConn.Dispose();
            }

        }

        [HttpPost]
        public JsonResult LoadDepartments(string sCampus)
        {
            SqlConnection ASConn = GetAssociateSearchConnection();
            const string cmdStoredProc = "spLoadDepartment";

            SqlCommand dbCmd = ASConn.CreateCommand();
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.CommandText = cmdStoredProc;


            // pass parameters in, and retrieve nReturnValue from stored procedure
            if (sCampus == "-- Select --")
                dbCmd.Parameters.AddWithValue("@Campus", DBNull.Value);
            else
                dbCmd.Parameters.AddWithValue("@Campus", sCampus);

            try
            {
                dbCmd.Connection = ASConn;
                SqlDataAdapter da = new SqlDataAdapter(dbCmd);
                DataSet ds = new DataSet();
                da.Fill(ds, "Department");

                var _department = new List<string>();
                
                foreach (DataRow dr in ds.Tables["Department"].Rows)
                {
                    _department.Add(dr["Department"].ToString());
                }
                return Json(new { ok = true, data = _department, message = "ok" });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = ex.Message });
            }
            finally
            {
                if (ASConn != null)
                    ASConn.Dispose();
            }

        }

        public DataSet ListAssociates(string Campus, string Department, string LastName, string FirstName, string MiddleName)
        {
            SqlConnection ASConn = GetAssociateSearchConnection();
            const string cmdStoredProc = "spAssociateSearch";


            SqlCommand dbCmd = ASConn.CreateCommand();
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.CommandText = cmdStoredProc;

            // pass parameters in, and retrieve nReturnValue from stored procedure
            dbCmd.Parameters.AddWithValue("@Campus", Campus);
            dbCmd.Parameters.AddWithValue("@Department", Department);
            dbCmd.Parameters.AddWithValue("@FName", FirstName);
            dbCmd.Parameters.AddWithValue("@LName", LastName);
            dbCmd.Parameters.AddWithValue("@MName", MiddleName);


            foreach (SqlParameter param in dbCmd.Parameters)
            {
                if (param.Value == null || param.Value.ToString() == "")
                    param.Value = DBNull.Value;
            }

            


            // pass parameters in, and retrieve nReturnValue from stored procedure

            try
            {
                dbCmd.Connection = ASConn;
                SqlDataAdapter da = new SqlDataAdapter(dbCmd);
                DataSet ds = new DataSet();
                da.Fill(ds, "Associate");

                return ds;
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
            finally
            {
                if (ASConn != null)
                    ASConn.Dispose();
            }

        }

    }
}

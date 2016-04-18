using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Data;
using AssociateSearchSPA.Models;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AssociateSearchSPA.Controllers
{
    public class DataServiceController : DataController
    {
        string _AAConn = System.Configuration.ConfigurationManager.AppSettings["DefaultConnection"].ToString();

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

        public IEnumerable<Associate> GetAssociates()
        {
            SqlConnection ASConn = GetAssociateSearchConnection();
            const string cmdStoredProc = "spAssociateSearch";


            SqlCommand dbCmd = ASConn.CreateCommand();
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.CommandText = cmdStoredProc;

            // pass parameters in, and retrieve nReturnValue from stored procedure
            dbCmd.Parameters.AddWithValue("@Campus", null);
            dbCmd.Parameters.AddWithValue("@Department", null);
            dbCmd.Parameters.AddWithValue("@FName", null);
            dbCmd.Parameters.AddWithValue("@LName", null);
            dbCmd.Parameters.AddWithValue("@MName", null);


            foreach (SqlParameter param in dbCmd.Parameters)
            {
                if (param.Value.ToString() == "" || param.Value.ToString() == "-- Select --" || param.Value.ToString() == "-- All --")
                    param.Value = DBNull.Value;
            }

            SqlDataAdapter da = new SqlDataAdapter(dbCmd);
            DataSet ds = new DataSet();
            da.Fill(ds, "Associate");

            List<Associate> result = new List<Associate>(from t in ds.Tables["Associate"].AsEnumerable() select t.Field<Associate>("EmployeeId")).ToList() ;
            return result;
        }

    }
}

using Marisfrolg.Fee.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Marisfrolg.Fee.BLL
{


    public class ReportHelper
    {
        OracleConnection _Conn = null;
        SqlConnection _SqlConn = null;


        public ReportHelper()
        {

        }

        public SqlConnection SqlConn
        {
            get
            {
                if (_SqlConn == null)
                {
                    _SqlConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ReportSQLSERVERDBConn"].ToString());
                }

                return _SqlConn;
            }

        }


        public OracleConnection Conn
        {
            get
            {
                if (_Conn == null)
                {
                    _Conn = new OracleConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ReportOracleDBConn"].ToString());
                }

                return _Conn;
            }
        }


        public OracleConnection Conn2
        {
            get
            {
                if (_Conn == null)
                {
                    _Conn = new OracleConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MultiBankOracleDBConn"].ToString());
                }

                return _Conn;
            }
        }


        /// <summary>
        /// 获取oracle数据
        /// </summary>
        /// <param name="SQL">SQL语句</param>
        /// <param name="ConnConfig">链接配置，1为费用配置，2为多银行系统配置</param>
        /// <returns></returns>
        public System.Data.DataTable GetDataTable(string SQL, int ConnConfig = 1)
        {

            DataTable dt = new DataTable();

            OracleConnection OraConn;

            if (ConnConfig == 1)
            {
                OraConn = Conn;
            }
            else
            {
                OraConn = Conn2;
            }

            using (OraConn)
            {

                OracleCommand cmd = new OracleCommand(SQL, OraConn);
                OracleDataAdapter da = new OracleDataAdapter();
                cmd.CommandTimeout = 60;
                da.SelectCommand = cmd;
                if (Conn.State != System.Data.ConnectionState.Open)
                {
                    Conn.Open();
                }

                da.Fill(dt);

            }

            return dt;
        }

        /// <summary>
        /// 获取SQL数据
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public System.Data.DataTable GetSQLDataTable(string SQL)
        {

            DataTable dt = new DataTable();
            using (SqlConn)
            {

                SqlCommand cmd = new SqlCommand(SQL, SqlConn);
                SqlDataAdapter da = new SqlDataAdapter();
                cmd.CommandTimeout = 60;
                da.SelectCommand = cmd;
                if (SqlConn.State != System.Data.ConnectionState.Open)
                {
                    SqlConn.Open();
                }

                da.Fill(dt);

            }

            return dt;
        }

        public static ReportModel ConvertDataTable(DataTable dt)
        {
            ReportModel result = new ReportModel();
            result.Columns = new List<string>();
            result.Rows = new List<object[]>();
            foreach (DataColumn column in dt.Columns)
            {
                result.Columns.Add(column.ColumnName);
            }
            foreach (DataRow row in dt.Rows)
            {
                result.Rows.Add(row.ItemArray);
            }
            return result;
        }

        #region 执行事务

        #region 批量执行 含事务
        /// <summary>
        /// 批量执行 含事务
        /// </summary>
        /// <param name="SqlList">Sql 列表</param>
        /// <returns>True 成功 False 失败</returns>
        public bool ExecuteSqlTran(ArrayList SqlList)
        {

            using (Conn)
            {
                if (Conn.State != System.Data.ConnectionState.Open)
                {
                    Conn.Open();
                }
                OracleTransaction MyTran = Conn.BeginTransaction();
                OracleCommand MyComm = Conn.CreateCommand();

                try
                {
                    MyComm.CommandTimeout = 60;
                    MyComm.Transaction = MyTran;

                    foreach (string Sql in SqlList)
                    {
                        MyComm.CommandText = Sql;
                        MyComm.ExecuteNonQuery();
                    }
                    MyTran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    MyTran.Rollback();
                    throw ex;
                }
                finally
                {
                    Conn.Close();
                    MyComm.Dispose();
                }
            }
        }
        #endregion

        #endregion
    }
}
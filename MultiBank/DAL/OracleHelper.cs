using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MultiBank.DAL
{
    public class OracleHelper
    {
        protected OracleConnection Connection;
        private string connectionString;

        public OracleHelper()
        {
            string connStr;
            connStr = System.Configuration.ConfigurationManager.ConnectionStrings["OraConnection"].ToString();
            connectionString = connStr;
            Connection = new OracleConnection(connectionString);
        }

        public OracleHelper(string ConnString)
        {
            string connStr;
            connStr = System.Configuration.ConfigurationManager.ConnectionStrings[ConnString].ToString();
            Connection = new OracleConnection(connStr);
        }


        public void OpenConn()
        {
            if (this.Connection.State != ConnectionState.Open)
                this.Connection.Open();
        }

        public void CloseConn()
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }


        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            OpenConn();
            OracleDataAdapter OraDA = new OracleDataAdapter(sql, Connection);
            OraDA.Fill(dt);
            CloseConn();
            return dt;
        }

        /// <summary>
        /// 分页返回dataset
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="PageSize"></param>
        /// <param name="CurrPageIndex"></param>
        /// <param name="DataSetName"></param>
        /// <returns></returns>
        public DataSet ReturnDataSet(string sql, int PageSize, int CurrPageIndex, string DataSetName)
        {
            DataSet dataSet = new DataSet();
            OpenConn();
            OracleDataAdapter OraDA = new OracleDataAdapter(sql, Connection);
            OraDA.Fill(dataSet, PageSize * (CurrPageIndex - 1), PageSize, DataSetName);
            CloseConn();
            return dataSet;
        }

        /// <summary>
        /// 返回记录总条数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int GetRecordCount(string sql)
        {
            int recordCount = 0;
            OpenConn();
            OracleCommand command = new OracleCommand(sql, Connection);
            OracleDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                recordCount++;
            }
            dataReader.Close();
            CloseConn();
            return recordCount;
        }

        /// <summary>
        /// 返回受影响行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteSQL(string sql)
        {
            int Cmd = 0;
            OpenConn();
            OracleCommand command = new OracleCommand(sql, Connection);
            try
            {
                Cmd = command.ExecuteNonQuery();
            }
            catch
            {

            }
            finally
            {
                CloseConn();
            }

            return Cmd;
        }

        /// <summary>
        /// 批量事物化执行SQL
        /// </summary>
        /// <param name="SqlList"></param>
        /// <returns></returns>
        public bool ExecuteSqlTran(ArrayList SqlList)
        {

            using (Connection)
            {
                OpenConn();
                OracleTransaction MyTran = Connection.BeginTransaction();
                OracleCommand MyComm = Connection.CreateCommand();

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
                    Connection.Close();
                    MyComm.Dispose();
                }
            }
        }

        /// <summary>
        /// 反射返回list集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public IList<T> DataTableToList<T>(DataTable table)
        {
            IList<T> list = new List<T>();
            T t = default(T);
            PropertyInfo[] propertypes = null;
            string tempName = string.Empty;
            foreach (DataRow row in table.Rows)
            {
                t = Activator.CreateInstance<T>();
                propertypes = t.GetType().GetProperties();
                foreach (PropertyInfo pro in propertypes)
                {
                    tempName = pro.Name;
                    if (table.Columns.Contains(tempName))
                    {
                        object value = IsNullOrDBNull(row[tempName]) ? null : row[tempName];
                        pro.SetValue(t, value, null);
                    }
                }
                list.Add(t);
            }
            return list;
        }

        static bool IsNullOrDBNull(object data)
        {
            if (data == null)
            {
                return true;
            }
            if (data == System.DBNull.Value)
            {
                return true;
            }
            return false;
        }
    }
}
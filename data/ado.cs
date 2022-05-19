using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace rfidWebservice.data
{
    public class ado
    {
        private SqlConnection sql_conn;
        private SqlCommand sql_cmd;
        private SqlDataAdapter sql_da;

        public ado()
        {
            sql_conn = new SqlConnection(SysParam.mssql);
            sql_cmd = new SqlCommand("", sql_conn);
            sql_da = new SqlDataAdapter(sql_cmd);
        }

        public int ExecuteNonQuery(string sqlText)
        {
            int ret = -1;
            sql_cmd.CommandText = sqlText;

            try
            {
                if (sql_conn.State == System.Data.ConnectionState.Closed)
                    sql_conn.Open();

                ret = sql_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ret = -1;
                pub.WriteErrLog("Modul Name: dao.ExecuteNonQuery(sqlText). " + "\r\n" + "SqlCommand: " + sqlText + ". " + "\r\n" + "Message: " + ex.Message + ". ");
            }
            finally
            {
                sql_conn.Close();
            }

            return ret;
        }

        public int ExecuteQuery(string sqlText, out System.Data.DataTable dt)
        {
            int ret = -1;
            sql_cmd.CommandText = sqlText;
            sql_da = new SqlDataAdapter(sql_cmd);
            dt = new System.Data.DataTable("returnDataTable");

            try
            {
                sql_da.Fill(dt);
                ret = dt.Rows.Count;
            }
            catch (Exception ex)
            {
                ret = -1;
                pub.WriteErrLog("Modul Name: dao.ExecuteQuery. " + "\r\n" + "SqlCommand: " + sqlText + ". " + "\r\n" + "Message: " + ex.Message + ". ");
            }

            return ret;
        }

        public int ExecuteNonQuery(List<string> List_sqlText)
        {
            int ret = -1;
            string errSql = "";

            if (sql_conn.State == System.Data.ConnectionState.Closed)
            {
                sql_conn.Open();
            }

            SqlTransaction trans = sql_conn.BeginTransaction();

            try
            {
                sql_cmd.Transaction = trans;
                ret = 0;

                foreach (string sqlText in List_sqlText)
                {
                    errSql = sqlText;
                    sql_cmd.CommandText = sqlText;
                    ret += sql_cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                pub.WriteErrLog("Error : " + ret);
                pub.WriteErrLog("ErrSQLstr：" + errSql);
                ret = -1;
                pub.WriteErrLog("Modul Name: dao.ExecuteNonQuery(List_sqlText). " + "\r\n" + "Message: " + ex.Message + ". ");
                trans.Rollback();
            }
            finally
            {
                trans.Dispose();
                sql_conn.Close();
            }

            return ret;
        }

        public int ExecuteNonQuery(string sqlText, List<Sqlparameter> parameter)
        {
            int ret = -1;
            sql_cmd.CommandText = sqlText;
            sql_cmd.Parameters.Clear();

            try
            {
                if (parameter.Count > 0)
                {
                    foreach (Sqlparameter parm in parameter)
                        sql_cmd.Parameters.AddWithValue(parm.parameter_name, parm.parameter_value);
                }

                if (sql_conn.State == System.Data.ConnectionState.Closed)
                {
                    sql_conn.Open();
                }

                ret = sql_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ret = -1;
                pub.WriteErrLog("Modul Name: dao.ExecuteNonQuery(sqlText, parameter). " + "\r\n" + "SqlCommand: " + sqlText + ". " + "\r\n" + "Message: " + ex.Message + ". ");
            }
            finally
            {
                sql_conn.Close();
            }

            return ret;
        }
        public int ExecuteQuery(string sqlText, List<Sqlparameter> parameter, out System.Data.DataTable dt)
        {
            int ret = -1;
            sql_cmd.CommandText = sqlText;
            dt = new System.Data.DataTable("returnDataTable");

            try
            {
                if (parameter.Count > 0)
                {
                    foreach (Sqlparameter parm in parameter)
                        sql_cmd.Parameters.AddWithValue(parm.parameter_name, parm.parameter_value);
                }

                sql_da = new SqlDataAdapter(sql_cmd);

                sql_da.Fill(dt);
                ret = dt.Rows.Count;
            }
            catch (Exception ex)
            {
                ret = -1;
                pub.WriteErrLog("Modul Name: dao.ExecuteQuery(sqlText, parameter). " + "\r\n" + "SqlCommand: " + sqlText + ". " + "\r\n" + "Message: " + ex.Message + ". ");
            }

            return ret;
        }
    }

    public class Sqlparameter
    {
        public string parameter_name { get; set; }
        public object parameter_value { get; set; }
    }
}

using System.Data;
using System.Data.SQLite;
using System.Data.Common;
using System.Collections.Generic;

namespace SuperDataBase
{
    /// <summary>
    /// 类 名:SqLiteHelper
    /// 版 本:Release
    /// 日 期:2015-06-25
    /// 作 者:不良帥
    /// 描 述:SQLite数据库辅助类
    /// </summary>
    public static class DBSqLiteHelper
    {
        /// <summary>
        /// 创建链接字符串
        /// </summary>
        public static string strConn = string.Empty;

        #region   执行Sql语句，增删改 
        /// <summary>
        /// 执行Sql语句，增删改
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parms">参数</param>
        /// <returns>影响行数</returns>
        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parms)
        {
            if (string.IsNullOrEmpty(sql))
                return -1;
            using (SQLiteConnection conn = new(strConn))
            {
                using (SQLiteCommand cmd = new(sql, conn))
                {
                    if (parms != null)
                    {
                        cmd.Parameters.AddRange(parms);
                    }
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region  执行Sql语句，1个返回值 

        /// <summary>
        /// 查询方法
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parms">sql参数</param>
        /// <returns> 返回第一行第一列数据 </returns>
        public static object ExecuteScalar(string sql, params SQLiteParameter[] parms)
        {
            if (string.IsNullOrEmpty(sql))
                return -1;
            using (SQLiteConnection conn = new(strConn))
            {
                using (SQLiteCommand cmd = new(sql, conn))
                {
                    if (parms != null)
                    {
                        cmd.Parameters.AddRange(parms);
                    }
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }
        #endregion

        #region  执行sql语句，返回结果集 
        /// <summary>
        /// 执行sql语句，返回结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parms">参数</param>
        /// <returns></returns>
        public static SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parms)
        {
            using (SQLiteConnection conn = new(strConn))
            {
                using (SQLiteCommand cmd = new(sql, conn))
                {
                    if (parms != null)
                    {
                        cmd.Parameters.AddRange(parms);
                    }
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open(); //非打开状态时,打开数据库
                    }
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);//当SQLiteDataReader释放时,释放连接
                }
            }
        }
        #endregion

        #region  多语句事务 
        /// <summary>
        /// 多语句事务
        /// </summary>
        /// <param name="sql">sql对象集合</param>
        /// <returns>提交事务返回true，否则回滚</returns>
        public static bool ExecTransaction(List<SqlObject> sql)
        {
            if (sql == null)
                return false;
            using (SQLiteConnection conn = new(strConn))
            {
                using (DbTransaction transaction = conn.BeginTransaction())
                {
                    int o = 0;
                    using (SQLiteCommand command = new(conn))
                    {
                        for (int i = 0; i < sql.Count; i++)
                        {
                            command.CommandText = sql[i].sqlText;
                            if (sql[i].paramList != null)
                            {
                                command.Parameters.AddRange(sql[i].paramList.ToArray());
                            }
                        }
                        o += (int)command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        if (o >= 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                }
            }
        }
        #endregion

        #region  参数类 
        /// <summary>
        /// 参数
        /// </summary>
        public class SqlObject
        {
            /// <summary>
            /// sql语句
            /// </summary>
            public string sqlText { get; set; }
            /// <summary>
            /// 参数列表
            /// </summary>
            public List<SQLiteParameter> paramList = new();
        }
        #endregion
    }
}

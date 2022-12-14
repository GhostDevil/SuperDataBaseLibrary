using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace SuperDataBase
{

    /// <summary>
    /// 通用数据库访问类 DBHelper类
    /// 不同类型的数据源
    /// </summary>
    public class DBHelper : IDisposable
    {
        /// <summary>
        /// 获取access文件密码
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetAccessPassword(string file)
        {
            byte[] array = new byte[16] {190,236,101,156,254,40,43,138,108,123,205,223,79,19,247,177 };
            byte b = 12;
            string text = "";
            try
            {
                FileStream fileStream = File.OpenRead(file);
                fileStream.Seek(20L, SeekOrigin.Begin);
                byte b2 = (byte)fileStream.ReadByte();
                fileStream.Seek(66L, SeekOrigin.Begin);
                byte[] array2 = new byte[33];
                if (fileStream.Read(array2, 0, 33) != 33)
                {
                    return "";
                }
                byte b3 = (byte)(array2[32] ^ b);
                for (int i = 0; i < 16; i++)
                {
                    byte b4 = (byte)(array[i] ^ array2[i * 2]);
                    if (i % 2 == 0 && b2 == 1)
                    {
                        b4 = (byte)(b4 ^ b3);
                    }
                    if (b4 > 0)
                    {
                        string str = text;
                        char c = (char)b4;
                        text = str + c.ToString();
                    }
                }
                return text;
            }
            catch
            {
                return text;
            }
        }

        #region 初始化
        private DbProviderFactory provider = null;
        /// <summary>
        /// 获取数据库类型属性
        /// </summary>
        public DBType DatabaseType { get; } = DBType.SQLServer;
        /// <summary>
        /// 获取数据库连接字符串属性
        /// </summary>
        public string ConnectionString { get; } = "";
        /// <summary>
        /// 命令超时时间
        /// </summary>
        public int CommandTimeout { get; } = 60;
        /// <summary>
        /// 是否开启调试日志
        /// </summary>
        public bool EnableDebugLog { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ConString">数据库连接字符串</param> 
        /// <param name="DbType">访问的数据库类型</param>
        public DBHelper(string ConString, DBType DbType)
        {
            DatabaseType = DbType;
            ConnectionString = ConString;
            CommandTimeout = CommandTimeout;
            GetProvider();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConString"></param>
        /// <param name="DbType"></param> 
        /// <param name="CommandTimeout"></param>
        public DBHelper(string ConString, DBType DbType, int CommandTimeout)
        {
            DatabaseType = DbType;
            ConnectionString = ConString;
            this.CommandTimeout = CommandTimeout;
            GetProvider();
        }


        /// <summary>
        /// 根据数据库类型获取数据库实例
        /// </summary>
        /// <returns></returns>
        private void GetProvider()
        {
            switch (DatabaseType)
            {
                case DBType.SQLServer:
                    provider = SqlClientFactory.Instance;
                    break;
                case DBType.OleDb:
                    provider = OleDbFactory.Instance;
                    break;
                case DBType.ODBC:
                    provider = OdbcFactory.Instance;
                    break;
                case DBType.Oracle:
                    provider = OracleClientFactory.Instance;
                    break;
                case DBType.MySQL:
                    provider = MySqlClientFactory.Instance;
                    break;
                case DBType.SQLite:
                    provider = SQLiteFactory.Instance;
                    break;
                case DBType.Firebird:
                    provider = FirebirdClientFactory.Instance;
                    break;
            }
        }
        #endregion

        #region  ExecuteNonQuery
        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, null);
        }

        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText, CommandType cmdType)
        {
            return ExecuteNonQuery(cmdText, cmdType, null);
        }

        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText, Dictionary<string, object> ParametersList)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, ParametersList);
        }


        /// <summary>
        /// 执行SQL语句并返回受影响的行的数目
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>受影响的行的数目</returns>
        public int ExecuteNonQuery(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                try
                {
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, cmdText, ParametersList);
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        #endregion

        #region ExecuteReader
        /// <summary>
        /// 执行SQL语句并返回数据行  Reader.Close()
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText)
        {
            return ExecuteReader(cmdText, CommandType.Text, null);
        }
        /// <summary>
        /// 执行SQL语句并返回数据行  Reader.Close()
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType)
        {
            return ExecuteReader(cmdText, cmdType, null);
        }

        /// <summary>
        /// 执行SQL语句并返回数据行
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText, Dictionary<string, object> ParametersList)
        {
            return ExecuteReader(cmdText, CommandType.Text, ParametersList);
        }

        /// <summary>
        /// 执行SQL语句并返回数据行 ,执行后必须关闭  Reader.Close()
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据读取器接口</returns>
        public IDataReader ExecuteReader(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {

            DbConnection conn = provider.CreateConnection();
            var cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
            try
            {
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, cmdText, ParametersList);
                throw new Exception(ex.Message, ex);
            }


        }

        #endregion

        #region GetOneResult
        /// <summary>
        /// 执行SQL语句并返回单值对象
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public object GetOneResult(string cmdText)
        {
            return GetOneResult(cmdText, CommandType.Text, null);
        }

        /// <summary>
        /// 执行SQL语句并返回单值
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public string GetOneString(string cmdText)
        {
            string str = "";
            var res = GetOneResult(cmdText, CommandType.Text, null);
            if (res != null) { str = res.ToString(); }
            return str;
        }


        /// <summary>
        /// 执行SQL语句并返回单值对象
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public object GetOneResult(string cmdText, Dictionary<string, object> ParametersList)
        {
            return GetOneResult(cmdText, CommandType.Text, ParametersList);
        }
        /// <summary>
        /// 执行SQL语句并返回单值
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="dc">数据库命令字符串</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public string GetOneString(string cmdText, Dictionary<string, object> dc)
        {
            string str = "";
            var res = GetOneResult(cmdText, dc);
            if (res != null)
            { str = res.ToString(); }
            return str;
        }


        /// <summary>
        /// 执行SQL语句并返回单值对象
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public object GetOneResult(string cmdText, CommandType cmdType)
        {
            return GetOneResult(cmdText, cmdType, null);
        }

        /// <summary>
        /// 执行SQL语句并返回单值
        /// 即结果集中第一行的第一条数据
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>单值对象－结果集中第一行的第一条数据</returns>
        public string GetOneString(string cmdText, CommandType cmdType)
        {
            string str = "";
            var res = GetOneResult(cmdText, cmdType, null);
            if (res != null) { str = res.ToString(); }
            return str;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="ParametersList"></param>
        /// <returns></returns>
        public object GetOneResult(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {

            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                try
                {
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, cmdText, ParametersList);
                    throw new Exception(ex.Message, ex);
                }

            }
        }

        #endregion

        #region GetDataSet
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据集对象</returns>
        public DataSet GetDataSet(string cmdText)
        {
            return GetDataSet(cmdText, CommandType.Text, null);
        }
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据集对象</returns>
        public DataSet GetDataSet(string cmdText, CommandType cmdType)
        {
            return GetDataSet(cmdText, cmdType, null);
        }
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据集对象</returns>
        public DataSet GetDataSet(string cmdText, Dictionary<string, object> ParametersList)
        {
            return GetDataSet(cmdText, CommandType.Text, ParametersList);
        }
        /// <summary>
        /// 填充一个数据集对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据集对象</returns>
        private DataSet GetDataSet(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            DataSet ds = new();
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                using (DbDataAdapter da = provider.CreateDataAdapter())
                {
                    da.SelectCommand = cmd;
                    try
                    {
                        da.Fill(ds);
                        return ds;
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog(ex, cmdText, ParametersList);
                        throw new Exception(ex.Message, ex);
                    }
                }
            }
        }

        #endregion

        #region GetDataTable
        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText)
        {
            return GetDataTable(cmdText, CommandType.Text, null);
        }

        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText, CommandType cmdType)
        {
            return GetDataTable(cmdText, cmdType, null);
        }

        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText, Dictionary<string, object> ParametersList)
        {
            return GetDataTable(cmdText, CommandType.Text, ParametersList);
        }
        /// <summary>
        /// 填充一个数据表对象并返回之
        /// </summary>
        /// <param name="cmdText">数据库命令字符串</param>
        /// <param name="cmdType">命令执行方式</param>
        /// <param name="ParametersList">参数</param>
        /// <returns>数据表对象</returns>
        public DataTable GetDataTable(string cmdText, CommandType cmdType, Dictionary<string, object> ParametersList)
        {
            DataTable dt = new();
            using (DbConnection conn = provider.CreateConnection())
            {
                var cmd = conn.CreateCommand();
                PrepareCommand(conn, cmd, cmdText, cmdType, ParametersList);
                using (DbDataAdapter da = provider.CreateDataAdapter())
                {
                    da.SelectCommand = cmd;
                    try
                    {
                        da.Fill(dt);
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog(ex, cmdText, ParametersList);
                        throw new Exception(ex.Message, ex);
                    }
                }
            }
        }

        #endregion

        #region ExecuteProcedure


        /// <summary>
        /// 执行带返回参数的存储过程
        /// 各种数据库类型使用各自的Parameter， 
        /// MySqlParameter,SqlParameter,OracleParameter,OleDbParameter,SQLiteParameter等
        /// 需要using 各自的实例 
        /// </summary>
        /// <param name="ProcedureName">存储过程名</param>
        /// <param name="ParametersList">参数列表</param>
        /// <returns></returns>
        public int ExecuteProcedure(string ProcedureName, DbParameter[] ParametersList)
        {
            using (DbConnection conn = provider.CreateConnection())
            {
                WriteDebugLog(ProcedureName, ParametersList);

                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = ProcedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                if (ParametersList != null)
                {
                    cmd.Parameters.AddRange(ParametersList);
                }

                try
                {
                    int val = cmd.ExecuteNonQuery();
                    return val;
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, ProcedureName, ParametersList);
                    throw new Exception(ex.Message, ex);
                }
            }
        }




        #endregion

        #region GetProcedureDataTable


        /// <summary>
        /// 执行带返回参数的存储过程
        /// </summary>
        /// <param name="ProcedureName">存储过程名</param>
        /// <param name="ParametersList">参数列表</param>
        /// <returns></returns>
        private DataTable GetProcedureDataTable(string ProcedureName, DbParameter[] ParametersList)
        {
            return GetProcedureDataSet(ProcedureName, ParametersList).Tables[0];
        }

        #endregion

        #region GetProcedureDataSet


        /// <summary>
        /// 执行带返回参数的存储过程
        /// </summary>
        /// <param name="ProcedureName">存储过程名</param>
        /// <param name="ParametersList">参数列表</param>
        /// <returns></returns>
        private DataSet GetProcedureDataSet(string ProcedureName, DbParameter[] ParametersList)
        {
            using (DbConnection conn = provider.CreateConnection())
            {
                WriteDebugLog(ProcedureName, ParametersList);

                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = ProcedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                if (ParametersList != null)
                {
                    cmd.Parameters.AddRange(ParametersList);
                }

                using (DbDataAdapter da = provider.CreateDataAdapter())
                {
                    da.SelectCommand = cmd;
                    DataSet ds = new();
                    try
                    {
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog(ex, ProcedureName, ParametersList);
                        throw new Exception(ex.Message, ex);
                    }
                    return ds;
                }
            }
        }


        #endregion

        #region 执行多条Sql命令 事务 
        /// <summary>
        /// 执行多条Sql命令 事务 
        /// </summary>
        /// <param name="cmdTextList">Sql命令数组</param>
        /// <returns>正确执行返回True，错误执行为False,默认为false</returns>
        public void ExecuteSqlList(List<string> cmdTextList)
        {
            using (DbConnection conn = provider.CreateConnection())
            {
                WriteDebugLog(cmdTextList);

                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = CommandType.Text;

                var tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    foreach (string sql in cmdTextList)
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception(ex.Message, ex);
                }

            }
        }


        /// <summary>
        /// 执行多条Sql命令 事务 
        /// </summary>
        /// <param name="cmdTextList"></param>
        /// <param name="UseTransaction"></param>
        public void ExecuteSqlList(List<string> cmdTextList, bool UseTransaction)
        {
            using (DbConnection conn = provider.CreateConnection())
            {
                WriteDebugLog(cmdTextList);

                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = CommandType.Text;
                if (UseTransaction)
                {
                    var tx = conn.BeginTransaction();
                    cmd.Transaction = tx;
                    try
                    {
                        foreach (string sql in cmdTextList)
                        {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        throw new Exception(ex.Message, ex);
                    }
                }
                else
                {
                    try
                    {
                        foreach (string sql in cmdTextList)
                        {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message, ex);
                    }

                }

            }
        }
        #endregion

        #region 执行多条Sql命令带参数 事务
        /// <summary>
        /// 执行多条Sql命令带参数 事务
        /// </summary>
        /// <returns></returns>
        public void ExecuteTransaction(TranList TL)
        {
            if (TL == null || TL.GetNumEntries() == 0)
            {
                throw new Exception("SQL事物为空或0行");
            }
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = CommandType.Text;
                var tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    foreach (KeyValuePair<string, Dictionary<string, object>> kvp in TL)
                    {
                        cmd.CommandText = kvp.Key;
                        if (kvp.Value != null)
                        {
                            Dictionary<string, object> dc = kvp.Value as Dictionary<string, object>;
                            foreach (KeyValuePair<string, object> _param in dc)
                            {
                                var p = cmd.CreateParameter();
                                p.ParameterName = _param.Key;
                                p.Value = _param.Value;
                                cmd.Parameters.Add(p);
                            }
                        }
                        WriteDebugLog(kvp.Key, kvp.Value);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 执行多条Sql命令带参数 事务
        /// </summary>
        /// <returns></returns>
        public void ExecuteTransaction(TranList TL, ref int count)
        {
            if (TL == null || TL.GetNumEntries() == 0)
            {
                throw new Exception("SQL事物为空或0行");
            }
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = CommandType.Text;
                var tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    foreach (KeyValuePair<string, Dictionary<string, object>> kvp in TL)
                    {
                        cmd.CommandText = kvp.Key;
                        if (kvp.Value != null)
                        {
                            Dictionary<string, object> dc = kvp.Value as Dictionary<string, object>;
                            foreach (KeyValuePair<string, object> _param in dc)
                            {
                                var p = cmd.CreateParameter();
                                p.ParameterName = _param.Key;
                                p.Value = _param.Value;
                                cmd.Parameters.Add(p);
                            }
                        }
                        WriteDebugLog(kvp.Key, kvp.Value);
                        count += cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();                   
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion

        #region 批量处理sql元素

        public void SqlBulkInsert(string _tableName, DataTable dtSource)
        {

            if (DatabaseType == DBType.SQLServer)
            {
                using (SqlBulkCopy sqlbc = new(ConnectionString))
                {
                    sqlbc.BatchSize = 50000;
                    sqlbc.BulkCopyTimeout = 60;
                    sqlbc.DestinationTableName = _tableName;

                    //添加ColumnMappings , 不然默认是使用Index 排列，容易插错
                    for (int i = 0; i < dtSource.Columns.Count; i++)
                    {
                        sqlbc.ColumnMappings.Add(
                            dtSource.Columns[i].ColumnName, dtSource.Columns[i].ColumnName
                            );
                    }

                    try
                    {
                        sqlbc.WriteToServer(dtSource);
                    }
                    catch (Exception ex)
                    {
                        
                        throw new Exception(ex.Message, ex);
                    }
                }
            }
            else if (DatabaseType == DBType.MySQL)
            {
                if (string.IsNullOrEmpty(_tableName)) { throw new Exception("表名不能为空"); }
                if (dtSource.Rows.Count == 0) { throw new Exception("数据源Table为0行"); }

                int insertCount = 0;

                string csv_file_name = DataTableToCsv(dtSource);

                using (MySqlConnection conn = new(ConnectionString))
                {
                    MySqlTransaction tran = null;
                    try
                    {
                        conn.Open();
                        tran = conn.BeginTransaction();
                        MySqlBulkLoader bulk = new(conn)
                        {
                            FieldTerminator = "‖",
                            LineTerminator = "≡",
                            FileName = csv_file_name,
                            NumberOfLinesToSkip = 0,
                            TableName = _tableName,

                        };
                        bulk.Columns.AddRange(
                            dtSource.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList()
                            );
                        insertCount = bulk.Load();
                        tran.Commit();
                    }
                    catch (MySqlException ex)
                    {
                        if (tran != null) tran.Rollback();
                        throw ex;
                    }
                }

            }
            else
            {
                { throw new Exception("暂不支持" + DatabaseType + "数据库类型"); }
            }

        }
        #endregion

        #region 将DataTable转换为标准的CSV,写入到临时文件

        /// <summary>
        ///将DataTable转换为标准的CSV,写入到临时文件
        /// </summary>
        /// <param name="table">数据表</param>
        /// <returns>返回标准的CSV,文件路径</returns>
        private string DataTableToCsv(DataTable table)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。
            StringBuilder sb = new();
            DataColumn colum;

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append("‖");
                    if (colum.DataType == typeof(string) && row[colum].ToString().Contains("‖"))
                    {
                        sb.Append(row[colum].ToString().Replace("‖", "∥"));
                    }
                    else if (colum.DataType == typeof(string) && row[colum].ToString().Contains("≡"))
                    {
                        sb.Append(row[colum].ToString().Replace("≡", "〓"));
                    }
                    else { sb.Append(row[colum].ToString()); }
                }
                sb.Append("≡");
            }

            string temp_path = Path.GetTempFileName();
            File.WriteAllText(temp_path, sb.ToString());
            return temp_path;
        }
        #endregion

        #region PrepareCommand
        /// <summary>
        /// 准备执行命令
        /// </summary>
        /// <param name="conn">DbConnection</param>
        /// <param name="cmd">DbCommand</param>
        /// <param name="cmdText">sql命令字符串</param>
        /// <param name="cmdType">执行方式</param>
        /// <param name="ParametersList">参数</param>
        private void PrepareCommand(DbConnection conn, DbCommand cmd, string cmdText, CommandType cmdType,
            Dictionary<string, object> ParametersList)
        {

            WriteDebugLog(cmdText, ParametersList);

            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = CommandTimeout;
            if (ParametersList != null)
            {
                foreach (var p in ParametersList)
                {
                    var dp = cmd.CreateParameter();
                    dp.ParameterName = p.Key;
                    dp.Value = p.Value;
                    cmd.Parameters.Add(dp);
                }
            }

            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = ConnectionString;
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (conn.State == ConnectionState.Broken)
            {
                try
                {
                    conn.Close();
                    conn.Open();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        #endregion

        #region  ErrorLog
        private void WriteErrorLog(Exception ex, string cmdText,
            Dictionary<string, object> ParametersList)
        {
            StringBuilder sblog = new();
            sblog.Append(ex.Message + System.Environment.NewLine);
            sblog.Append("CommandText: " + cmdText + System.Environment.NewLine);
            if (ParametersList != null)
            {
                foreach (var item in ParametersList)
                {
                    string paraName = item.Key;
                    string paraValue = item.Value == null ? "" : item.Value.ToString();
                    sblog.Append("Params: " + paraName + " = " + paraValue + System.Environment.NewLine);
                }
            }
            
        }
        private void WriteErrorLog(Exception ex, string cmdText,
            DbParameter[] ParametersList)
        {
            StringBuilder sblog = new();
            sblog.Append(ex.Message + System.Environment.NewLine);
            sblog.Append("CommandText: " + cmdText + System.Environment.NewLine);
            if (ParametersList != null)
            {
                foreach (var item in ParametersList)
                {
                    string paraName = item.ParameterName;
                    string paraValue = item.Value == null ? "" : item.Value.ToString();
                    sblog.Append("Params: " + paraName + " = " + paraValue + System.Environment.NewLine);
                }
            }
            
        }
        #endregion

        #region DebugLog
        private void WriteDebugLog(List<string> sqllist)
        {
            if (EnableDebugLog)
            {
                StringBuilder sblog = new();
                foreach (var item in sqllist)
                {
                    sblog.Append(item + "\r\n");
                }
                

            }
        }
        private void WriteDebugLog(string cmdText,
            Dictionary<string, object> ParametersList)
        {
            if (EnableDebugLog)
            {
                StringBuilder sblog = new();
                sblog.Append("CommandText:\r\n" + cmdText + System.Environment.NewLine);
                if (ParametersList != null)
                {
                    foreach (var item in ParametersList)
                    {
                        string paraName = item.Key;
                        string paraValue = item.Value == null ? "" : item.Value.ToString();
                        sblog.Append("Params: " + paraName + " = " + paraValue + System.Environment.NewLine);
                    }
                }
                
            }
        }
        private void WriteDebugLog(string cmdText,
            DbParameter[] ParametersList)
        {
            if (EnableDebugLog)
            {
                StringBuilder sblog = new();
                sblog.Append("CommandText:\r\n" + cmdText + System.Environment.NewLine);
                if (ParametersList != null)
                {
                    foreach (var item in ParametersList)
                    {
                        string paraName = item.ParameterName;
                        string paraValue = item.Value == null ? "" : item.Value.ToString();
                        sblog.Append("Params: " + paraName + " = " + paraValue + System.Environment.NewLine);
                    }
                }
                
            }
        }
        #endregion

        #region  Dispose
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region 内部类
        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum DBType
        {
            /// <summary>
            /// SQL Server string connStr = "server= 127.0.0.1;uid=sa;pwd=sa;database=MyDB"
            /// </summary>
            SQLServer,

            /// <summary>
            /// Access string connStr = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=E:\数据库\My.mdb;"
            /// </summary>
            OleDb,

            /// <summary>
            /// ODBC数据源 string connStr = "DSN=XXX;uid=sa;pwd=sa;"
            /// </summary>
            ODBC,

            /// <summary>
            /// Oracle  string connStr = "Data Source=192.168.1.10/orcl;User Id=sixin;Password=sixin;";
            /// 注意 Oracle 参数化需要符号:  不是@
            /// </summary>
            Oracle, //不常用 使用时取消注释 并引用 Oracle.ManagedDataAccess.dll

            /// <summary>
            /// MySQL    string connStr = "server= 127.0.0.1;uid=root;pwd=root;database=MyDB;allow user variables=true;"
            /// </summary>
            MySQL,

            /// <summary> 
            /// SQLite  string connStr = "Data Source={0};";
            /// 带密码 += Password=myPassword;
            /// </summary>
            SQLite,

            /// <summary>
            /// 
            /// </summary>
            Firebird

        }



        /// <summary>
        /// SQL带参数列表，用于事务等
        /// </summary>
        public class TranList
        {
            private List<KeyValuePair<string, Dictionary<string, object>>> strArray;
            //控件的构造函数
            public TranList()
            {
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="cmdtxt"></param>
            /// <param name="dc"></param>
            public void Add(string cmdtxt, Dictionary<string, object> dc)
            {
                var kvp = new KeyValuePair<string, Dictionary<string, object>>(cmdtxt, dc);
                if (strArray == null)
                {
                    strArray = new List<KeyValuePair<string, Dictionary<string, object>>>();
                }
                strArray.Add(kvp);
            }
            //索引器的定义,有些类似属性的调用
            public KeyValuePair<string, Dictionary<string, object>> this[int index]
            {
                get
                {
                    return strArray[index];
                }
                set
                {
                    strArray[index] = value;
                }
            }
            //为TranList的foreach定义GetEnumerator()方法，便于迭代        
            public IEnumerator GetEnumerator()
            {
                foreach (var item in strArray)
                {
                    yield return item;
                }

            }
            //用于返回已存储的数组容量
            public int GetNumEntries()
            {
                return strArray == null ? 0 : strArray.Count;
            }

            public int Count
            {
                get { return GetNumEntries(); }
            }
        }
        #endregion
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Data;

namespace SuperDataBase
{
    /// <summary>
    /// 类 名:MySqlHelper
    /// 版 本:Release
    /// 日 期:2015-06-25
    /// 作 者:不良帥
    /// 描 述:MySql数据库辅助类
    /// </summary>
    public sealed class DBMySqlHelper
    {
        //Get the database connectionstring, which are static variables and readonly, all project documents can be used directly, but can not modify it 
        //the database connectionString 
        //public static readonly string connectionStringManager = ConfigurationManager.ConnectionStrings["MYSQLConnectionString"].ConnectionString;
        /// <summary>
        /// 设置数据库连接字符串
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        public static void SetConnectionString(string connStr)
        {
            connectionStringManager = connStr;
        }



        //This connectionString for the local test
        static string connectionStringManager = string.Empty; //System.Configuration.ConfigurationManager.AppSettings["MySQLConnString"];
        //ConfigurationManager.ConnectionStrings["MySQLConnString"].ConnectionString;

       //哈希表存储的参数信息，哈希表可以存储任何类型的参数
       //这里的哈希表是静态变量的静态类型，因为它是静态的，这是一个全球使用的定义。
       //全部参数使用的是哈希表，如何确保在改变别人不影响自己的时间去读它
       //之前，该方法可以使用锁的方法锁定表，不允许别人modify.when已读然后打开表。
       //现在。网络提供了实现同样功能的一个哈希表的同步方法，不需要手动锁，由系统框架直接完成 
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 执行一个SqlCommand命令不返回值，通过任命和指定ConnectionString，参数列表中使用数组形式的参数
        /// </summary>
        /// <remarks>
        /// 使用范例: 
        /// int result = ExecuteNonQuery(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="cmdType">sqlcommand类型（存储过程、SQL语句和T，等等。）</param>
        /// <param name="cmdText">存储过程的名称或T - SQL语句</param>
        /// <param name="commandParameters">使用MySQL命令提供的列表中的参数的数组</param>
        /// <returns>返回一个值，这意味着受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new();

            using (MySqlConnection conn = new(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// 执行一个SqlCommand命令不返回值，通过任命和指定ConnectionString 
        /// 使用数组形式参数的参数列表
        /// </summary>
        /// <remarks>
        /// 使用范例: 
        /// int result = ExecuteNonQuery(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="cmdType">MySQL命令命令类型（存储过程，T-SQL语句，等等。）</param>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="cmdText">存储过程的名称或T - SQL语句</param>
        /// <param name="commandParameters">使用MySQL命令提供的列表中的参数的数组</param>
        /// <returns>成功返回true，否则失败</returns>
        public static bool ExecuteNonQuery(CommandType cmdType, string connectionString, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new();

            using (MySqlConnection conn = new(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                try
                {
                    int val = cmd.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    cmd.Parameters.Clear();
                }
            }
        }
        /// <summary>
        /// 执行一个SqlCommand命令不返回值，通过任命和指定ConnectionString 
        /// 使用参数列表的形式参数数组 
        /// </summary>
        /// <param name="conn">SQL连接对象</param>
        /// <param name="cmdType">MySQL命令命令类型（存储过程，T-SQL语句，等等。）</param>
        /// <param name="cmdText">存储过程的名称或T-SQL语句</param>
        /// <param name="commandParameters">使用MySQL命令提供的列表中的参数的数组</param>
        /// <returns>返回一个值，这意味着受影响的行数</returns>
        public static int ExecuteNonQuery(MySqlConnection conn, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 执行一个SqlCommand命令不返回值，通过任命和指定ConnectionString
        /// 使用参数列表的形式参数数组 
        /// </summary>
        /// <param name="trans">SQL连接对象</param>
        /// <param name="cmdType">SqlCommand命令类型（存储过程，T-SQL语句，等等。）</param>
        /// <param name="cmdText">存储过程的名称或T-SQL语句</param>
        /// <param name="commandParameters">存储过程的名称或T-SQL语句</param>
        /// <returns>返回一个值，这意味着受影响的行数 </returns>
        public static int ExecuteNonQuery(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 调用SqlDataReader读取数据的方法
        /// </summary>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="cmdType">命令类型，如使用存储过程：commandtype.storedprocedure</param>
        /// <param name="cmdText">存储过程的名称或T-SQL语句</param>
        /// <param name="commandParameters">parameters参数数组</param>
        /// <returns>返回SqlDataReader类数据采集</returns>
        public static MySqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new();
            MySqlConnection conn = new(connectionString);

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                MySqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// 执行查询，返回结果集中第一行第一列的值
        /// </summary>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="cmdType">命令类型，如使用存储过程：commandtype.storedprocedure</param>
        /// <param name="cmdText">存储过程的名称或T-SQL语句</param>
        /// <param name="commandParameters">parameters参数数组</param>
        /// <returns>返回对象类型的值</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new();

            using (MySqlConnection connection = new(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }
        /// <summary>
        /// 读取数据表
        /// </summary>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="cmdText">存储过程的名称或T-SQL语句</param>
        /// <param name="commandParameters">parameters参数数组</param>
        /// <returns>返回DataSet</returns>
        public static DataSet GetDataSet(string connectionString, string cmdText, params MySqlParameter[] commandParameters)
        {
            DataSet retSet = new();
            using (MySqlDataAdapter msda = new(cmdText, connectionString))
            {
                msda.Fill(retSet);
            }
            return retSet;
        }

        /// <summary>
        /// 缓存的哈希表中的参数
        /// </summary>
        /// <param name="cacheKey">哈希表的键的名称</param>
        /// <param name="commandParameters">需要缓存参数</param>
        public static void CacheParameters(string cacheKey, params MySqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// 通过缓存的密钥获取参数的哈希表
        /// </summary>
        /// <param name="cacheKey">哈希表的键的名称</param>
        /// <returns>返回参数</returns>
        public static MySqlParameter[] GetCachedParameters(string cacheKey)
        {
            MySqlParameter[] cachedParms = (MySqlParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            MySqlParameter[] clonedParms = new MySqlParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (MySqlParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }

        /// <summary>
        /// 准备执行命令的参数
        /// </summary>
        /// <param name="cmd">MySQL命令命令</param>
        /// <param name="conn">有效的数据库连接对象</param>
        /// <param name="trans">数据库事务处理对象</param>
        /// <param name="cmdType">SqlCommand命令类型（存储过程，T-SQL语句，等等。） </param>
        /// <param name="cmdText">命令文本，T-SQL语句如SELECT * FROM 产品</param>
        /// <param name="cmdParms">返回的命令参数</param>
        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
                foreach (MySqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
        }
        #region parameters
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="ParamName">参数名称</param>
        /// <param name="DbType">数据类型</param>
        /// <param name="Size">数据大小</param>
        /// <param name="Direction">DataSet参数的类型</param>
        /// <param name="Value">设定值</param>
        /// <returns>返回已被分配参数</returns>
        public static MySqlParameter CreateParam(string ParamName, MySqlDbType DbType, Int32 Size, ParameterDirection Direction, object Value)
        {
            MySqlParameter param;


            if (Size > 0)
            {
                param = new MySqlParameter(ParamName, DbType, Size);
            }
            else
            {

                param = new MySqlParameter(ParamName, DbType);
            }


            param.Direction = Direction;
            if (!(Direction == ParameterDirection.Output && Value == null))
            {
                param.Value = Value;
            }


            return param;
        }

        /// <summary>
        /// 设置输入参数
        /// </summary>
        /// <param name="ParamName">参数的名称，如：“ID</param>
        /// <param name="DbType">参数的类型，如：mysqldbtype int。</param>
        /// <param name="Size">参数大小，如：为100字符类型的长度</param>
        /// <param name="Value">parameter value to be assigned</param>
        /// <returns>Parameters</returns>
        public static MySqlParameter CreateInParam(string ParamName, MySqlDbType DbType, int Size, object Value)
        {
            return CreateParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }

        /// <summary>
        /// 输出参数 
        /// </summary>
        /// <param name="ParamName">参数的名称，如：“ID</param>
        /// <param name="DbType">参数的类型，如：mysqldbtype int。</param>
        /// <param name="Size">参数大小，如：为100字符类型的长度</param>
        /// <returns>Parameters</returns>
        public static MySqlParameter CreateOutParam(string ParamName, MySqlDbType DbType, int Size)
        {
            return CreateParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }

        /// <summary>
        /// 设置返回参数值 
        /// </summary>
        /// <param name="ParamName">参数的名称，如：“ID</param>
        /// <param name="DbType">参数的类型，如：mysqldbtype int。</param>
        /// <param name="Size">参数大小，如：为100字符类型的长度</param>
        /// <returns>Parameters</returns>
        public static MySqlParameter CreateReturnParam(string ParamName, MySqlDbType DbType, int Size)
        {
            return CreateParam(ParamName, DbType, Size, ParameterDirection.ReturnValue, null);
        }

        /// <summary>
        /// 生成分页存储过程的参数
        /// </summary>
        /// <param name="CurrentIndex">当前页</param>
        /// <param name="PageSize">页面大小</param>
        /// <param name="WhereSql">查询条件</param>
        /// <param name="TableName">表名</param>
        /// <param name="Columns">查询列名</param>
        /// <param name="Sort">排序</param>
        /// <returns>mysqlparameter对象</returns>
        public static MySqlParameter[] GetPageParm(int CurrentIndex, int PageSize, string WhereSql, string TableName, string Columns, Hashtable Sort)
        {
            MySqlParameter[] parm = {
                                   DBMySqlHelper.CreateInParam("@CurrentIndex",  MySqlDbType.Int32,      4,      CurrentIndex    ),
                                   DBMySqlHelper.CreateInParam("@PageSize",      MySqlDbType.Int32,      4,      PageSize        ),
                                   DBMySqlHelper.CreateInParam("@WhereSql",      MySqlDbType.VarChar,  2500,    WhereSql        ),
                                   DBMySqlHelper.CreateInParam("@TableName",     MySqlDbType.VarChar,  20,     TableName       ),
                                   DBMySqlHelper.CreateInParam("@Column",        MySqlDbType.VarChar,  2500,    Columns         ),
                                   DBMySqlHelper.CreateInParam("@Sort",          MySqlDbType.VarChar,  50,     GetSort(Sort)   ),
                                   DBMySqlHelper.CreateOutParam("@RecordCount",  MySqlDbType.Int32,      4                       )
                                   };
            return parm;
        }
        /// <summary>
        /// 统计数据表
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="Columns">统计列</param>
        /// <param name="WhereSql">条件</param>
        /// <returns>参数设置</returns>
        public static MySqlParameter[] GetCountParm(string TableName, string Columns, string WhereSql)
        {
            MySqlParameter[] parm = {
                                   DBMySqlHelper.CreateInParam("@TableName",     MySqlDbType.VarChar,  20,     TableName       ),
                                   DBMySqlHelper.CreateInParam("@CountColumn",  MySqlDbType.VarChar,  20,     Columns         ),
                                   DBMySqlHelper.CreateInParam("@WhereSql",      MySqlDbType.VarChar,  250,    WhereSql        ),
                                   DBMySqlHelper.CreateOutParam("@RecordCount",  MySqlDbType.Int32,      4                      )
                                   };
            return parm;
        }
        /// <summary>
        /// 得到的SQL，排序 
        /// </summary>
        /// <param name="sort">排序列和值</param>
        /// <returns>SQL排序字符串</returns>
        private static string GetSort(Hashtable sort)
        {
            string str = "";
            int i = 0;
            if (sort != null && sort.Count > 0)
            {
                foreach (DictionaryEntry de in sort)
                {
                    i++;
                    str += de.Key + " " + de.Value;
                    if (i != sort.Count)
                    {
                        str += ",";
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// 执行trascation包括一个或多个SQL语句
        /// </summary>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="cmdType">SqlCommand命令类型（存储过程，T-SQL语句，等等。） </param>
        /// <param name="cmdTexts">命令文本，T-SQL语句如SELECT * FROM 产品</param>
        /// <param name="commandParameters"></param>
        /// <returns>执行trascation结果（成功：true|失败：false）</returns>
        public static bool ExecuteTransaction(string connectionString, CommandType cmdType, string[] cmdTexts, params MySqlParameter[][] commandParameters)
        {
            MySqlConnection myConnection = new(connectionString);       //get the connection object
            myConnection.Open();                                                        //open the connection
            MySqlTransaction myTrans = myConnection.BeginTransaction();                 //begin a trascation
            MySqlCommand cmd = new()
            {
                Connection = myConnection,
                Transaction = myTrans
            };

            try
            {
                for (int i = 0; i < cmdTexts.Length; i++)
                {
                    PrepareCommand(cmd, myConnection, null, cmdType, cmdTexts[i], commandParameters[i]);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                myTrans.Commit();
            }
            catch
            {
                myTrans.Rollback();
                return false;
            }
            finally
            {
                myConnection.Close();
            }
            return true;
        }
        #endregion

        //static string connectionString = "";
        ///// <summary>
        ///// 获取数据库连接字符串
        ///// </summary>
        ///// <param name="source">数据库</param>
        ///// <param name="user">用户名</param>
        ///// <param name="pwd">密码</param>
        ///// <returns>返回连接字符串</returns>
        //public static string SetMySqlConn(string  source,string user,string pwd)
        //{
            
        //    //////获取app.config配置文件里面的连接字符串
        //    //try
        //    //{
        //    //    connectionString = ConfigurationSettings.AppSettings["mySqlConnString"].ToString();
        //    //}
        //    //catch
        //    //{
        //    //    if (connectionString.Length == 0)////若是为空，则给一个默认
        //    //    {
        //            connectionString = "Database='receipt';Data Source='"+source+"';User Id='"+ user + "';Password='"+ pwd + "';charset='utf8';pooling=true";
        //    //    }
        //    //}


        //    return connectionString;
        //}

        ///// <summary>
        ///// 验证是否成功链接数据库
        ///// </summary>
        ///// <returns></returns>
        //public static bool IsOpenDB()
        //{

        //    using (MySqlConnection conn = new MySqlConnection(connectionString))
        //    {
        //        try
        //        {
        //            if (conn.State != ConnectionState.Open)
        //                conn.Open();
        //            conn.Close();
        //            conn.Dispose();
        //            return true;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //}

        //// 用于缓存参数的HASH表
        ////private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        ///// <summary>
        /////  给定连接的数据库用假设参数执行一个sql命令（不返回数据集）
        ///// </summary>
        ///// <param name="connectionString">一个有效的连接字符串</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns>执行命令所影响的行数</returns>
        //public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        //{

        //    MySqlCommand cmd = new MySqlCommand();

        //    using (MySqlConnection conn = new MySqlConnection(connectionString))
        //    {
        //        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //        int val = cmd.ExecuteNonQuery();
        //        cmd.Parameters.Clear();
        //        return val;

        //    }
        //}

        ///// <summary>
        ///// 用现有的数据库连接执行一个sql命令（不返回数据集）
        ///// </summary>
        ///// <param name="connection">一个现有的数据库连接</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns>执行命令所影响的行数</returns>
        //public static int ExecuteNonQuery(MySqlConnection connection, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        //{

        //    MySqlCommand cmd = new MySqlCommand();
        //    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        //    int val = cmd.ExecuteNonQuery();
        //    cmd.Parameters.Clear();
        //    return val;
        //}

        ///// <summary>
        /////使用现有的SQL事务执行一个sql命令（不返回数据集）
        ///// </summary>
        ///// <remarks>
        /////举例:
        /////  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="trans">一个现有的事务</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns>执行命令所影响的行数</returns>
        //public static int ExecuteNonQuery(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        //{
        //    MySqlCommand cmd = new MySqlCommand();
        //    PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
        //    int val = cmd.ExecuteNonQuery();
        //    cmd.Parameters.Clear();
        //    return val;
        //}

        ///// <summary>
        ///// 用执行的数据库连接执行一个返回数据集的sql命令
        ///// </summary>
        ///// <remarks>
        ///// 举例:
        /////  MySqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connectionString">一个有效的连接字符串</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns>包含结果的读取器</returns>
        //public static MySqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        //{
        //    //创建一个MySqlCommand对象
        //    MySqlCommand cmd = new MySqlCommand();
        //    //创建一个MySqlConnection对象
        //    MySqlConnection conn = new MySqlConnection(connectionString);

        //    //在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，
        //    //因此commandBehaviour.CloseConnection 就不会执行
        //    try
        //    {
        //        //调用 PrepareCommand 方法，对 MySqlCommand 对象设置参数
        //        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //        //调用 MySqlCommand  的 ExecuteReader 方法
        //        MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        //        //清除参数
        //        cmd.Parameters.Clear();
        //        return reader;
        //    }
        //    catch
        //    {
        //        //关闭连接，抛出异常
        //        conn.Close();
        //        throw;
        //    }
        //}

        ///// <summary>
        ///// 返回DataSet
        ///// </summary>
        ///// <param name="connectionString">一个有效的连接字符串</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="tableName">DataSet的表名</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns> 返回DataSet</returns>
        //public static DataSet GetDataSet(string connectionString, CommandType cmdType, string cmdText, string tableName, params MySqlParameter[] commandParameters)
        //{
        //    //创建一个MySqlCommand对象
        //    MySqlCommand cmd = new MySqlCommand();
        //    //创建一个MySqlConnection对象
        //    MySqlConnection conn = new MySqlConnection(connectionString);

        //    //在这里我们用一个try/catch结构执行sql文本命令/存储过程，因为如果这个方法产生一个异常我们要关闭连接，因为没有读取器存在，

        //    try
        //    {
        //        //调用 PrepareCommand 方法，对 MySqlCommand 对象设置参数
        //        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //        //调用 MySqlCommand  的 ExecuteReader 方法
        //        MySqlDataAdapter adapter = new MySqlDataAdapter();
        //        adapter.SelectCommand = cmd;
        //        DataSet ds = new DataSet();

        //        adapter.Fill(ds, tableName);
        //        //清除参数
        //        cmd.Parameters.Clear();
        //        conn.Close();
        //        return ds;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        ///// <summary>
        ///// 用指定的数据库连接字符串执行一个命令并返回一个数据集的第一列
        ///// </summary>
        ///// <remarks>
        /////例如:
        /////  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///// </remarks>
        /////<param name="connectionString">一个有效的连接字符串</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns>用 Convert.To{Type}把类型转换为想要的 </returns>
        //public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        //{
        //    MySqlCommand cmd = new MySqlCommand();

        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        //        object val = cmd.ExecuteScalar();
        //        cmd.Parameters.Clear();
        //        return val;
        //    }
        //}

        ///// <summary>
        ///// 用指定的数据库连接执行一个命令并返回一个数据集的第一列
        ///// </summary>
        ///// <remarks>
        ///// 例如:
        /////  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connection">一个存在的数据库连接</param>
        ///// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
        ///// <param name="cmdText">存储过程名称或者sql命令语句</param>
        ///// <param name="commandParameters">执行命令所用参数的集合</param>
        ///// <returns>用 Convert.To{Type}把类型转换为想要的 </returns>
        //public static object ExecuteScalar(MySqlConnection connection, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        //{

        //    MySqlCommand cmd = new MySqlCommand();

        //    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        //    object val = cmd.ExecuteScalar();
        //    cmd.Parameters.Clear();
        //    return val;
        //}

        ///// <summary>
        ///// 将参数集合添加到缓存
        ///// </summary>
        ///// <param name="cacheKey">添加到缓存的变量</param>
        ///// <param name="commandParameters">一个将要添加到缓存的sql参数集合</param>
        //public static void CacheParameters(string cacheKey, params MySqlParameter[] commandParameters)
        //{
        //    parmCache[cacheKey] = commandParameters;
        //}

        ///// <summary>
        ///// 找回缓存参数集合
        ///// </summary>
        ///// <param name="cacheKey">用于找回参数的关键字</param>
        ///// <returns>缓存的参数集合</returns>
        //public static MySqlParameter[] GetCachedParameters(string cacheKey)
        //{
        //    MySqlParameter[] cachedParms = (MySqlParameter[])parmCache[cacheKey];

        //    if (cachedParms == null)
        //        return null;

        //    MySqlParameter[] clonedParms = new MySqlParameter[cachedParms.Length];

        //    for (int i = 0, j = cachedParms.Length; i < j; i++)
        //        clonedParms[i] = (MySqlParameter)((ICloneable)cachedParms[i]).Clone();

        //    return clonedParms;
        //}

        ///// <summary>
        ///// 准备执行一个命令
        ///// </summary>
        ///// <param name="cmd">sql命令</param>
        ///// <param name="conn">OleDb连接</param>
        ///// <param name="trans">OleDb事务</param>
        ///// <param name="cmdType">命令类型例如 存储过程或者文本</param>
        ///// <param name="cmdText">命令文本,例如:Select * from Products</param>
        ///// <param name="cmdParms">执行命令的参数</param>
        //private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
        //{

        //    if (conn.State != ConnectionState.Open)
        //        conn.Open();

        //    cmd.Connection = conn;
        //    cmd.CommandText = cmdText;

        //    if (trans != null)
        //        cmd.Transaction = trans;

        //    cmd.CommandType = cmdType;

        //    if (cmdParms != null)
        //    {
        //        foreach (MySqlParameter parm in cmdParms)
        //            cmd.Parameters.Add(parm);
        //    }
        //}

    }
}

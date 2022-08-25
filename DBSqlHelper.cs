using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SuperDataBase
{
    /// <summary>
    /// 日 期:2014-11-17
    /// 作 者:不良帥
    /// 描 述:SqlServer数据库访问辅助类（适用于 sqlserver，mssql）
    /// </summary>
    public sealed class DBSqlHelper
    {

        #region  连接字符串对象 
        /// <summary>
        ///连接字符串
        /// </summary>
        private static string connStr = string.Empty;
        #endregion

        #region  设置连接字符串 
        /// <summary>
        /// 设置连接字符串
        /// </summary>
        /// <param name="server">数据库地址</param>
        /// <param name="uid">用户</param>
        /// <param name="pwd">密码</param>
        /// <param name="dataBase">数据库</param>
        public static void SetConnectionStr(string server,string uid,string pwd,string dataBase)
        {
            connStr=string.Format("server={0};uid={1};pwd={2};database={3}",server,uid,pwd,dataBase);
        }
        /// <summary>
        /// 设置连接字符串
        /// </summary>
       /// <param name="connectionStr">连接字符串</param>
        public static void SetConnectionStr(string connectionStr)
        {
            connStr=connectionStr;
        }
        #endregion

        #region  内部成员 
        /// <summary>
        /// 修改表的操作类型
        /// </summary>
        public enum AlertType
        {
            /// <summary>
            /// 修改表名称
            /// </summary>
            AlterTableName,
            /// <summary>
            /// 修改表中字段名称
            /// </summary>
            AlterColumnName,
            /// <summary>
            /// 修改表中字段
            /// </summary>
            AlterColumn,
            /// <summary>
            /// 修改表中字段类型
            /// </summary>
            AlterColumnType,
            /// <summary>
            /// 删除表
            /// </summary>
            dropTable,
            /// <summary>
            /// 删除表中字段
            /// </summary>
            dropColumn,
            /// <summary>
            /// 快速清空表中所有数据
            /// </summary>
            TruncateTable
        }
        private static SqlConnection conn;
        /// <summary>
        /// 连接对象
        /// </summary>
        static SqlConnection Conn
        {
            get
            {
                if (conn == null)
                    conn = new SqlConnection(connStr);
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                if (conn.State == ConnectionState.Broken)
                {
                    conn.Close();
                    conn.Open();
                }
                return conn;
            }
        }
        #endregion

        #region  查询DataReader 
        /// <summary>
        /// 查询DataReader，需要手动关闭SqlDataReader。
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回SqlDataReader</returns>
        public static SqlDataReader GetReader(string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    SqlDataReader sd = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    conn.Close();
                    return sd;
                }
            }
            catch { throw; }
        }
        /// <summary>
        /// 查询DataReader，需要手动关闭SqlDataReader。
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// /// <param name="paras">SqlParameter参数</param>
        /// <returns>返回SqlDataReader</returns>
        public static SqlDataReader GetReader(string sql, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    cmd.Parameters.AddRange(paras);
                    SqlDataReader sr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    cmd.Parameters.Clear();
                    Conn.Close();
                    return sr;
                }
            }
            catch { throw; }
        }
        #endregion

        #region  查询DataTable 
        /// <summary>
        /// 查询DataTable
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        public static DataTable GetTable(string sql)
        {
            try
            {
                using (SqlDataAdapter dap = new SqlDataAdapter(sql, Conn))
                {
                    DataTable dt = new DataTable();
                    dap.Fill(dt);
                    conn.Close();
                    return dt;
                }
            }
            catch { throw; }
        }
        /// <summary>
        /// 查询:DataTable
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="paras">SqlParameter参数</param>
        /// <returns>返回DataTable</returns>
        public static DataTable GetTable(string sql, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    cmd.Parameters.AddRange(paras);
                    using (SqlDataAdapter dap = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        dap.Fill(dt);
                        cmd.Parameters.Clear();
                        return dt;
                    }
                }
            }
            catch { throw; }
        }
        #endregion

        #region  增删改并返回受影响行数 
        /// <summary>
        /// 增删改并返回受影响行数
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        public static int ExecuteNoQuery(string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    int result = cmd.ExecuteNonQuery();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        /// <summary>
        /// 增删改并返回受影响行数
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="paras">SqlParameter参数</param>
        /// <returns>返回受影响行数</returns>
        public static int ExecuteNoQuery(string sql, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    cmd.Parameters.AddRange(paras);
                    int result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        #endregion

        #region  执行存储过程做增删改并返回受影响行数 
        /// <summary>
        /// 执行存储过程做增删改并返回受影响行数
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paras">参数</param>
        /// <returns>返回受影响行数</returns>
        public static int ExecuteProcNoQuery(string procName, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(procName, Conn))
                {
                    cmd.Parameters.AddRange(paras);
                    cmd.CommandType = CommandType.StoredProcedure;
                    int result = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        #endregion

        #region  删除表或表中的字段-谨慎操作 
        /// <summary>
        /// 删除表或表中的字段-谨慎操作
        /// </summary>
        /// <param name="type">操作类型</param>
        /// <param name="tableName">表名</param>
        /// <param name="tableNewName">新表名</param>
        /// <param name="columnName">字段名</param>
        /// <param name="columnNewName">新字段名</param>
        /// <param name="columnType">字段类型</param>
        /// <param name="dataTypeSize">类型大小,0为max</param>
        /// <returns>受影响行数</returns>
        public static int AlterTable(AlertType type, string tableName, string tableNewName = "", string columnName = "", string columnNewName = "", SqlDbType columnType = SqlDbType.NVarChar, int dataTypeSize = 0)
        {
            string sql = "";
            string size = "";
            switch (columnType)
            {
                case SqlDbType.Char:
                case SqlDbType.Binary:
                case SqlDbType.NChar:
                    if (dataTypeSize > 0)
                        size = string.Format("({0})", dataTypeSize);
                    else
                        size = "(50)";
                    break;
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    if (dataTypeSize > 0)
                        size = string.Format("({0})", dataTypeSize);
                    else
                        size = "(MAX)";
                    break;
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    if (dataTypeSize > 0 && dataTypeSize <= 7)
                        size = string.Format("({0})", dataTypeSize);
                    else
                        size = "(7)";
                    break;
                case SqlDbType.Decimal:
                    if (dataTypeSize > 0 && dataTypeSize <= 38)
                        size = string.Format("({0},4)", dataTypeSize);
                    else
                        size = "(38,4)";
                    break;
                case SqlDbType.BigInt:
                    break;
                case SqlDbType.Bit:
                    break;
                case SqlDbType.Date:
                    break;
                case SqlDbType.DateTime:
                    break;
                case SqlDbType.Float:
                    break;
                case SqlDbType.Image:
                    break;
                case SqlDbType.Int:
                    break;
                case SqlDbType.Money:
                    break;
                case SqlDbType.NText:
                    break;
                case SqlDbType.Real:
                    break;
                case SqlDbType.SmallDateTime:
                    break;
                case SqlDbType.SmallInt:
                    break;
                case SqlDbType.SmallMoney:
                    break;
                case SqlDbType.Structured:
                    break;
                case SqlDbType.Text:
                    break;
                case SqlDbType.Timestamp:
                    break;
                case SqlDbType.TinyInt:
                    break;
                case SqlDbType.Udt:
                    break;
                case SqlDbType.UniqueIdentifier:
                    break;
                case SqlDbType.Variant:
                    break;
                case SqlDbType.Xml:
                    break;
                default:
                    break;
            }
            switch (type)
            {
                case AlertType.AlterTableName:
                    sql = string.Format("EXEC sp_rename '{0}','{1}'", tableName, tableNewName);
                    break;
                case AlertType.AlterColumnName:
                    sql = string.Format("EXEC sp_rename '{0}.{1}','{2}'", tableName, columnName, columnNewName);
                    break;
                case AlertType.AlterColumn:
                    sql = string.Format("EXEC sp_rename '{0}.{1}','{2}' ALTER TABLE {3} ALTER COLUMN {4} {5}{6} NULL", tableName, columnName, columnNewName, tableName, columnNewName, columnType, dataTypeSize);
                    break;
                case AlertType.AlterColumnType:
                    sql = string.Format("ALTER TABLE {0} ALTER COLUMN {1} {2}{3} NULL", tableName, columnNewName, columnType, size);
                    break;
                case AlertType.dropTable:
                    sql = string.Format("drop table {0}", tableName);
                    break;
                case AlertType.dropColumn:
                    string.Format("alter table {0} drop {1}", tableName, columnName);
                    break;
                case AlertType.TruncateTable:
                    sql = string.Format("truncate table {0}", tableName);
                    break;
                default:
                    break;
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    int result = cmd.ExecuteNonQuery();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        #endregion

        #region  复制表带数据 
        /// <summary>
        /// 复制表带数据
        /// </summary>
        /// <param name="tableName">要复制的表名</param>
        /// <param name="saveTableName">保存的表名</param>
        /// <param name="isCopyData">是否复制数据</param>
        /// <returns>返回受影响行数</returns>
        public static int CopyTable(string tableName, string saveTableName, bool isCopyData)
        {
            try
            {
                string sql = "";
                if (isCopyData)
                    sql = string.Format("select * into {0} from {1}", saveTableName, tableName);
                else
                    sql = string.Format("select * into {0} from {1} where 1==2", saveTableName, tableName);
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    int result = cmd.ExecuteNonQuery();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        #endregion

        #region  查询所返回的结果集中第一行的第一列 
        /// <summary>
        /// 查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回object</returns>
        public static object ExecuteScalar(string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    object result = cmd.ExecuteScalar();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        /// <summary>
        ///  查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="paras">SqlParameter参数</param>
        /// <returns>返回object</returns>
        public static object ExecuteScalar(string sql, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conn))
                {
                    cmd.Parameters.AddRange(paras);
                    object result = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    conn.Close();
                    return result;
                }
            }
            catch { throw; }
        }
        #endregion

        #region  执行存储过程获取数据集 
        /// <summary>
        /// 执行存储过程获取数据集
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paras">参数</param>
        /// <returns>返回DataTable</returns>
        public static DataTable ExecuteProcSelect(string procName, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(procName, Conn))
                {
                    cmd.Parameters.AddRange(paras);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter dap = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        dap.Fill(dt);
                        cmd.Parameters.Clear();
                        return dt;
                    }
                }
            }
            catch { throw; }
        }
        #endregion

        #region  执行分页存储过程 
        /// <summary>
        /// 执行分页存储过程
        /// </summary>
        /// <param name="procName">分页存储过程名称</param>
        /// <param name="paras">SqlParameter参数</param>
        /// <returns>返回DataTable</returns>
        public static DataTable ExecProPageList(string procName, SqlParameter[] paras)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(procName, Conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        cmd.Parameters.Clear();
                        return dt;
                    }
                }
            }
            catch { throw; }
        }
        /// <summary>
        /// 执行简单分页存储过程
        /// </summary>
        /// <param name="procName">分页存储过程名称</param>
        /// <param name="paras">SqlParameter参数</param>
        /// <param name="rowCountParasName">SqlParameter参数中接收输出分页行数的参数名称</param>
        /// <param name="pageCountParasName">SqlParameter参数中接收输出分页总数的参数名称</param>
        /// <param name="rowCount">总行数-输出</param>
        /// <param name="pageCount">总页数-输出</param>
        /// <returns>返回DataTable</returns>
        public static DataTable ExecProSimplePageList(string procName, SqlParameter[] paras, string rowCountParasName, string pageCountParasName, out int rowCount, out int pageCount)
        {
            try
            {
                rowCount = 0;
                pageCount = 0;
                using (SqlCommand cmd = new SqlCommand(procName, Conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        rowCount = Convert.ToInt32(cmd.Parameters[string.Format("@{0}", rowCountParasName)].Value);
                        pageCount = Convert.ToInt32(cmd.Parameters[string.Format("@{0}", pageCountParasName)].Value);
                        cmd.Parameters.Clear();
                        return dt;
                    }
                }
            }
            catch { throw; }
        }
        /// <summary>
        /// 执行分页存储过程,并输出总行数和总页数
        /// </summary>
        /// <param name="procName">分页存储过程名称</param>
        /// <param name="paras">SqlParameter参数</param>
        /// <param name="rowCountParasName">SqlParameter参数中接收输出分页行数的参数名称</param>
        /// <param name="pageCountParasName">SqlParameter参数中接收输出分页总数的参数名称</param>
        /// <param name="rowCount">out总行数</param>
        /// <param name="pageCount">out总页数</param>
        /// <returns>返回DataTable、总行数和总页数</returns>
        public static DataTable ExecProPageList(string procName, SqlParameter[] paras, string rowCountParasName, string pageCountParasName, out int rowCount, out int pageCount)
        {
            try
            {
                rowCount = 0;
                pageCount = 0;
                using (SqlCommand cmd = new SqlCommand(procName, Conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        cmd.Parameters.Clear();
                        rowCount = Convert.ToInt32(cmd.Parameters[string.Format("@{0}", rowCountParasName)].Value);
                        pageCount = Convert.ToInt32(cmd.Parameters[string.Format("@{0}", pageCountParasName)].Value);
                        return dt;
                    }
                }
            }
            catch { throw; }
        }
        #endregion

        #region  执行事物(ADO.NET) 
        /// <summary>
        ///执行事物(ADO.NET)
        /// </summary>
        /// <param name="sqlStr">sql语句</param>
        /// <param name="TranName">事物名称</param>
        /// <returns>返回bool</returns>
        public static bool ExecuteTrasaction(string sqlStr, string TranName)
        {
            bool result = true;
            SqlTransaction tran = null;
            try
            {
                tran = Conn.BeginTransaction(TranName);
                using (SqlCommand cmd = new SqlCommand(sqlStr, Conn, tran))
                {
                    int n = cmd.ExecuteNonQuery();
                    if (n > 0)
                    {
                        tran.Commit();
                        result = true;
                    }
                    else
                    {
                        tran.Rollback();
                        result = false;
                    }
                }
            }
            catch
            {
                tran.Rollback();
                result = false;
            }
            return result;
        }
        #endregion

        #region  执行返回一条记录的泛型对象 
        /// <summary>
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">sql语句或存储过程名</param>
        /// <param name="commandType">sql命令类型</param>
        /// <param name="param">sql命令参数数组</param>
        /// <returns>实体对象</returns>
        public static T ExecuteEntity<T>(string commandText, CommandType commandType, params SqlParameter[] param)
        {
            T obj = default(T);
            using (SqlConnection connection = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(param);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        obj = ExecuteDataReader<T>(reader);
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="reader">只进只读对象</param>
        /// <returns>泛型对象</returns>
        private static T ExecuteDataReader<T>(IDataReader reader)
        {
            T obj = default(T);
            try
            {
                Type type = typeof(T);
                obj = (T)Activator.CreateInstance(type);//从当前程序集里面通过反射的方式创建指定类型的对象   
                //obj = (T)Assembly.Load(OracleHelper._assemblyName).CreateInstance(OracleHelper._assemblyName + "." + type.Name);//从另一个程序集里面通过反射的方式创建指定类型的对象 
                PropertyInfo[] propertyInfos = type.GetProperties();//获取指定类型里面的所有属性
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string fieldName = reader.GetName(i);
                        if (fieldName.ToLower() == propertyInfo.Name.ToLower())
                        {
                            object val = reader[propertyInfo.Name];//读取表中某一条记录里面的某一列信息
                            if (val != null && val != DBNull.Value)
                                propertyInfo.SetValue(obj, val, null);//给对象的某一个属性赋值
                            break;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            return obj;
        }
        #endregion

        #region  执行返回多条记录的泛型集合对象 
        /// <summary>
        /// 执行返回多条记录的泛型集合对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">sql语句或存储过程名</param>
        /// <param name="commandType">sql命令类型</param>
        /// <param name="param">sql命令参数数组</param>
        /// <returns>泛型集合对象</returns>
        public static List<T> ExecuteList<T>(string commandText, CommandType commandType, params SqlParameter[] param)
        {
            List<T> list = new List<T>();
            using (SqlConnection connection = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = commandType;
                        cmd.Parameters.AddRange(param);
                        connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        while (reader.Read())
                        {
                            T obj = ExecuteDataReader<T>(reader);
                            list.Add(obj);
                        }
                    }
                    catch
                    {
                        list = null;
                    }
                }
            }
            return list;
        }
        #endregion

        #region  ！！ 
        //        /// <summary>
        //        /// 数据库连接字符串
        //        /// </summary>
        //        public static string connectionString = string.Empty;

        //        // Hashtable 存储缓存参数
        //        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        //        #region  执行sql语句返回受影响行数 
        //        /// <summary>
        //        /// 执行一个不需要返回值的SqlCommand命令，通过指定专用的连接字符串，使用参数数组形式提供参数列表。
        //        /// </summary>
        //        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns>
        //        public static int ExecteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            using (SqlConnection conn = new SqlConnection(connectionString))
        //            {
        //                using (SqlCommand cmd = new SqlCommand())
        //                {
        //                    //通过PrePareCommand方法将参数逐个加入到SqlCommand的参数集合中
        //                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //                    int val = cmd.ExecuteNonQuery();
        //                    //清空SqlCommand中的参数列表
        //                    cmd.Parameters.Clear();
        //                    return val;
        //                }
        //            }
        //        }

        //        /// <summary>
        //        /// 执行一个不需要返回值的SqlCommand命令，通过指定专用的连接字符串，使用参数数组形式提供参数列表 。
        //        /// </summary>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns>
        //        public static int ExecteNonQuery(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecteNonQuery(connectionString, cmdType, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 执行存储过程
        //        /// </summary>
        //        /// <param name="cmdText">存储过程的名字</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns>
        //        public static int ExecteNonQueryProducts(string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecteNonQuery(CommandType.StoredProcedure, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 执行sql语句
        //        /// </summary>
        //        /// <param name="cmdText">T_Sql语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns>
        //        public static int ExecteNonQueryText(string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecteNonQuery(CommandType.Text, cmdText, commandParameters);
        //        }

        //        #endregion
        //        #region  执行sql语句获得DataTable 

        //        /// <summary>
        //        /// 执行一条返回结果集的SqlCommand，通过一个已经存在的数据库连接，使用参数数组提供参数。
        //        /// </summary>
        //        /// <param name="connecttionString">一个现有的数据库连接</param>
        //        /// <param name="cmdTye">SqlCommand命令类型</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个表集合(DataTableCollection)表示查询得到的数据集</returns>
        //        public static DataTableCollection GetTable(string connecttionString, CommandType cmdTye, string cmdText, SqlParameter[] commandParameters)
        //        {
        //            using (SqlConnection conn = new SqlConnection(connecttionString))
        //            {
        //                using (SqlCommand cmd = new SqlCommand())
        //                {
        //                    DataSet ds = new DataSet();
        //                    PrepareCommand(cmd, conn, null, cmdTye, cmdText, commandParameters);
        //                    SqlDataAdapter adapter = new SqlDataAdapter();
        //                    adapter.SelectCommand = cmd;
        //                    adapter.Fill(ds);
        //                    DataTableCollection table = ds.Tables;
        //                    return table;
        //                }
        //            }
        //        }

        //        /// <summary>
        //        /// 执行一条返回结果集的SqlCommand，通过一个已经存在的数据库连接
        //        /// 使用参数数组提供参数
        //        /// </summary>
        //        /// <param name="cmdTye">SqlCommand命令类型</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个表集合(DataTableCollection)表示查询得到的数据集</returns>
        //        public static DataTableCollection GetTable(CommandType cmdTye, string cmdText, SqlParameter[] commandParameters)
        //        {
        //            return GetTable(SqlHelper.connectionString, cmdTye, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 存储过程专用
        //        /// </summary>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个表集合(DataTableCollection)表示查询得到的数据集</returns>
        //        public static DataTableCollection GetTableProducts(string cmdText, SqlParameter[] commandParameters)
        //        {
        //            return GetTable(CommandType.StoredProcedure, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// Sql语句专用
        //        /// </summary>
        //        /// <param name="cmdText"> T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个表集合(DataTableCollection)表示查询得到的数据集</returns>
        //        public static DataTableCollection GetTableText(string cmdText, SqlParameter[] commandParameters)
        //        {
        //            return GetTable(CommandType.Text, cmdText, commandParameters);
        //        }

        //        #endregion
        //        #region  为执行命令准备参数 
        //        /// <summary>
        //        /// 为执行命令准备参数
        //        /// </summary>
        //        /// <param name="cmd">SqlCommand 命令</param>
        //        /// <param name="conn">已经存在的数据库连接</param>
        //        /// <param name="trans">数据库事物处理</param>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">Command text，T-SQL语句 例如 Select * from Products</param>
        //        /// <param name="cmdParms">返回带参数的命令</param>
        //        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        //        {
        //            //判断数据库连接状态
        //            if (conn.State != ConnectionState.Open)
        //                conn.Open();
        //            cmd.Connection = conn;
        //            cmd.CommandText = cmdText;
        //            //判断是否需要事物处理
        //            if (trans != null)
        //                cmd.Transaction = trans;
        //            cmd.CommandType = cmdType;
        //            if (cmdParms != null)
        //            {
        //                foreach (SqlParameter parm in cmdParms)
        //                    cmd.Parameters.Add(parm);
        //            }
        //        }
        //        #endregion
        //        #region  执行命令，返回在连接字符串中指定的数据库结果集，使用参数数组提供参数。使用后需要手动关闭连接。 

        //        /// <summary>
        //        /// 执行命令，返回在连接字符串中指定的数据库结果集，使用参数数组提供参数。使用后需要手动关闭连接。
        //        /// </summary>
        //        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回SqlDataReader</returns>
        //        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            try
        //            {
        //                SqlCommand cmd = new SqlCommand();
        //                SqlConnection conn = new SqlConnection(connectionString);
        //                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        //                cmd.Parameters.Clear();
        //                return rdr;
        //            }
        //            catch { throw; }
        //        }
        //        #endregion
        //        #region  执行sql语句获得DataSet 

        //        /// <summary>
        //        /// 获取DateSet
        //        /// </summary>
        //        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回DataSet</returns>
        //        public static DataSet ExecuteDataSet(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {

        //            try
        //            {
        //                using (SqlConnection conn = new SqlConnection(connectionString))
        //                {
        //                    using (SqlCommand cmd = new SqlCommand())
        //                    {
        //                        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //                        SqlDataAdapter da = new SqlDataAdapter();
        //                        DataSet ds = new DataSet();
        //                        da.SelectCommand = cmd;
        //                        da.Fill(ds);
        //                        return ds;
        //                    }
        //                }
        //            }
        //            catch
        //            {
        //                throw;
        //            }
        //        }

        //        /// <summary>
        //        /// 获取DataSet
        //        /// </summary>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回DataSet</returns>
        //        public static DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecuteDataSet(connectionString, cmdType, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 获取DataSet
        //        /// </summary>
        //        /// <param name="cmdText">存储过程的名字</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回DataSet</returns>
        //        public static DataSet ExecuteDataSetProducts(string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecuteDataSet(connectionString, CommandType.StoredProcedure, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 获取DataSet
        //        /// </summary>
        //        /// <param name="cmdText">T-SQL语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回DataSet</returns>
        //        public static DataSet ExecuteDataSetText(string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecuteDataSet(connectionString, CommandType.Text, cmdText, commandParameters);
        //        }
        //#endregion
        //        #region  执行sql语句获得DateView 
        //        /// <summary>
        //        /// 获得DateView
        //        /// </summary>
        //        /// <param name="connectionString">连接字符串</param>
        //        /// <param name="sortExpression">排序字段</param>
        //        /// <param name="direction">排序方式</param>
        //        /// <param name="cmdType">语句类型</param>
        //        /// <param name="cmdText">sql语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回DataView</returns>
        //        public static DataView ExecuteDataSet(string connectionString, string sortExpression, string direction, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {

        //            try
        //            {
        //                using (SqlConnection conn = new SqlConnection(connectionString))
        //                {
        //                    using (SqlCommand cmd = new SqlCommand())
        //                    {
        //                        PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
        //                        SqlDataAdapter da = new SqlDataAdapter();
        //                        DataSet ds = new DataSet();
        //                        da.SelectCommand = cmd;
        //                        da.Fill(ds);
        //                        DataView dv = ds.Tables[0].DefaultView;
        //                        dv.Sort = sortExpression + " " + direction;
        //                        return dv;
        //                    }
        //                }
        //            }
        //            catch
        //            {
        //                throw;
        //            }
        //        }
        //        #endregion
        //        #region  执行sql语句返回第一行的第一列 

        //        /// <summary>
        //        /// 返回第一行的第一列
        //        /// </summary>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回一个对象</returns>
        //        public static object ExecuteScalar(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecuteScalar(SqlHelper.connectionString, cmdType, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 返回第一行的第一列存储过程专用
        //        /// </summary>
        //        /// <param name="cmdText">存储过程的名字</param>
        //        /// <param name="commandParameters">SqlParameter参数</param>
        //        /// <returns>返回object</returns>
        //        public static object ExecuteScalarProducts(string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecuteScalar(SqlHelper.connectionString, CommandType.StoredProcedure, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 返回第一行的第一列Sql语句专用
        //        /// </summary>
        //        /// <param name="cmdText">sql语句</param>
        //        /// <param name="commandParameters">SqlParameter参数</param>
        //        /// <returns>返回object</returns>
        //        public static object ExecuteScalarText(string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            return ExecuteScalar(SqlHelper.connectionString, CommandType.Text, cmdText, commandParameters);
        //        }

        //        /// <summary>
        //        /// 执行命令，返回在连接字符串中指定的数据库的第一列的第一个记录。
        //        /// </summary>
        //        /// <remarks>
        //        /// e.g.:  
        //        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        //        /// </remarks>
        //        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回object</returns>
        //        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            using (SqlConnection connection = new SqlConnection(connectionString))
        //            {
        //                using (SqlCommand cmd = new SqlCommand())
        //                {
        //                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        //                    object val = cmd.ExecuteScalar();
        //                    cmd.Parameters.Clear();
        //                    return val;
        //                }
        //            }
        //        }

        //        /// <summary>
        //        /// 执行命令，返回在连接字符串中指定的数据库的第一列的第一个记录。
        //        /// </summary>
        //        /// <remarks>
        //        /// e.g.:  
        //        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        //        /// </remarks>
        //        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        //        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param>
        //        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param>
        //        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param>
        //        /// <returns>返回object</returns>
        //        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        //        {
        //            using (SqlCommand cmd = new SqlCommand())
        //            {
        //                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
        //                object val = cmd.ExecuteScalar();
        //                cmd.Parameters.Clear();
        //                return val;
        //            }
        //        }
        //        #endregion
        //        #region  添加参数数组缓存 
        //        /// <summary>
        //        /// 添加参数数组缓存
        //        /// </summary>
        //        /// <param name="cacheKey">关键参数的高速缓存</param>
        //        /// <param name="cmdParms">sqlparamters的数组被缓存</param>
        //        public static void CacheParameters(string cacheKey, params SqlParameter[] commandParameters)
        //        {
        //            parmCache[cacheKey] = commandParameters;
        //        }
        //        #endregion
        //        #region  检索SqlParameter参数 
        //        /// <summary>
        //        /// 检索SqlParameter参数
        //        /// </summary>
        //        /// <param name="cacheKey">主要用于查找参数</param>
        //        /// <returns>返回sqlparamter[]</returns>
        //        public static SqlParameter[] GetCachedParameters(string cacheKey)
        //        {
        //            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];
        //            if (cachedParms == null)
        //                return null;
        //            SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];
        //            for (int i = 0, j = cachedParms.Length; i < j; i++)
        //                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();
        //            return clonedParms;
        //        }
        //        #endregion
        //        #region  检查是否存在 
        //        /// <summary>
        //        /// 检查是否存在
        //        /// </summary>
        //        /// <param name="strSql">Sql语句</param>
        //        /// <param name="cmdParms">SqlParameter参数</param>
        //        /// <returns>存在返回true，不存在返回false</returns>
        //        public static bool CheckExists(string strSql, params SqlParameter[] cmdParms)
        //        {
        //            int cmdresult = Convert.ToInt32(ExecuteScalar(connectionString, CommandType.Text, strSql, cmdParms));
        //            if (cmdresult == 0)
        //                return false;
        //            else
        //                return true;
        //        }
        //        #endregion

        #endregion

    }
}

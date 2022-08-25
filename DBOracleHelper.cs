using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using System.ComponentModel;

namespace SuperDataBase
{
    /// <summary>
    /// 日 期:2018-11-15
    /// 作 者:不良帥
    /// 描 述:ManagedDataAccess方式数据操作辅助方法类(Oracle命令参数数组，参数顺序需要对应定义时的顺序)
    /// </summary>
    public class DBOracleHelper
    {
        #region 【数据库连接字符串】
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string connectionString = string.Empty;
        /// <summary>
        /// 设置数据库连接
        /// </summary>
        /// <param name="dbIp">数据库ip</param>
        /// <param name="dbPort">数据库端口</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="userId">数据库用户id</param>
        /// <param name="userPwd">数据库用户密码</param>
        /// <returns>返回字符串</returns>
        public static string SetConnectionStr(string dbIp,string dbPort,string dbName,string userId,string userPwd)
        {
            return connectionString = string.Format("Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1})))(CONNECT_DATA = (SERVICE_NAME = {2}))); Persist Security Info = True; User ID = {3}; Password = {4};",dbIp,dbPort,dbName,userId,userPwd);
        }
        #endregion

        #region 【获取最大ID】
        /// <summary>
        /// 获取最大ID
        /// </summary>
        /// <param name="FieldName">字段名称</param>
        /// <param name="TableName">表名</param>
        /// <returns>最大ID</returns>
        public static int GetMaxID(string FieldName, string TableName)
        {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle(strsql);
            if (obj == null)
                return 1;
            else
                return int.Parse(obj.ToString());
        }
        #endregion

        #region 【检查是否存在】
        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="strSql">sql语句</param>
        /// <returns>存在返回true，不存在返回flase</returns>
        public static bool CheckExists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if (Equals(obj, null) || Equals(obj, DBNull.Value))
                cmdresult = 0;
            else
                cmdresult = int.Parse(obj.ToString());
            if (cmdresult == 0)
                return false;
            else
                return true;
        }
        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="strSql">sql语句</param>
        /// <param name="cmdParms">OracleParameter参数</param>
        /// <returns>存在返回true，不存在返回flase</returns>
        public static bool CheckExists(string strSql, params OracleParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if (Equals(obj, null) || Equals(obj, DBNull.Value))
                cmdresult = 0;
            else
                cmdresult = int.Parse(obj.ToString());
            if (cmdresult == 0)
                return false;
            else
                return true;
        }

        #endregion

        #region【执行SQL语句】

        /// <summary>
        /// 执行SQL语句，返回影响的记录数。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (OracleException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    OracleParameter myParameter = new OracleParameter("@content", OracleDbType.NVarchar2)
                    {
                        Value = content
                    };
                    cmd.Parameters.Add(myParameter);
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (OracleException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">OracleParameter参数</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params OracleParameter[] cmdParms)
        {
            OracleConnection connection = null;
            OracleCommand cmd = null;
            try
            {
                connection = new OracleConnection(connectionString);
                cmd = new OracleCommand();

                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                int rows = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                connection.Close();
                return rows;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        #endregion

        #region 【执行多条SQL语句，实现数据库事务】
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    OracleTransaction tx = conn.BeginTransaction();
                    cmd.Transaction = tx;
                    try
                    {
                        for (int n = 0; n < SQLStringList.Count; n++)
                        {
                            string strsql = SQLStringList[n].ToString();
                            if (strsql.Trim().Length > 1)
                            {
                                cmd.CommandText = strsql;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch (OracleException E)
                    {
                        tx.Rollback();
                        throw new Exception(E.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的OracleParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                using (OracleTransaction trans = conn.BeginTransaction())
                {
                    OracleCommand cmd = new OracleCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            OracleParameter[] cmdParms = (OracleParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region 【向数据库里插入图像格式的字段】
        /// <summary>
        /// 向数据库里插入图像格式的字段
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(strSQL, connection))
                {
                    OracleParameter myParameter = new OracleParameter("@fs", OracleDbType.LongRaw)
                    {
                        Value = fs
                    };
                    cmd.Parameters.Add(myParameter);
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (OracleException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
        }
        #endregion

        #region 【执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )】
        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(string strSQL)
        {
            try
            {
                OracleConnection connection = new OracleConnection(connectionString);
                OracleCommand cmd = new OracleCommand(strSQL, connection);
                connection.Open();
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (OracleException e)
            {
                throw new Exception(e.Message);
            }

        }
        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <param name="cmdParms">OracleParameter参数</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(string SQLString, params OracleParameter[] cmdParms)
        {
            try
            {
                OracleConnection connection = new OracleConnection(connectionString);
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region 【执行查询语句，返回DataSet】
        /// <summary>
        /// 执行查询语句，返回DataSet。
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>返回DataSet</returns>
        public static DataSet QueryDataSet(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch
                {
                    throw;
                }
                return ds;
            }
        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">OracleParameter参数</param>
        /// <returns>返回DataSet</returns>
        public static DataSet QueryDataSet(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch
                    {
                        throw;
                    }
                    return ds;
                }
            }
        }
        #endregion

        #region 【执行返回一行一列的数据库操作，返回查询结果（object）】
        /// <summary>
        /// 执行返回一行一列的数据库操作，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">Oracle命令参数数组，参数顺序需要对应定义时的顺序</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if (Equals(obj, null) || Equals(obj, DBNull.Value))
                            return null;
                        else
                            return obj;
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行返回一行一列的数据库操作，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if (Equals(obj, null) || Equals(obj, DBNull.Value))
                            return null;
                        else
                            return obj;
                    }
                    catch (OracleException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        #endregion

        #region【执行存储过程】

        /// <summary>
        /// 执行存储过程，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            try
            {
                OracleConnection connection = new OracleConnection(connectionString);
                OracleDataReader returnReader;
                connection.Open();
                OracleCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return returnReader;
            }
            catch { throw; }
        }


        /// <summary>
        /// 执行存储过程，返回DataSet	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>返回DataSet</returns>
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                OracleDataAdapter sqlDA = new OracleDataAdapter
                {
                    SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
                };
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }
        /// <summary>
        /// 执行存储过程，返回影响的行数		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns>返回受影响行数</returns>
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                int result;
                connection.Open();
                OracleCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                return result;
            }
        }
        #endregion

        #region  【执行返回一条记录的泛型对象】 
        
        /// <summary>
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <param name="connectionString">连接语句</param>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句</param>
        /// <returns>实体对象</returns>
        public static T ExecuteEntity<T>(string commandText)
        {
            T obj = default(T);
            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand(commandText, connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        connection.Open();
                        using (OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                obj = ExecuteDataReader<T>(reader);
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            catch {}
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
                            try
                            {
                                object val = reader[propertyInfo.Name];//读取表中某一条记录里面的某一列信息
                                if (val != null && val != DBNull.Value)
                                    propertyInfo.SetValue(obj, val, null);//给对象的某一个属性赋值
                            }
                            catch (Exception ex) { }
                            break;
                        }
                    }
                }
            }
            catch {  }
            return obj;
        }
        /// <summary>
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <param name="param">Oracle命令参数数组</param>
        /// <returns>实体对象</returns>
        public static T ExecuteEntity<T>(string commandText, CommandType commandType, params OracleParameter[] param)
        {
            T obj = default(T);
            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand(commandText, connection))
                    {
                        cmd.CommandType = commandType;
                        cmd.Parameters.AddRange(param);
                        connection.Open();
                        using (OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                obj = ExecuteDataReader<T>(reader);
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                }
            }
            catch { }
            return obj;
        }
        #endregion

        #region 【执行返回多条记录的泛型集合对象】
        /// <summary>
        /// 执行返回多条记录的泛型集合对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <param name="param">Oracle命令参数数组，参数顺序需要对应定义时的顺序</param>
        /// <returns>泛型集合对象</returns>
        public static List<T> ExecuteList<T>(string commandText, CommandType commandType, params OracleParameter[] param)
        {
            List<T> list = new List<T>();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = commandType;
                        cmd.Parameters.AddRange(param);
                        connection.Open();
                        using (OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                T obj = ExecuteDataReader<T>(reader);
                                list.Add(obj);
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    catch
                    {
                        list = null;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }

            return list;
        }
        /// <summary>
        /// 执行返回多条记录的泛型集合对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <returns>泛型集合对象</returns>
        public static List<T> ExecuteList<T>(string commandText)
        {
            List<T> list = new List<T>();
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        connection.Open();
                        using (OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                T obj = ExecuteDataReader<T>(reader);
                                list.Add(obj);
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    catch
                    {
                        list = null;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 执行返回多条记录的泛型集合对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="commandText">Oracle语句或存储过程名</param>
        /// <param name="commandType">Oracle命令类型</param>
        /// <returns>泛型集合对象</returns>
        public static BindingList<T> ExecuteBindingList<T>(string commandText)
        {
            BindingList<T> list = new BindingList<T>();
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(commandText, connection))
                {
                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        connection.Open();
                        using (OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                T obj = ExecuteDataReader<T>(reader);
                                list.Add(obj);
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        list = null;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return list;
        }
        
        #endregion

        #region 【私有方法】
        /// <summary>
        /// 构建 OracleCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>OracleCommand</returns>
        private static OracleCommand BuildQueryCommand(OracleConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            OracleCommand command = new OracleCommand(storedProcName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            foreach (OracleParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }
        /// <summary>
        /// 创建 OracleCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>OracleCommand 对象实例</returns>
        private static OracleCommand BuildIntCommand(OracleConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            OracleCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new OracleParameter("ReturnValue",
                OracleDbType.Int32, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        private static void PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, string cmdText, OracleParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (OracleParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        #endregion
    }
}

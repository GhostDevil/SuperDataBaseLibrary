using System;
using System.Collections;
using System.Data;
using Oracle.ManagedDataAccess.Client;


namespace SuperUtilities.SuperDataBase
{
    class DBOracleHelper2
    {
        /// <summary>
        /// 数据访问基础类(基于Oracle) Copyright (C) Maticsoft
        /// </summary>
        public abstract class DbHelperOra
        {
            //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.
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
            public static string SetConnectionStr(string dbIp, string dbPort, string dbName, string userId, string userPwd)
            {
                return connectionString = string.Format("Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1})))(CONNECT_DATA = (SERVICE_NAME = {2}))); Persist Security Info = True; User ID = {3}; Password = {4};", dbIp, dbPort, dbName, userId, userPwd);
            }
            #endregion

            #region 公用方法

            public static int GetMaxID(string FieldName, string TableName)
            {
                string strsql = "select max(" + FieldName + ")+1 from " + TableName;
                object obj = GetSingle(strsql);
                if (obj == null)
                {
                    return 1;
                }
                else
                {
                    return int.Parse(obj.ToString());
                }
            }
            public static bool Exists(string strSql)
            {
                object obj = GetSingle(strSql);
                int cmdresult;
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    cmdresult = 0;
                }
                else
                {
                    cmdresult = int.Parse(obj.ToString());
                }
                if (cmdresult == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public static bool Exists(string strSql, params OracleParameter[] cmdParms)
            {
                object obj = GetSingle(strSql, cmdParms);
                int cmdresult;
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    cmdresult = 0;
                }
                else
                {
                    cmdresult = int.Parse(obj.ToString());
                }
                if (cmdresult == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


            #endregion

            #region  执行简单SQL语句

            /// <summary>
            /// 执行SQL语句，返回影响的记录数
            /// </summary>
            /// <param name="SQLString">SQL语句</param>
            /// <returns>影响的记录数</returns>
            public static int ExecuteSql(string SQLString)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    using (OracleCommand cmd = new(SQLString, connection))
                    {
                        try
                        {
                            connection.Open();
                            int rows = cmd.ExecuteNonQuery();
                            return rows;
                        }
                        catch (OracleException E)
                        {
                            connection.Close();
                            throw new Exception(E.Message);
                        }
                    }
                }
            }

            /// <summary>
            /// 执行多条SQL语句，实现数据库事务。
            /// </summary>
            /// <param name="SQLStringList">多条SQL语句</param>		
            public static void ExecuteSqlTran(ArrayList SQLStringList)
            {
                using (OracleConnection conn = new(connectionString))
                {
                    conn.Open();
                    OracleCommand cmd = new()
                    {
                        Connection = conn
                    };
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
            /// <summary>
            /// 执行带一个存储过程参数的的SQL语句。
            /// </summary>
            /// <param name="SQLString">SQL语句</param>
            /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
            /// <returns>影响的记录数</returns>
            public static int ExecuteSql(string SQLString, string content)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    OracleCommand cmd = new(SQLString, connection);
                    OracleParameter myParameter = new("@content", OracleDbType.Varchar2)
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
                    }
                }
            }
            /// <summary>
            /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
            /// </summary>
            /// <param name="strSQL">SQL语句</param>
            /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
            /// <returns>影响的记录数</returns>
            public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    OracleCommand cmd = new(strSQL, connection);
                    OracleParameter myParameter = new("@fs", OracleDbType.LongRaw)
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
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }

            /// <summary>
            /// 执行一条计算查询结果语句，返回查询结果（object）。
            /// </summary>
            /// <param name="SQLString">计算查询结果语句</param>
            /// <returns>查询结果（object）</returns>
            public static object GetSingle(string SQLString)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    using (OracleCommand cmd = new(SQLString, connection))
                    {
                        try
                        {
                            connection.Open();
                            object obj = cmd.ExecuteScalar();
                            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                            {
                                return null;
                            }
                            else
                            {
                                return obj;
                            }
                        }
                        catch (OracleException e)
                        {
                            connection.Close();
                            throw new Exception(e.Message);
                        }
                    }
                }
            }
            /// <summary>
            /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
            /// </summary>
            /// <param name="strSQL">查询语句</param>
            /// <returns>OracleDataReader</returns>
            public static OracleDataReader ExecuteReader(string strSQL)
            {
                OracleConnection connection = new(connectionString);
                OracleCommand cmd = new(strSQL, connection);
                try
                {
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
            /// 执行查询语句，返回DataSet
            /// </summary>
            /// <param name="SQLString">查询语句</param>
            /// <returns>DataSet</returns>
            public static DataSet Query(string SQLString)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    DataSet ds = new();
                    try
                    {
                        connection.Open();
                        OracleDataAdapter command = new(SQLString, connection);
                        command.Fill(ds, "ds");
                    }
                    catch (OracleException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }


            #endregion

            #region 执行带参数的SQL语句

            /// <summary>
            /// 执行SQL语句，返回影响的记录数
            /// </summary>
            /// <param name="SQLString">SQL语句</param>
            /// <returns>影响的记录数</returns>
            public static int ExecuteSql(string SQLString, params OracleParameter[] cmdParms)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    using (OracleCommand cmd = new())
                    {
                        try
                        {
                            PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                            int rows = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            return rows;
                        }
                        catch (OracleException E)
                        {
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
                using (OracleConnection conn = new(connectionString))
                {
                    conn.Open();
                    using (OracleTransaction trans = conn.BeginTransaction())
                    {
                        OracleCommand cmd = new();
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


            /// <summary>
            /// 执行一条计算查询结果语句，返回查询结果（object）。
            /// </summary>
            /// <param name="SQLString">计算查询结果语句</param>
            /// <returns>查询结果（object）</returns>
            public static object GetSingle(string SQLString, params OracleParameter[] cmdParms)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    using (OracleCommand cmd = new())
                    {
                        try
                        {
                            PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                            object obj = cmd.ExecuteScalar();
                            cmd.Parameters.Clear();
                            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                            {
                                return null;
                            }
                            else
                            {
                                return obj;
                            }
                        }
                        catch (OracleException e)
                        {
                            throw new Exception(e.Message);
                        }
                    }
                }
            }

            /// <summary>
            /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
            /// </summary>
            /// <param name="strSQL">查询语句</param>
            /// <returns>OracleDataReader</returns>
            public static OracleDataReader ExecuteReader(string SQLString, params OracleParameter[] cmdParms)
            {
                OracleConnection connection = new(connectionString);
                OracleCommand cmd = new();
                try
                {
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    cmd.Parameters.Clear();
                    return myReader;
                }
                catch (OracleException e)
                {
                    throw new Exception(e.Message);
                }

            }

            /// <summary>
            /// 执行查询语句，返回DataSet
            /// </summary>
            /// <param name="SQLString">查询语句</param>
            /// <returns>DataSet</returns>
            public static DataSet Query(string SQLString, params OracleParameter[] cmdParms)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    OracleCommand cmd = new();
                    PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                    using (OracleDataAdapter da = new(cmd))
                    {
                        DataSet ds = new();
                        try
                        {
                            da.Fill(ds, "ds");
                            cmd.Parameters.Clear();
                        }
                        catch (OracleException ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        return ds;
                    }
                }
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

            #region 存储过程操作

            /// <summary>
            /// 执行存储过程 返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
            /// </summary>
            /// <param name="storedProcName">存储过程名</param>
            /// <param name="parameters">存储过程参数</param>
            /// <returns>OracleDataReader</returns>
            public static OracleDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
            {
                OracleConnection connection = new(connectionString);
                OracleDataReader returnReader;
                connection.Open();
                OracleCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return returnReader;
            }


            /// <summary>
            /// 执行存储过程
            /// </summary>
            /// <param name="storedProcName">存储过程名</param>
            /// <param name="parameters">存储过程参数</param>
            /// <param name="tableName">DataSet结果中的表名</param>
            /// <returns>DataSet</returns>
            public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    DataSet dataSet = new();
                    connection.Open();
                    OracleDataAdapter sqlDA = new()
                    {
                        SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
                    };
                    sqlDA.Fill(dataSet, tableName);
                    connection.Close();
                    return dataSet;
                }
            }


            /// <summary>
            /// 构建 OracleCommand 对象(用来返回一个结果集，而不是一个整数值)
            /// </summary>
            /// <param name="connection">数据库连接</param>
            /// <param name="storedProcName">存储过程名</param>
            /// <param name="parameters">存储过程参数</param>
            /// <returns>OracleCommand</returns>
            private static OracleCommand BuildQueryCommand(OracleConnection connection, string storedProcName, IDataParameter[] parameters)
            {
                OracleCommand command = new(storedProcName, connection)
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
            /// 执行存储过程，返回影响的行数		
            /// </summary>
            /// <param name="storedProcName">存储过程名</param>
            /// <param name="parameters">存储过程参数</param>
            /// <param name="rowsAffected">影响的行数</param>
            /// <returns></returns>
            public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
            {
                using (OracleConnection connection = new(connectionString))
                {
                    int result;
                    connection.Open();
                    OracleCommand command = BuildIntCommand(connection, storedProcName, parameters);
                    rowsAffected = command.ExecuteNonQuery();
                    result = (int)command.Parameters["ReturnValue"].Value;
                    //Connection.Close();
                    return result;
                }
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
            #endregion

        }
    }
}

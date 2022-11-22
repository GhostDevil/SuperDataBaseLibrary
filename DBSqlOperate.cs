using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SuperDataBase
{
    /// <summary>
    /// 日 期:2014-12-07
    /// 作 者:不良帥
    /// 描 述:SqlServer数据库操作帮助类,包括数据的创建，删除，修改密码等一系统列操作。
    /// </summary>
    public abstract class DBSqlOperate
    {
        #region  连接Sql字符串 

        /// <summary> 
        /// 数据库连接字符串 
        /// </summary> 
        public static readonly string connectionString = string.Empty;

        #endregion

        #region  ExecteNonQuery方法 

        /// <summary> 
        ///执行一个不需要返回值的SqlCommand命令，通过指定专用的连接字符串。 
        /// 使用参数数组形式提供参数列表  
        /// </summary> 
        /// <param name="connectionString">一个有效的数据库连接字符串</param> 
        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param> 
        /// <param name="cmdText">存储过程的名字或者 T-SQL 语句</param> 
        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param> 
        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns> 
        public static int ExecteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            using (SqlConnection conn = new(connectionString))
            {
                using (SqlCommand cmd = new())
                {
                    //通过PrePareCommand方法将参数逐个加入到SqlCommand的参数集合中 
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    int val = cmd.ExecuteNonQuery();
                    //清空SqlCommand中的参数列表 
                    cmd.Parameters.Clear();
                    return val;
                }
            }
        }

        /// <summary> 
        ///存储过程专用 
        /// </summary> 
        /// <param name="cmdText">存储过程的名字</param> 
        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param> 
        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns> 
        public static int ExecteNonQueryProducts(string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecteNonQuery(DBSqlOperate.connectionString, CommandType.StoredProcedure, cmdText, commandParameters);
        }

        /// <summary> 
        ///Sql语句专用 
        /// </summary> 
        /// <param name="cmdText">T_Sql语句</param> 
        /// <param name="commandParameters">以数组形式提供SqlCommand命令中用到的参数列表</param> 
        /// <returns>返回一个数值表示此SqlCommand命令执行后影响的行数</returns> 
        public static int ExecteNonQueryText(string cmdText, params SqlParameter[] commandParameters)
        {
            return ExecteNonQuery(DBSqlOperate.connectionString, CommandType.Text, cmdText, commandParameters);
        }

        #endregion

        #region  为执行命令准备参数 

        /// <summary> 
        /// 为执行命令准备参数 
        /// </summary> 
        /// <param name="cmd">SqlCommand 命令</param> 
        /// <param name="conn">已经存在的数据库连接</param> 
        /// <param name="trans">数据库事物处理</param> 
        /// <param name="cmdType">SqlCommand命令类型 (存储过程， T-SQL语句， 等等。)</param> 
        /// <param name="cmdText">Command text，T-SQL语句 例如 Select * from Products</param> 
        /// <param name="cmdParms">返回带参数的命令</param> 
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            //判断数据库连接状态 
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            //判断是否需要事物处理 
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        #endregion

        #region  根据条件创建数据库 

        /// <summary> 
        /// 根据条件创建数据库 
        /// </summary> 
        /// <param name="dbName">用于指定数据文件的逻辑名称</param> 
        /// <param name="dbFileName">指定数据文件的操作系统文件名。其后面的参数是创建数据文件时定义的物理文件的路径名和文件名</param> 
        /// <param name="dbSize">指定数据文件的大小</param> 
        /// <param name="dbMaxSize">指定数据文件可以增长到的最大大小</param> 
        /// <param name="dbFileGrowth"> 
        /// 指定数据文件的增长增量，其值不能超过maxsize设置。0表示不增长。，默认值为MB。如果指定为%， 
        /// 则增量大小为发生时文件大小的指定百分比，如果没有指定，默认值为10%。 
        /// </param> 
        /// <param name="logName">用于指定数据日志的逻辑名称</param> 
        /// <param name="logFileName">指定数据日志的操作系统文件名。其后面的参数是创建数据日志时定义的物理文件的路径名和文件名</param> 
        /// <param name="logSize">指定数据日志的大小</param> 
        /// <param name="logMaxSize">指定数据日志可以增长到的最大大小</param> 
        /// <param name="logFileGrowth"> 
        /// 指定数据日志的增长增量，其值不能超过maxsize设置。0表示不增长。，默认值为MB。如果指定为%， 
        /// 则增量大小为发生时文件大小的指定百分比，如果没有指定，默认值为10%。 
        /// </param> 
        /// <param name="isDeletedb">在创建数据库是否删除同名的现存数据库</param> 
        public static void CreateDatabase(string dbName, string dbFileName, string dbSize, string dbMaxSize, string dbFileGrowth,
           string logName, string logFileName, string logSize, string logMaxSize, string logFileGrowth, Boolean isDeletedb)
        {
            #region 检查是否存在数据dbName

            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO");
            if (isDeletedb)
            {
                dbSql.Append("IF  EXISTS（SELECT * FROM  sysdatabases WHERE  name ='@dbName'）begin DROP DATABASE @dbName  end");
            }
            #endregion

            #region 创建数据库

            //开始创建数据并指定名称 
            dbSql.Append("CREATE DATABASE @dbName ON  PRIMARY (");
            //数据库名 
            dbSql.Append("NAME='@ dbName" + "_data',");
            //数据路经 
            dbSql.Append("FILENAME='@dbFileName', ");
            //大小 
            dbSql.Append("SIZE=@dbSize, ");
            //最大值 
            dbSql.Append("MAXSIZE= @dbMaxSize,");
            //增长值 
            dbSql.Append("FILEGROWTH=@dbFileGrowth)");

            #endregion

            #region 创建数据库日志

            //开始创建日志文件 
            dbSql.Append("LOG ON (");
            //日志文件名 
            dbSql.Append("NAME='@logName" + "_log',");
            //日志文件路经 
            dbSql.Append("FILENAME='@logFileName',");
            //大小 
            dbSql.Append("SIZE=@logSize,");
            //最大值 
            dbSql.Append("MAXSIZE=@logMaxSize,");
            //增加值 
            dbSql.Append("FILEGROWTH=@logFileGrowth ) GO");

            #endregion

            #region 开始执行创建命令

            //设置参数列表 
            SqlParameter[] parameter =  
                { 
                    new SqlParameter("@dbName", dbName),  
                    new SqlParameter("@dbFileName", dbFileName), 
                    new SqlParameter("@dbSize", dbSize), 
                    new SqlParameter("@dbMaxSize", dbMaxSize), 
                    new SqlParameter("@dbFileGrowth", dbFileGrowth), 
                    new SqlParameter("@logName", logName), 
                    new SqlParameter("@logFileName", logFileName), 
                    new SqlParameter("@logSize", logSize), 
                    new SqlParameter("@logMaxSize", logMaxSize), 
                    new SqlParameter("@logFileGrowth", logFileGrowth) 
                };

            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);

            #endregion
        }
        #endregion

        #region  删除指定名称的数据库文件以及日志文件 
        /// <summary> 
        /// 删除指定名称的数据库文件以及日志文件 
        /// </summary> 
        /// <param name="dbName">数据库名称</param> 
        public static void DropDatabase(string dbName)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO  ");
            dbSql.Append("DROP DATABASE @dbName");
            //设置参数列表 
            SqlParameter[] parameter = { new SqlParameter("@dbName", dbName) };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), parameter);
        }
        #endregion

        #region  备份数据库 
        /// <summary> 
        /// 备份数据库 
        /// </summary> 
        /// <param name="dbName">数据库文件名</param> 
        /// <param name="dbFileName">路经包括盘符和文件名以及扩展名称一般为“_dat”</param> 
        public static void BackupDatabase(string dbName, string dbFileName)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO  ");
            dbSql.Append("BACKUP DATABASE @dbName TO DISK ='@dbFileName'");

            //设置参数列表 
            SqlParameter[] parameter =  
                { 
                    new SqlParameter("@dbName", dbName),  
                    new SqlParameter("@dbFileName", dbFileName) 
                };

            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), parameter);
        }

        #endregion

        #region  恢复数据库 
        /// <summary> 
        /// 恢复数据库 
        /// </summary> 
        /// <param name="dbName">数据库名</param> 
        /// <param name="dbFileName">路经包括盘符和文件名以及扩展名称一般为“_dat”</param> 
        public static void RestoreDatabase(string dbName, string dbFileName)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO  ");
            dbSql.Append("restore database @dbName from disk='@dbFileName'  WITH REPLACE,RECOVERY");

            //设置参数列表 
            SqlParameter[] parameter =  
                { 
                    new SqlParameter("@dbName", dbName),  
                    new SqlParameter("@dbFileName", dbFileName) 
                };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);

        }
        #endregion

        #region  附加数据库文件 
        /// <summary> 
        /// 附加数据库文件 
        /// </summary> 
        /// <param name="newDbName">附加时的新名称可以是原名，也可以得新取一个新名称</param> 
        /// <param name="dbFileName">数据文件的路径包括盘符和文件名以及扩展名</param> 
        /// <param name="logFileName">日志文件的路径包括盘符和文件名以及扩展名</param> 
        public static void OnlineDatabase(string newDbName, string dbFileName, string logFileName)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO  ");
            dbSql.Append("EXEC sp_attach_db @ newDbName,'@dbFileName','@logFileName'");

            //设置参数列表 
            SqlParameter[] parameter =  
                { 
                    new SqlParameter("@dbFileName", dbFileName),  
                    new SqlParameter("@logFileName", logFileName) 
                };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);

        }
        #endregion

        #region  分离数据库文件 
        /// <summary> 
        /// 分离数据库文件 
        /// </summary> 
        /// <param name="dbName">数据库名称</param> 
        public static void OfflineDatabase(string dbName)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO  ");
            dbSql.Append(" exec  sp_detach_db '@dbName' ");
            //设置参数列表 
            SqlParameter[] parameter = { new SqlParameter("@dbName", dbName) };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);

        }
        #endregion

        #region  重新设置用户的密码 
        /// <summary> 
        /// 重新设置用户的密码 
        /// </summary> 
        /// <param name="newPassword">新密码</param> 
        /// <param name="userName">登录用户名</param> 
        public static void ResetPassword(string newPassword, string userName)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE master ");
            dbSql.Append("  GO  ");
            dbSql.Append("EXEC   sp_password null,'@newPassword','@userName'");

            //设置参数列表 
            SqlParameter[] parameter =  
                { 
                    new SqlParameter("@newPassword", newPassword), 
                    new SqlParameter("@userName", userName)  
                };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);
        }
        #endregion

        #region  创建新用户 

        /// <summary> 
        /// 创建新用户
        /// </summary> 
        /// <param name="dbName">数据库名称</param> 
        /// <param name="newPassword">新密码</param> 
        /// <param name="userName">登录用户名</param> 
        public static void CreateDbUser(string dbName, string userName, string passWord)
        {
            StringBuilder dbSql = new();
            //设置当前数据库 
            dbSql.Append("USE  " + dbName);
            dbSql.Append("  GO  ");
            dbSql.Append("EXEC sp_addlogin N'@userName','@passWord'");
            dbSql.Append("EXEC sp_grantdbaccess N'@userName'");

            //设置参数列表 
            SqlParameter[] parameter =  
                {  
                    new SqlParameter("@dbName",userName), 
                    new SqlParameter("@userName", userName), 
                    new SqlParameter("@passWord", passWord) 
                };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);

        }

        #endregion

        #region  给指定数据的指定用户授于本数据库的所有操作权限 
        /// <summary> 
        /// 给指定数据的指定用户授于本数据库的所有操作权限 
        /// </summary> 
        /// <param name="dbName">数据库名称</param> 
        /// <param name="userName">用户名称</param> 
        public static void AddRoleToDbUser(string dbName, string userName)
        {
            StringBuilder dbSql = new();

            //设置当前数据库 
            dbSql.Append("USE " + dbName);
            dbSql.Append("GO ");
            dbSql.Append("EXEC sp_addrolemember N'@dbName', N'@userName'");


            //设置参数列表 
            SqlParameter[] parameter =  
                {  
                    new SqlParameter("@dbName",userName), 
                    new SqlParameter("@userName", userName) 
                };
            DBSqlOperate.ExecteNonQueryText(dbSql.ToString().Trim(), null);
        }
        #endregion
    }
}

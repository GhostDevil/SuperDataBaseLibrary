namespace SuperDataBase
{
    /// <summary>
    /// 日 期:2015-08-05
    /// 作 者:不良帥
    /// 描 述:数据库连接字符辅助
    /// </summary>
    public static class ConnectionStrHelper
    {
        #region  Oledb 
        /// <summary>
        /// 获取oledb连接本地数据的字符串（OLEDB.4.0）
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbLocal(string mdbPath)
        {

            return string.Format("Provider = Microsoft.Jet.OLEDB.4.0; DataSource = {0}", mdbPath);
        }
        /// <summary>
        /// 获取oledb连接本地Access数据的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbLocalAccess(string mdbPath, string userId,string pwd)
        {

            return string.Format("Driver={Microsoft Access Driver(*.mdb)};Dbq={0};Uid={1};Pwd={2};", mdbPath, userId,pwd);
        }
        /// <summary>
        /// 获取oledb连接本地Access系统数据的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbLocalSystemAccess(string mdbPath)
        {

            return string.Format("Driver={Microsoft Access Driver(*.mdb)};Dbq={0};", mdbPath);
        }
        /// <summary>
        /// 获取oledb连接Excel系统数据的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbLocalSystemExcel(string xlsPath, string defaultDir)
        {

            return string.Format("Driver={Microsoft Access Driver(*.xls)};DriverId=790;Dbq={0};DefaultDir={1};", xlsPath, defaultDir);
        }
        /// <summary>
        /// 获取oledb连接Oracle数据的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbOracle(string server, string userId, string pwd)
        {
            return string.Format("Driver={Microsoft ODBC for oracle};" + "Server=OracleServer.{0};Uid={1};Pwd={2};", server,userId,pwd);
        }
        /// <summary>
        /// 获取oledb连接SqlServer数据的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbSql(string mdbPath, string server, string userId, string pwd)
        {
            return string.Format("Driver={Sql Server};Server={0};Database={1};Uid={2};Pwd={3};", server, mdbPath, userId, pwd); 
        }
        /// <summary>
        /// 获取oledb连接Visual FoxPro的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbVisualFoxPro(string dbcName)
        {

            return string.Format("Driver={Microsoft Visual FoxPro Driver};SourceType=DBC;SourceDB={0};Exclusive=No;", dbcName);
        }
        /// <summary>
        /// 获取oledb连接MSSQL的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbMSSQL(string dataSourceName,string dbName, string userId, string pwd)
        {

            return string.Format("Provider=SQLOLEDB;data source={0};initial catalog={1};userid={2};password={3};", dataSourceName, dbName, userId, pwd);
        }
        /// <summary>
        /// 获取oledb连接MSText的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOledbMSText(string dataSourcePath)
        {

            return string.Format("Provider=microsof.jet.oledb.4.0;data source={0};Extended Properties'text;FMT=Delimited'", dataSourcePath);
        }
        #endregion

        #region  Odbc 
        /// <summary>
        /// 获取Odbc连接dBase的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcdBase(string dbfPath)
        {

            return string.Format("Driver={microsoft dbase driver(*.dbf)};driverid=277;dbq={0};", dbfPath);
        }
        /// <summary>
        /// 获取Odbc连接MSSQL的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcMSSQL(string serverName,string dbName, string userId, string pwd)
        {

            return string.Format("Driver={sql server};server={0};database={1};uid={2};pwd={3};", serverName, dbName, userId, pwd);
        }
        /// <summary>
        /// 获取Odbc连接MSText的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcMSText (string dbqPath)
        {

            return string.Format("Driver={microsoft text driver(*.txt; *.csv)};dbq={0};extensions=asc,csv,tab,txt;Persist SecurityInfo=false;", dbqPath);
        }
        /// <summary>
        /// 获取Odbc连接MySQL的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcMySQL(string dataBaseName,  string userId, string pwd, string option)
        {

            return string.Format("Driver={mysql};database={0};uid={1};pwd={2};option={3};", dataBaseName,userId, pwd, option);
        }
        /// <summary>
        /// 获取Odbc连接SQLite的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcSQLite(string dataBasePath)
        {

            return string.Format("Driver={SQLite3 Odbc Driver};Database={0}", dataBasePath);
        }
        /// <summary>
        /// 获取Odbc连接PostgreSQL的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcPostgreSQL(string serverIp,string dataBaseName, string userId, string pwd)
        {

            return string.Format("Driver={PostgreSQL ANSI};server={0};uid={1};pwd={2};database={3}", serverIp, userId, pwd, dataBaseName);
        }
        /// <summary>
        /// 获取Odbc连接Access数据库的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcAccess(string mdbPath,string userId, string pwd)
        {

            return string.Format("Driver={microsoft access driver(*.mdb)};dbq={0};uid={1};pwd={2};", mdbPath, userId, pwd);
        }
        /// <summary>
        /// 获取Odbc连接Oracle数据库的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcOracle(string dataBaseName, string userId, string pwd)
        {

            return string.Format("Driver={microsoft Odbc for oracle};server=oraclesever.{0};uid={1};pwd={2};", dataBaseName, userId, pwd);
        }
        /// <summary>
        /// 获取Odbc连接Visual Foxpro数据库的字符串
        /// </summary>
        /// <returns></returns>
        public static string GetStrForOdbcVisualFoxpro(string dbcName)
        {

            return string.Format("Driver={microsoft Visual Foxpro driver};sourcetype=DBC;sourceDB={0};Exclusive=No;", dbcName);
        }
        #endregion

        #region  Oracle 
        /// <summary>
        /// 获取oracle数据库连接
        /// </summary>
        /// <param name="dbIp">数据库ip</param>
        /// <param name="dbPort">数据库端口</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="userId">数据库用户id</param>
        /// <param name="userPwd">数据库用户密码</param>
        /// <returns></returns>
        public static string GetStrForOracle(string dbIp, string dbPort, string dbName, string userId, string userPwd)
        {
            return string.Format("Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1})))(CONNECT_DATA = (SERVICE_NAME = {2}))); Persist Security Info = True; User ID = {3}; Password = {4};", dbIp, dbPort, dbName, userId, userPwd);
        }
        #endregion

        #region  Sql 
        /// <summary>
        /// 设置连接字符串
        /// </summary>
        /// <param name="server">数据库地址</param>
        /// <param name="uid">用户</param>
        /// <param name="pwd">密码</param>
        /// <param name="dataBase">数据库</param>
        public static string GetStrForSqlserver(string server, string uid, string pwd, string dataBase)
        {
            return string.Format("server={0};uid={1};pwd={2};database={3}", server, uid, pwd, dataBase);
        }
        #endregion
    }
}

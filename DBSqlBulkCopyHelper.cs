using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace SuperDataBase
{
    /// <summary>
    /// SqlBulkCopy 海量数据插入辅助类(调用该方法需要注意，DataTable中的字段名称必须和数据库中的字段名称一一对应)
    /// </summary>
    public static class DBSqlBulkCopyHelper
    {
        /// <summary>
        /// 海量数据插入方法
        /// </summary>
        /// <param name="connectstring">数据连接字符串</param>
        /// <param name="table">内存表数据</param>
        /// <param name="tableName">目标数据的名称</param>
        /// <param name="batchSize">一次执行的条数</param>
        public static void AddData(string connectstring, DataTable table, string tableName, int batchSize = 4000)
        {
            if (table != null && table.Rows.Count > 0)
            {
                using (SqlBulkCopy bulk = new SqlBulkCopy(connectstring))
                {
                    bulk.BatchSize = batchSize;
                    bulk.BulkCopyTimeout = 100;
                    bulk.DestinationTableName = tableName;
                    bulk.WriteToServer(table);
                }
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="connectstring">数据库连接字符串</param>
        /// <param name="list">数据集合</param>
        /// <param name="TabelName">数据表名</param>
        public static void UpdateData<T>(string connectstring, List<T> list, string tableName)
        {
            DataTable dt = new DataTable("MyTable");
            dt = ConvertToDataTable(list);
            using (SqlConnection conn = new SqlConnection(connectstring))
            {
                using (SqlCommand command = new SqlCommand("", conn))
                {
                    try
                    {
                        conn.Open();
                        command.CommandText = "CREATE TABLE #TmpTable(...)";
                        command.ExecuteNonQuery();
                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = tableName;
                            bulkcopy.WriteToServer(dt);
                            bulkcopy.Close();
                        }
                        // Updating destination table, and dropping temp table
                        command.CommandTimeout = 300;
                        command.CommandText = "UPDATE T SET ... FROM " + tableName + " T INNER JOIN #TmpTable Temp ON ...; DROP TABLE #TmpTable;";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="connectstring">链接字符串</param>
        /// <param name="list">添加数据集合</param>
        /// <param name="tableName">数据表名</param>
        public static void InsertData<T>(string connectstring, List<T> list, string tableName)
        {
            DataTable dt = new DataTable("MyTable");
            dt = ConvertToDataTable(list);
            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connectstring))
            {
                bulkcopy.BulkCopyTimeout = 660;
                bulkcopy.DestinationTableName = tableName;
                bulkcopy.WriteToServer(dt);
            }
        }
        /// <summary>
        /// 将泛型集合转换为Table
        /// </summary>
        /// <typeparam name="T">泛型集合类型</typeparam>
        /// <param name="data">泛型集合</param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// 使用 SqlBulkCopy 向 destinationTableName 表插入数据
        /// </summary>
        /// <typeparam name="TModel">必须拥有与目标表所有字段对应属性</typeparam>
        /// <param name="conn"></param>
        /// <param name="modelList">要插入的数据</param>
        /// <param name="batchSize">SqlBulkCopy.BatchSize</param>
        /// <param name="destinationTableName">如果为 null，则使用 TModel 名称作为 destinationTableName</param>
        /// <param name="bulkCopyTimeout">SqlBulkCopy.BulkCopyTimeout</param>
        /// <param name="externalTransaction">要使用的事务</param>
        public static void BulkCopy<TModel>(this SqlConnection conn, List<TModel> modelList, int batchSize, string destinationTableName = null, int? bulkCopyTimeout = null, SqlTransaction externalTransaction = null)
        {
            bool shouldCloseConnection = false;

            if (string.IsNullOrEmpty(destinationTableName))
                destinationTableName = typeof(TModel).Name;

            DataTable dtToWrite = ConvertToDataTable(modelList); //ToSqlBulkCopyDataTable(modelList, conn, destinationTableName);

            SqlBulkCopy sbc = null;

            try
            {
                if (externalTransaction != null)
                    sbc = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, externalTransaction);
                else
                    sbc = new SqlBulkCopy(conn);

                using (sbc)
                {
                    sbc.BatchSize = batchSize;
                    sbc.DestinationTableName = destinationTableName;

                    if (bulkCopyTimeout != null)
                        sbc.BulkCopyTimeout = bulkCopyTimeout.Value;

                    if (conn.State != ConnectionState.Open)
                    {
                        shouldCloseConnection = true;
                        conn.Open();
                    }

                    sbc.WriteToServer(dtToWrite);
                }
            }
            finally
            {
                if (shouldCloseConnection && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        //public static DataTable ToSqlBulkCopyDataTable<TModel>(List<TModel> modelList, SqlConnection conn, string tableName)
        //{
        //    DataTable dt = new DataTable();

        //    Type modelType = typeof(TModel);

        //    List<SysColumn> columns = GetTableColumns(conn, tableName);
        //    List<PropertyInfo> mappingProps = new List<PropertyInfo>();

        //    var props = modelType.GetProperties();
        //    for (int i = 0; i < columns.Count; i++)
        //    {
        //        var column = columns[i];
        //        PropertyInfo mappingProp = props.Where(a => a.Name == column.Name).FirstOrDefault();
        //        if (mappingProp == null)
        //            throw new Exception(string.Format("model 类型 '{0}'未定义与表 '{1}' 列名为 '{2}' 映射的属性", modelType.FullName, tableName, column.Name));

        //        mappingProps.Add(mappingProp);
        //        Type dataType = GetUnderlyingType(mappingProp.PropertyType);
        //        if (dataType.IsEnum)
        //            dataType = typeof(int);
        //        dt.Columns.Add(new DataColumn(column.Name, dataType));
        //    }

        //    foreach (var model in modelList)
        //    {
        //        DataRow dr = dt.NewRow();
        //        for (int i = 0; i < mappingProps.Count; i++)
        //        {
        //            PropertyInfo prop = mappingProps[i];
        //            object value = prop.GetValue(model);

        //            if (GetUnderlyingType(prop.PropertyType).IsEnum)
        //            {
        //                if (value != null)
        //                    value = (int)value;
        //            }

        //            dr[i] = value ?? DBNull.Value;
        //        }

        //        dt.Rows.Add(dr);
        //    }

        //    return dt;
        //}
        //static List<SysColumn> GetTableColumns(SqlConnection sourceConn, string tableName)
        //{
        //    string sql = string.Format("select * from syscolumns inner join sysobjects on syscolumns.id=sysobjects.id where sysobjects.xtype='U' and sysobjects.name='{0}' order by syscolumns.colid asc", tableName);

        //    List<SysColumn> columns = new List<SysColumn>();
        //    using (SqlConnection conn = (SqlConnection)((ICloneable)sourceConn).Clone())
        //    {
        //        SqlCommand command = new SqlCommand(sql,conn);
        //        conn.Open();
        //        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
        //        {
        //            while (reader.Read())
        //            {
        //                SysColumn column = new SysColumn
        //                {
        //                    Name = reader.GetDbValue("name"),
        //                    ColOrder = reader.GetDbValue("colorder")
        //                };

        //                columns.Add(column);
        //            }
        //        }
        //        conn.Close();
        //    }

        //    return columns;
        //}

        //static Type GetUnderlyingType(Type type)
        //{
        //    Type unType = Nullable.GetUnderlyingType(type); ;
        //    if (unType == null)
        //        unType = type;

        //    return unType;
        //}

        //class SysColumn
        //{
        //    public string Name { get; set; }
        //    public int ColOrder { get; set; }
        //}
    }
}

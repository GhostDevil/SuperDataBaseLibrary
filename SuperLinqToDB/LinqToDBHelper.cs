using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using LinqEx=SuperFramework.SuperLinq;

namespace SuperDataBase.SuperLinqToDB
{
    /// <summary>
    /// Linq通用数据访问类
    /// <para>where：说明指代的类型</para>
    /// <para>new：限定必须有一个不带参数的构造函数</para>
    /// </summary>
    /// <typeparam name="Tdb">指定Tdb来代替后面要使用的数据上下文(指代)</typeparam>
    public class LinqToDBHelper<Tdb> where Tdb : DataContext, new()
    {

        //private static string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
        /// <summary>
        /// 数据上下文对象
        /// </summary>
        Tdb db = null;
        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="conString">数据库连接字符串</param>
        public LinqToDBHelper(string conString)
        {
            if (string.IsNullOrEmpty(conString))
            {
                throw new ArgumentException($"{nameof(conString)} is null or empty.", nameof(conString));
            }
            if (db is null)
                db = new Tdb();
            db.Connection.ConnectionString = conString;
        }
        public void Dispose()
        {
            if (db != null)
                db.Dispose();
        }
        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <returns></returns>
        public List<T> GetList<T>() where T : class => db.GetTable<T>().ToList();

        /// <summary>
        /// 按条件查询
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="predicate">Lambda表达式条件</param>
        /// <returns></returns>
        public List<T> GetList<T>(Expression<Func<T, bool>> predicate) where T : class => db.GetTable<T>().Where(predicate).ToList();

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="predicate">Lambda表达式条件</param>
        /// <returns></returns>
        public T GetEntity<T>(Expression<Func<T, bool>> predicate) where T : class => db.GetTable<T>().Where(predicate).FirstOrDefault();

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="entity">实体对象</param>
        public void InsertEntity<T>(T entity) where T : class
        {
            try
            {
                //将对象保存到上下文当中
                db.GetTable<T>().InsertOnSubmit(entity);
                //提交更改
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 修改实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="entity">实体对象</param>
        public void UpdateEntity<T>(T entity) where T : class
        {
            try
            {
                //将新实体附加到上下文
                db.GetTable<T>().Attach(entity);
                //刷新数据库
                db.Refresh(RefreshMode.KeepCurrentValues, entity);
                //提交更改
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="predicate">Lambda表达式条件</param>
        public void DeleteEntity<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                //获取要删除的实体
                var entity = db.GetTable<T>().Where(predicate).FirstOrDefault();
                if (entity == null) return;
                db.GetTable<T>().DeleteOnSubmit(entity);
                db.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 根据指定属性名称对序列进行排序
        /// </summary>
        /// <typeparam name="TSource">数据实体类</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="property">属性名称</param>
        /// <param name="descending">是否降序</param>
        /// <returns></returns>
        public List<T> OrderBy<T>(List<T> source, string property, bool descending) where T : class => LinqEx.LinqEx<T>.OrderBy(source.AsQueryable(), property, descending).ToList();
        /// <summary>
        /// 排序并分页 
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="gridPager">操作参数</param>
        /// <returns></returns>
        public List<T> OrderAndPaging<T>(List<T> source, LinqEx.LinqEx<T>.GridPager gridPager) where T : class => LinqEx.LinqEx<T>.SortingAndPaging(source.AsQueryable(), gridPager).ToList();
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        public List<T> DataPaging<T>(List<T> source, int pageNumber, int pageSize) where T : class => LinqEx.LinqEx<T>.DataPaging(source.AsQueryable(), pageNumber, pageSize).ToList();
        /// <summary>
        /// 撤销添加
        /// </summary>
        public void CancelInsert()
        {
            ChangeSet changeSet = db.GetChangeSet();
            foreach (var item in changeSet.Inserts)
            {
                db.GetTable(item.GetType()).DeleteOnSubmit(item);
            }
        }
        /// <summary>
        /// 撤销更新
        /// </summary>
        public void CancelUpdate()
        {
            ChangeSet changeSet = db.GetChangeSet();
            foreach (var item in changeSet.Updates)
            {
                db.Refresh(RefreshMode.OverwriteCurrentValues, item);
            }
        }
        /// <summary>
        /// 撤销删除
        /// </summary>
        public void CancelDelte()
        {
            ChangeSet changeSet = db.GetChangeSet();
            foreach (var item in changeSet.Deletes)
            {
                db.GetTable(item.GetType()).InsertOnSubmit(item);
            }
        }

        #region 静态函数
        /// <summary>   
        /// 查询全部数据   
        /// </summary>  
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <returns></returns>   
        public static List<T> ReturnAllRows<T>(string conString) where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                return database.GetTable<T>().ToList();
            }
        }
        /// <summary>   
        /// 查看是否存在数据   
        /// </summary>   
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="predicate">Lambda表达式条件</param>   
        /// <returns></returns>   
        public static bool EntityExists<T>(string conString, Expression<Func<T, bool>> predicate)
        where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                return database.GetTable<T>().Where(predicate).Count() > 0;
            }
        }
        /// <summary>   
        /// 有条件的查询数据List<typeparamref name="数据源:DataContext"/>  Filter<typeparamref name="表的类名:orders"/>    
        /// 最后面的可以输入LINQ查询语句（p=>p.order_id="00001")或者(form o in orders where o.order_id>100 sleect o);   
        /// </summary>  
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="predicate">Lambda表达式条件</param>   
        /// <returns></returns>   
        public static List<T> Filter<T>(string conString, Expression<Func<T, bool>> predicate)
        where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                return database.GetTable<T>().Where(predicate).ToList();
            }
        }
        /// <summary>   
        /// 插入数据  
        /// </summary>   
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="entity">实体对象</param>   
        public static void Insert<T>(string conString, T entity) where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                database.GetTable<T>().InsertOnSubmit(entity);
                database.SubmitChanges();
            }
        }
        /// <summary>
        /// 修改实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="entity">实体对象</param>
        public static void Update<T>(string conString, T entity) where T : class
        {
            try
            {
                using (Tdb database = new Tdb())
                {
                    //将新实体附加到上下文
                    database.GetTable<T>().Attach(entity);
                    //刷新数据库
                    database.Refresh(RefreshMode.KeepCurrentValues, entity);
                    //提交更改
                    database.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>   
        /// 删除指定数据,支持多条删除  Expression<Func<T, bool>> predicate就是查询语句，只能用：p=>p.user_id=="123"的语句！   
        /// </summary>  
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="predicate">Lambda表达式条件</param>   
        ///返回值：0 成功    -1 失败   
        public static int Delete<T>(string conString, Expression<Func<T, bool>> predicate)
        where T : class
        {
            if (EntityExists(conString, predicate))
            {
                using (Tdb database = new Tdb())
                {
                    database.Connection.ConnectionString = conString;
                    T t = database.GetTable<T>().Where(predicate).Single();
                    database.GetTable<T>().DeleteOnSubmit(t);
                    database.SubmitChanges();
                }
                return 0;
            }
            return -1;
        }
        /// <summary>   
        /// 返回分页面数据   
        /// </summary>   
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="pageSize">一次取多少条数据</param>   
        /// <param name="currerCount">当前提交的页数字</param>   
        /// <returns></returns>   
        public static List<T> GetpPgeRow<T>(string conString, int pageSize, int currerCount) where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                return database.GetTable<T>().Skip((currerCount - 1) * pageSize).Take(pageSize).ToList();
            }
        }
        /// <summary>
        /// 根据指定属性名称对序列进行排序
        /// </summary>
        /// <typeparam name="TSource">数据实体类</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="property">属性名称</param>
        /// <param name="descending">是否降序</param>
        /// <returns>返回排序后集合</returns>
        public static List<T> SortBy<T>(List<T> source, string property, bool descending) where T : class => LinqEx.LinqEx<T>.OrderBy(source.AsQueryable(), property, descending).ToList();
        /// <summary>
        /// 排序并分页 
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="gridPager">操作参数</param>
        /// <returns>返回分页后集合</returns>
        public static List<T> SortingAndPaging<T>(List<T> source, LinqEx.LinqEx<T>.GridPager gridPager) where T : class => LinqEx.LinqEx<T>.SortingAndPaging(source.AsQueryable(), gridPager).ToList();

        /// <summary>   
        /// 返回多少条数据   
        /// </summary>   
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="predicate">Lambda表达式条件</param>   
        /// <returns>返回数据量</returns>   
        public static int GetPageCount<T>(string conString, Expression<Func<T, bool>> predicate) where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                return database.GetTable<T>().Where(predicate).Count();
            }
        }
        /// <summary>   
        /// 一次插入多条数据    
        /// </summary>   
        /// <param name="conString">数据库连接字符串</param>
        /// <typeparam name="T">数据实体类</typeparam>   
        /// <param name="entity">实体对象</param>   
        public static void InsetRows<T>(string conString, List<T> entity) where T : class
        {
            using (Tdb database = new Tdb())
            {
                database.Connection.ConnectionString = conString;
                foreach (T t in entity.ToList())
                {
                    database.GetTable<T>().InsertOnSubmit(t);
                    database.SubmitChanges();
                }
            }
        }
        #endregion
    }
}

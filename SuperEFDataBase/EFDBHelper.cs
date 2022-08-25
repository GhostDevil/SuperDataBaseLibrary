using SuperFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace SuperDataBase.SuperEFDataBase
{
    /// <summary>
    /// EntityFramework增删改查基本操作辅助
    /// </summary>
    /// <typeparam name="T">实体类</typeparam>
    public static class EFDBHelper<T> where T : class
    {
        #region 事务批量执行
        /// <summary>
        /// 事务批量执行
        /// </summary>
        /// <returns></returns>
        public static int SaveChange(DbContext db)
        {
            return db.SaveChanges();
        }
        #endregion

        #region sql语句
        /// <summary>
        /// 执行增加,删除,修改操作(或调用存储过程)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static int ExecuteSql(DbContext db,string sql, params object[] pars)
        {
            return db.Database.ExecuteSqlCommand(sql, pars);
        }

        /// <summary>
        /// 执行查询操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static List<T> ExecuteQuery(DbContext db, string sql, params object[] pars)
        {
            return db.Database.SqlQuery<T>(sql, pars).ToList();
        }


        /// <summary>
        /// 执行查询操作(结果为单一实体)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static T SqlQuery(DbContext db, string sql, params object[] paras)
        {
            return db.Database.SqlQuery<T>(sql, paras).FirstOrDefault();
        }
        /// <summary>
        /// 执行其他命令 如 存储过程,函数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="procName"></param>
        /// <param name="type"></param>
        /// <param name="paras">输入输出参数</param>
        /// <returns></returns>
        public static object ExecuteByOther(DbContext db, string procName, System.Data.CommandType type, params object[] paras)
        {
            try
            {
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandType = type;
                cmd.CommandText = procName;
                if (paras.Length > 0)
                    cmd.Parameters.AddRange(paras);
                cmd.Connection.Open();
                object ret = cmd.ExecuteNonQuery();
                var par = paras[paras.Length - 1];
                cmd.Connection.Close();
                return par;
            }
            catch (Exception ex) { return ex.InnerException.Message; }
        }
        #endregion

        #region 新增
        public static int Add(DbContext db, T model)
        {
            DbSet<T> dst = db.Set<T>();
            dst.Add(model);
            return db.SaveChanges();
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model">需要新增的实体</param>
        public static void AddNoSave(DbContext db, T model)
        {
            db.Set<T>().Add(model);
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model">需要删除的实体</param>
        public static void DelNoSave(DbContext db, T model)
        {
            db.Entry(model).State = EntityState.Deleted;
        }
        
        /// <summary>
        /// 删除(适用于先查询后删除的单个实体)
        /// </summary>
        /// <param name="model">需要删除的实体</param>
        /// <returns></returns>
        public static int Del(DbContext db, T model)
        {
            db.Set<T>().Attach(model);
            db.Set<T>().Remove(model);
            return db.SaveChanges();
        }
       
        /// <summary>
        /// 根据条件删除(支持批量删除)
        /// </summary>
        /// <param name="delWhere">传入Lambda表达式(生成表达式目录树)</param>
        /// <returns></returns>
        public static int DelBy(DbContext db, Expression<Func<T, bool>> delWhere)
        {
            List<T> listDels = db.Set<T>().Where(delWhere).ToList();
            listDels.ForEach(d =>
            {
                db.Set<T>().Attach(d);
                db.Set<T>().Remove(d);
            });
            return db.SaveChanges();
        }
        
        #endregion

        #region 查询
        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="whereLambda">查询条件(lambda表达式的形式生成表达式目录树)</param>
        /// <returns></returns>
        public static List<T> GetListBy(DbContext db, Expression<Func<T, bool>> whereLambda)
        {
            return db.Set<T>().Where(whereLambda).ToList();
        }
        /// <summary>
        /// 根据条件排序和查询
        /// </summary>
        /// <typeparam name="Tkey">排序字段类型</typeparam>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="orderLambda">排序条件</param>
        /// <param name="isAsc">升序or降序</param>
        /// <returns></returns>
        public static List<T> GetListBy<Tkey>(DbContext db, Expression<Func<T, bool>> whereLambda, Expression<Func<T, Tkey>> orderLambda, bool isAsc = true)
        {
            List<T> list = null;
            if (isAsc)
            {
                list = db.Set<T>().Where(whereLambda).OrderBy(orderLambda).ToList();
            }
            else
            {
                list = db.Set<T>().Where(whereLambda).OrderByDescending(orderLambda).ToList();
            }
            return list;
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="Tkey">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="orderLambda">排序条件</param>
        /// <param name="isAsc">升序or降序</param>
        /// <returns></returns>
        public static List<T> GetPageList<Tkey>(DbContext db, int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, Tkey>> orderLambda, bool isAsc = true)
        {

            List<T> list = null;
            if (isAsc)
            {
                list = db.Set<T>().Where(whereLambda).OrderBy(orderLambda)
               .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                list = db.Set<T>().Where(whereLambda).OrderByDescending(orderLambda)
              .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
            return list;
        }
        /// <summary>
        /// 分页查询输出总行数
        /// </summary>
        /// <typeparam name="Tkey">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="orderLambda">排序条件</param>
        /// <param name="isAsc">升序or降序</param>
        /// <returns></returns>
        public static List<T> GetPageList<Tkey>(DbContext db, int pageIndex, int pageSize, ref int rowCount, Expression<Func<T, bool>> whereLambda, Expression<Func<T, Tkey>> orderLambda, bool isAsc = true)
        {
            int count = 0;
            List<T> list = null;
            count = db.Set<T>().Where(whereLambda).Count();
            if (isAsc)
            {
                var iQueryList = db.Set<T>().Where(whereLambda).OrderBy(orderLambda)
                   .Skip((pageIndex - 1) * pageSize).Take(pageSize);

                list = iQueryList.ToList();
            }
            else
            {
                var iQueryList = db.Set<T>().Where(whereLambda).OrderByDescending(orderLambda)
                 .Skip((pageIndex - 1) * pageSize).Take(pageSize);
                list = iQueryList.ToList();
            }
            rowCount = count;
            return list;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model">修改后的实体</param>
        /// <returns></returns>
        public static void ModifyNoSave(DbContext db, T model)
        {
            db.Entry(model).State = EntityState.Modified;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model">修改后的实体</param>
        /// <returns></returns>
        public static int Modify(DbContext db, T model)
        {
            db.Entry(model).State = EntityState.Modified;
            return db.SaveChanges();
        }

        /// <summary>
        /// 单实体扩展修改（把不需要修改的列用LAMBDA数组表示出来）
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="ignoreProperties">不须要修改的相关字段</param>
        /// <returns>受影响的行数</returns>
        public static int Modify(DbContext db, T model, params Expression<Func<T, object>>[] ignoreProperties)
        {
            //using (DbContext db = new IDbContextFactory<>().GetDbContext())
            //{
            db.Set<T>().Attach(model);

            DbEntityEntry entry = db.Entry(model);
            entry.State = EntityState.Unchanged;

            Type t = typeof(T);
            List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();

            Dictionary<string, PropertyInfo> dicPros = new Dictionary<string, PropertyInfo>();
            proInfos.ForEach(
                p => dicPros.Add(p.Name, p)
                );

            if (ignoreProperties != null)
            {
                foreach (var ignorePropertyExpression in ignoreProperties)
                {
                    //根据表达式得到对应的字段信息
                    var ignorePropertyName = PropertyInfoHelper.GetPropertyInfo<T>(ignorePropertyExpression).Name;
                    dicPros.Remove(ignorePropertyName);
                }
            }

            foreach (string proName in dicPros.Keys)
            {
                entry.Property(proName).IsModified = true;
            }
            return db.SaveChanges();
            //}
        }

        /// <summary>
        /// 批量修改（非lambda）
        /// </summary>
        /// <param name="model">要修改实体中 修改后的属性 </param>
        /// <param name="whereLambda">查询实体的条件</param>
        /// <param name="proNames">lambda的形式表示要修改的实体属性名</param>
        /// <returns></returns>
        public static int ModifyBy(DbContext db, T model, Expression<Func<T, bool>> whereLambda, params string[] proNames)
        {
            List<T> listModifes = db.Set<T>().Where(whereLambda).ToList();
            Type t = typeof(T);
            List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            Dictionary<string, PropertyInfo> dicPros = new Dictionary<string, PropertyInfo>();
            proInfos.ForEach(p =>
            {
                if (proNames.Contains(p.Name))
                {
                    dicPros.Add(p.Name, p);
                }
            });
            foreach (string proName in proNames)
            {
                if (dicPros.ContainsKey(proName))
                {
                    PropertyInfo proInfo = dicPros[proName];
                    object newValue = proInfo.GetValue(model, null);
                    foreach (T m in listModifes)
                    {
                        proInfo.SetValue(m, newValue, null);
                    }
                }
            }
            return db.SaveChanges();
        }

        /// <summary>
        /// 批量修改（支持lambda）
        /// </summary>
        /// <param name="model">要修改实体中 修改后的属性 </param>
        /// <param name="whereLambda">查询实体的条件</param>
        /// <param name="proNames">lambda的形式表示要修改的实体属性名</param>
        /// <returns></returns>
        public static int ModifyBy(DbContext db, T model, Expression<Func<T, bool>> whereLambda, params Expression<Func<T, object>>[] proNames)
        {
            List<T> listModifes = db.Set<T>().Where(whereLambda).ToList();
            Type t = typeof(T);
            List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            Dictionary<string, PropertyInfo> dicPros = new Dictionary<string, PropertyInfo>();
            if (proNames != null)
            {
                foreach (var myProperyExp in proNames)
                {
                    var my_ProName = PropertyInfoHelper.GetPropertyInfo<T>(myProperyExp).Name; //new PropertyExpressionParser<T>(myProperyExp).Name;
                    proInfos.ForEach(p =>
                    {
                        if (p.Name.Equals(my_ProName))
                        {
                            dicPros.Add(p.Name, p);
                        }
                    });
                    if (dicPros.ContainsKey(my_ProName))
                    {
                        PropertyInfo proInfo = dicPros[my_ProName];
                        object newValue = proInfo.GetValue(model, null);
                        foreach (T m in listModifes)
                        {
                            proInfo.SetValue(m, newValue, null);
                        }
                    }
                }
            }
            return db.SaveChanges();
        }
        #endregion  
    }
}

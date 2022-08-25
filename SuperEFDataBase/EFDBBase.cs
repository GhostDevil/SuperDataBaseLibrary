using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace SuperDataBase.SuperEFDataBase
{
    /// <summary>
    /// 对EntityFramework增删改查基本操作的封装
    /// </summary>
    /// <typeparam name="t">实体对象</typeparam>
    public class EFDBBase<T> where T : class, new()
    {
        //上下文对象
        protected DbContext dbContext = null;
        public EFDBBase(DbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="t">实体对象</param>
        /// <returns></returns>
        public virtual int Add(T t)
        {
            dbContext.Entry(t).State = EntityState.Added;
            return dbContext.SaveChanges();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="t">实体对象</param>
        /// <returns></returns>
        public virtual int Update(T t)
        {
            dbContext.Entry(t).State = EntityState.Modified;
            return dbContext.SaveChanges();
        }
        /// <summary>
        ///  修改 个别 数据  
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <returns></returns>
        public virtual int Modify(T model, params string[] proNames)
        {
            DbEntityEntry entry = dbContext.Entry(model);
            entry.State = EntityState.Unchanged;
            foreach (string proName in proNames)
            {
                entry.Property(proName).IsModified = true;
            }
            dbContext.Configuration.ValidateOnSaveEnabled = false;
            return dbContext.SaveChanges();
        }
        /// <summary>
        ///  修改 多数 数据, 个别数据除外,  proNames 不写 则是 修改全部
        /// </summary>
        /// <param name="model">要修改的实体对象(对面 过滤null 值)</param>
        /// <param name="proNames">不需要要修改的 属性 名称</param>
        /// <returns></returns>
        public virtual int ModifyWithOutproNames(T model, params string[] proNames)
        {
            
            DbEntityEntry entry = dbContext.Entry(model);
            entry.State = EntityState.Unchanged;
            var properties = model.GetType().GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].PropertyType.Name.Contains("ICollection")// 排除 外键  
                    || proNames.Contains(properties[i].Name) || properties[i].GetValue(model, null) == null) continue;
                entry.Property(properties[i].Name).IsModified = true;
            }
            dbContext.Configuration.ValidateOnSaveEnabled = false;
            return dbContext.SaveChanges();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="t">实体对象</param>
        /// <returns></returns>
        public virtual int Delete(T t)
        {
            dbContext.Entry(t).State = EntityState.Deleted;
            return dbContext.SaveChanges();
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="whereFun"></param>
        /// <param name="selectFun"></param>
        /// <returns></returns>
        public virtual IQueryable<T> Query(Expression<Func<T, bool>> whereFun = null, Expression<Func<T, T>> selectFun = null)
        {
            IQueryable<T> result = dbContext.Set<T>();
            if (whereFun != null) result = result.Where(whereFun);
            if (selectFun != null) result = result.Select(selectFun);
            return result;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageNo">页码</param>
        /// <param name="pageCount">实体对象页数</param>
        /// <param name="whereFun">查询条件</param>
        /// <param name="selectFun">返回数据</param>
        /// <param name="orderByFun">排序</param>
        /// <param name="total">总数</param>
        /// <param name="isAsc">排序，默认升序</param>
        /// <returns>list</returns>
        public virtual List<T> PageQuery<S>(int pageNo, int pageCount, Expression<Func<T, bool>> whereFun,
            Expression<Func<T, T>> selectFun, Expression<Func<T, S>> orderByFun, out int total, bool isAsc = true)
        {
            total = 0;
            int startIndex = pageCount * (pageNo - 1);
            var list = dbContext.Set<T>().Where(whereFun);
            total = list.Count();
            if (total <= 0) return new List<T>();
            if (isAsc)
                list = list.OrderBy(orderByFun).Skip(startIndex).Take(pageCount).Select(selectFun);
            else
                list = list.OrderByDescending(orderByFun).Skip(startIndex).Take(pageCount).Select(selectFun);

            return list.ToList();
        }

    }
}

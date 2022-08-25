using NLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SuperDataBase.SuperLinqToDB
{
    /// <summary>
    /// Elinq ORM组件封装
    /// </summary>
    public class ELinqHelper
    {
        const string ConnectionStringName = "northwind";
        static DbConfiguration dbConfiguration;
        private IDbContext _dbContext;
        /// <summary>
        /// 构造初始化连接
        /// </summary>
        /// <param name="stringName">连接字符串</param>
        public ELinqHelper(string stringName = ConnectionStringName)
        {
            if (string.IsNullOrEmpty(stringName))
            {
                throw new ArgumentException($"{nameof(stringName)} is null or empty.", nameof(stringName));
            }

            dbConfiguration = DbConfiguration.Configure(stringName);
            _dbContext = dbConfiguration.CreateDbContext();
        }
        /// <summary>
        /// 注册实体到数据表的映射关系
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        public void AddClass<T>()
        {
            if (!dbConfiguration.HasClass(typeof(T)))
                dbConfiguration.AddClass<T>();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <returns></returns>        
        public List<T> List<T>()
        {
            AddClass<T>();
            return _dbContext.Set<T>().ToList();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="filter">Lambda表达式条件</param>
        /// <returns></returns>        
        public List<T> List<T>(Expression<Func<T, bool>> filter)
        {
            if (filter == null)
                throw new ArgumentNullException();
            AddClass<T>();
            return _dbContext.Set<T>().Where(filter).ToList();
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="filter">Lambda表达式条件</param>
        /// <param name="dynamic"></param>
        /// <returns></returns>        
        public List<T> Select<T>(Expression<Func<T, bool>> filter, Expression<Func<T, T>> dynamic)
        {
            if (filter == null || dynamic == null)
                throw new ArgumentNullException();
            AddClass<T>();
            return _dbContext.Set<T>().Where(filter).Select(dynamic).ToList();
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="filter">Lambda表达式条件</param>
        /// <param name="order">排序条件</param>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pagesize">页面大小</param>
        /// <returns></returns>        
        public List<T> PageList<T>(Expression<Func<T, bool>> filter, Expression<Func<T, DateTime>> order, int pageIndex, int pagesize)
        {
            AddClass<T>();
            return filter == null ? _dbContext.Set<T>().Skip((pageIndex - 1) * pagesize).Take(pagesize).OrderByDescending(order).ToList() : _dbContext.Set<T>().Where(filter).Skip((pageIndex - 1) * pagesize).Take(pagesize).OrderByDescending(order).ToList();
        }

        /// <summary>
        /// 获取单一实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="id">单一id条件或者联合id以数组形式</param>
        /// <returns></returns>        
        public T GetModel<T>(object id)
        {
            AddClass<T>();
            return _dbContext.Set<T>().Get(id);
        }
        /// <summary>
        /// 获取单一实体
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="filter">Lambda表达式条件</param>
        /// <returns></returns>        
        public T GetModel<T>(Expression<Func<T, bool>> filter)
        {
            AddClass<T>();
            return _dbContext.Set<T>().FirstOrDefault(filter);
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="model">实体对象</param>
        /// <returns>受影响行数</returns>        
        public int Insert<T>(T model)
        {
            AddClass<T>();
            return _dbContext.Set<T>().Insert(model);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="model">实体对象</param>
        /// <returns>受影响行数</returns>      
        public int Update<T>(T model)
        {
            AddClass<T>();
            return _dbContext.Set<T>().Update(model);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="obj">更新的实体对象</param>
        /// <param name="filter">Lambda表达式条件,除了id之外的附加条件</param>
        /// <returns>受影响行数</returns>      
        public int Update<T>(object obj, Expression<Func<T, bool>> filter)
        {
            AddClass<T>();
            return _dbContext.Set<T>().Update(obj,filter);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="filter">Lambda表达式条件</param>
        /// <returns>受影响行数</returns>      
        public int Delete<T>(Expression<Func<T, bool>> filter)
        {
            AddClass<T>();
            return _dbContext.Set<T>().Delete(filter);
        }

        /// <summary>
        /// 获取总数
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="filter">Lambda表达式条件</param>
        /// <returns>总数</returns>        
        public int Count<T>(Expression<Func<T, bool>> filter)
        {
            AddClass<T>();
            return filter == null ? _dbContext.Set<T>().Count() : _dbContext.Set<T>().Count(filter);
        }
    }
}

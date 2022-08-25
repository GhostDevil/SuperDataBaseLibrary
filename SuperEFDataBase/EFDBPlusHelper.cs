using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace SuperDataBase.SuperEFDataBase
{
    /// <summary>
    /// 如果需要删除数百个或数千个实体，使用实体框架删除可能会非常慢。实体在被删除之前先在上下文中加载，这对性能非常不利，然后逐个删除它们，从而使删除操作更加糟糕。EF+批删除删除单个数据库往返中的多行，而不加载上下文中的实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFDBPlusHelper<T> where T : class
    {
        /// <summary>
        /// 单个更新(适用于先查询后更新的单个实体)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="oldModel">需要更新的实体</param>
        /// <param name="newModel">需要更新的实体属性</param>
        /// <returns></returns>
        public static int UpPlus(DbContext db, T oldModel, T newModel)
        {
            return db.Set<T>().Where(o => o == oldModel).Update(o => newModel);

        }
        /// <summary>
        /// 单个更新异步执行(适用于先查询后更新的单个实体)
        /// </summary>
        /// <param name="db">上下文对象</param>
        /// <param name="oldModel">需要更新的实体</param>
        /// <param name="newModel">需要更新的实体属性</param>
        /// <returns></returns>
        public static async Task<int> UpPlusAsync(DbContext db, T oldModel, T newModel)
        {
            return await db.Set<T>().Where(o => o == oldModel).UpdateAsync(o => newModel);

        }
        /// <summary>
        /// 根据条件更新异步执行(支持批量删除)
        /// </summary>
        /// <param name="db">上下文对象</param>
        /// <param name="upWhere">需要更新的条件</param>
        ///<param name="newModel">需要更新的实体属性</param>
        ///<returns>返回受影响行数</returns>
        public static async Task<int> UpPlusAsyncBy(DbContext db, Expression<Func<T, bool>> upWhere, T newModel)
        {
            return await db.Set<T>().Where(upWhere).UpdateAsync(o => newModel);

        }
        /// <summary>
        /// 根据条件更新(支持批量删除)
        /// </summary>
        /// <param name="db">上下文对象</param>
        /// <param name="upWhere">需要更新的条件</param>
        ///<param name="newModel">需要更新的实体属性</param>
        ///<returns>返回受影响行数</returns>
        public static int UpPlusBy(DbContext db, Expression<Func<T, bool>> upWhere, T newModel)
        {
            return db.Set<T>().Where(upWhere).Update(o => newModel);
        }
        /// <summary>
        /// 删除(适用于先查询后删除的单个实体)
        /// </summary>
        /// <param name="db">上下文对象</param>
        /// <param name="model">需要删除的实体</param>
        ///<returns>返回受影响行数</returns>
        public static int DelPlus(DbContext db, T model)
        {
            return db.Set<T>().Where(o => o == model).Delete();
        }
        /// <summary>
        /// 根据条件删除(支持批量删除)
        /// </summary>
        /// <param name="db">上下文对象</param>
        /// <param name="delWhere">需要删除的条件</param>
        ///<returns>返回受影响行数</returns>
        public static int DelPlusBy(DbContext db, Expression<Func<T, bool>> delWhere)
        {
            return db.Set<T>().Where(delWhere).Delete();
        }

        /// <summary>
        /// 根据条件删除异步执行(支持批量删除)
        /// </summary>
        /// <param name="db">上下文对象</param>
        /// <param name="delWhere">需要删除的条件</param>
        /// <param name="batchSize">BatchSize属性设置要在单个批处理中删除的行数,适当的提高该值，会增加删除效率，默认4000</param>
        ///<returns>返回受影响行数</returns>
        public static async Task<int> DelPlusAsyncBy(DbContext db, Expression<Func<T, bool>> delWhere, int batchSize = 4000)
        {
            return await db.Set<T>().Where(delWhere).DeleteAsync(x => x.BatchSize=batchSize);
        }
        ///// <summary>
        ///// 根据条件删除异步执行(支持批量删除)
        ///// </summary>
        ///// <param name="db">上下文对象</param>
        ///// <param name="delWhere">需要删除的条件</param>
        ///// <param>batchDelayInterval属性设置启动下一个删除批处理之前等待的时间，默认0(以毫秒为单位)</param>
        /////<returns>返回受影响行数</returns>
        //public static async Task<int> DelPlusAsyncBy(DbContext db, Expression<Func<T, bool>> delWhere, int batchDelayInterval = 0)
        //{
        //    return await db.Set<T>().Where(delWhere).DeleteAsync(x => x.BatchDelayInterval = batchDelayInterval);
        //}
    }
}

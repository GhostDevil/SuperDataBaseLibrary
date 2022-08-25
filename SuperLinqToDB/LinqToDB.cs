using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;

namespace SuperDataBase.SuperLinqToDB
{
    public class LinqToDB
    {
        static DataContext _dc = null;

        #region 构造函数开始
        public LinqToDB(System.Data.IDbConnection connection) => _dc = new DataContext(connection);
        #endregion 构造函数结束

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate">Lambda表达式条件</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Query<TEntity>(Func<TEntity, bool> predicate) where TEntity : class => GetTable<TEntity>().Where(predicate).AsEnumerable();
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public Table<TEntity> GetTable<TEntity>() where TEntity : class => _dc.GetTable<TEntity>();
        /// <summary>
        /// 提交更改
        /// </summary>
        public void SubmitChanges()
        {
            try
            {
                _dc.SubmitChanges();
            }
            catch
            {
                foreach (ObjectChangeConflict occ in _dc.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.KeepCurrentValues);
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    occ.Resolve(RefreshMode.KeepChanges);
                }
                _dc.SubmitChanges();
            }
        }
        #region 封装Table方法开始
        /// <summary>
        /// 附加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        public void Attach<TEntity>(TEntity entity) where TEntity : class => GetTable<TEntity>().Attach(entity);
        /// <summary>
        /// 附加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="asModified">是否附加为已修改</param>
        public void Attach<TEntity>(TEntity entity, bool asModified) where TEntity : class => GetTable<TEntity>().Attach(entity, asModified);
        /// <summary>
        /// 附加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="original"></param>
        public void Attach<TEntity>(TEntity entity, TEntity original) where TEntity : class => GetTable<TEntity>().Attach(entity,original);
        /// <summary>
        /// 批量附加
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entities">实体对象集合</param>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : class => GetTable<TSubEntity>().AttachAll(entities);
        /// <summary>
        /// 批量附加
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entities">实体对象集合</param>
        /// <param name="asModified"></param>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : class => GetTable<TSubEntity>().AttachAll(entities, asModified);
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="TSubEntity"></typeparam>
        /// <param name="entities">实体对象集合</param>
        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : class => GetTable<TSubEntity>().DeleteAllOnSubmit(entities);
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        public void DeleteOnSubmit<TEntity>(TEntity entity) where TEntity : class => GetTable<TEntity>().DeleteOnSubmit(entity);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IEnumerator<TEntity> GetEnumerator<TEntity>() where TEntity : class => GetTable<TEntity>().AsEnumerable().GetEnumerator();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public ModifiedMemberInfo[] GetModifiedMembers<TEntity>(TEntity entity) where TEntity : class => GetTable<TEntity>().GetModifiedMembers(entity);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IBindingList GetNewBindingList<TEntity>() where TEntity : class => GetTable<TEntity>().GetNewBindingList();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns></returns>
        public TEntity GetOriginalEntityState<TEntity>(TEntity entity) where TEntity : class => GetTable<TEntity>().GetOriginalEntityState(entity);
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities">实体对象集合</param>
        public void InsertAllOnSubmit<TEntity>(IEnumerable<TEntity> entities) where TEntity : class => GetTable<TEntity>().InsertAllOnSubmit(entities);
        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">实体对象</param>
        public void InsertOnSubmit<TEntity>(TEntity entity) where TEntity : class => GetTable<TEntity>().InsertOnSubmit(entity);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "Linq2Db Ojbect!";
        #endregion 封装Table方法结束
    }
}

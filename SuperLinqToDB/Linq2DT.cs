using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperDataBase.SuperLinqToDB
{
    public class Linq2DT<TEntity> where TEntity : class
    {
        static System.Data.Linq.DataContext _dc = null;

        #region 构造函数开始
        public Linq2DT(System.Data.IDbConnection connection)
        {
            _dc = new System.Data.Linq.DataContext(connection);
        }
        #endregion 构造函数结束

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> Query(Func<TEntity, bool> predicate)
        {
            return _dc.GetTable<TEntity>().AsEnumerable();
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public System.Data.Linq.Table<TEntity> GetTable()
        {
            return _dc.GetTable<TEntity>();
        }
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
                foreach (System.Data.Linq.ObjectChangeConflict occ in _dc.ChangeConflicts)
                {
                    occ.Resolve(System.Data.Linq.RefreshMode.KeepCurrentValues);
                    occ.Resolve(System.Data.Linq.RefreshMode.OverwriteCurrentValues);
                    occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                }
                _dc.SubmitChanges();
            }
        }

        #region 封装Table方法开始
        public void Attach(TEntity entity)
        {
            GetTable().Attach(entity);
        }
        public void Attach(TEntity entity, bool asModified)
        {
            GetTable().Attach(entity, asModified);
        }
        public void Attach(TEntity entity, TEntity original)
        {
            GetTable().Attach(entity, original);
        }
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            GetTable().AttachAll(entities);
        }
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : TEntity
        {
            GetTable().AttachAll(entities, asModified);
        }
        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            GetTable().DeleteAllOnSubmit(entities);
        }
        public void DeleteOnSubmit(TEntity entity)
        {
            GetTable().DeleteOnSubmit(entity);
        }
        public IEnumerator<TEntity> GetEnumerator()
        {
            return GetTable().AsEnumerable().GetEnumerator();
        }
        public System.Data.Linq.ModifiedMemberInfo[] GetModifiedMembers(TEntity entity)
        {
            return GetTable().GetModifiedMembers(entity);
        }
        public System.ComponentModel.IBindingList GetNewBindingList()
        {
            return GetTable().GetNewBindingList();
        }
        public TEntity GetOriginalEntityState(TEntity entity)
        {
            return GetTable().GetOriginalEntityState(entity);
        }
        public void InsertAllOnSubmit(IEnumerable<TEntity> entities)
        {
            GetTable().InsertAllOnSubmit(entities);
        }
        public void InsertOnSubmit(TEntity entity)
        {
            GetTable().InsertOnSubmit(entity);
        }
        public override string ToString()
        {
            return "Linq2DT Ojbect!";
        }
        #endregion 封装Table方法结束
    }
}

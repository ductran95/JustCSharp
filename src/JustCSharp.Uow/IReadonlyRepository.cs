using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Data.Entities;

namespace JustCSharp.Uow
{
    public interface IReadonlyRepository<TEntity>
        where TEntity: class, IEntity
    {
        IQueryable<TEntity> GetQueryable();
        Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default);

        TEntity? Find(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true);

        Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default);

        List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true);

        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default);
    }
}
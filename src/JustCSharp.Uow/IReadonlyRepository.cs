using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Data;
using JustCSharp.Data.Entities;

namespace JustCSharp.Uow
{
    public interface IReadonlyRepository<TEntity> where TEntity : IEntity
    {
        IQueryable<TEntity> GetQueryable();
        Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default);

        TEntity Find([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true);

        Task<TEntity> FindAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default);

        List<TEntity> GetList([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true);

        Task<List<TEntity>> GetListAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default);
    }
}
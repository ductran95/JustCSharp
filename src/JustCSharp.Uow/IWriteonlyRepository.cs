using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Data;
using JustCSharp.Data.Entities;

namespace JustCSharp.Uow
{
    public interface IWriteonlyRepository<TEntity> where TEntity: IEntity
    {
        TEntity Insert([NotNull] TEntity entity, bool autoSave = false);
        Task<TEntity> InsertAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);
        
        void InsertMany([NotNull] IEnumerable<TEntity> entities, bool autoSave = false);
        Task InsertManyAsync([NotNull] IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);
        
        TEntity Update([NotNull] TEntity entity, bool autoSave = false);
        Task<TEntity> UpdateAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);
        
        void UpdateMany([NotNull] IEnumerable<TEntity> entities, bool autoSave = false);
        Task UpdateManyAsync([NotNull] IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);
        
        void Delete([NotNull] TEntity entity, bool autoSave = false);
        Task DeleteAsync([NotNull] TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);
        
        void DeleteMany([NotNull] IEnumerable<TEntity> entities, bool autoSave = false);
        Task DeleteManyAsync([NotNull] IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);
        
        void DeleteMany([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false);
        Task DeleteManyAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default);
    }
}
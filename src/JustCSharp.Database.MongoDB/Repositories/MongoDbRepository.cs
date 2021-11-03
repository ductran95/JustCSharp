using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Authentication;
using JustCSharp.Data.Entities;
using JustCSharp.Database.MongoDB.Context;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

// ReSharper disable SuspiciousTypeConversion.Global

namespace JustCSharp.Database.MongoDB.Repositories
{
    public abstract class MongoDbRepository<TDbContext, TEntity>: IMongoDbRepository<TEntity> 
        where TDbContext: class, IMongoDbContext 
        where TEntity : class, IEntity
    {
        protected readonly IAuthContextProvider _authContextProvider;
        protected readonly TDbContext _dbContext;

        public MongoDbRepository(IAuthContextProvider authContextProvider, TDbContext dbContext)
        {
            _authContextProvider = authContextProvider;
            _dbContext = dbContext;
        }

        #region Data

        protected IClientSessionHandle GetSessionHandle()
        {
            _dbContext.CheckStateAndConnect();
            return _dbContext.SessionHandle;
        }
        
        protected async Task<IClientSessionHandle> GetSessionHandleAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            return _dbContext.SessionHandle;
        }
        
        public IMongoDatabase GetDatabase()
        {
            _dbContext.CheckStateAndConnect();
            return _dbContext.Database;
        }

        public async Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            return _dbContext.Database;
        }

        public IMongoCollection<TEntity> GetCollection()
        {
            _dbContext.CheckStateAndConnect();
            return _dbContext.Collection<TEntity>();
        }

        public async Task<IMongoCollection<TEntity>> GetCollectionAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            return _dbContext.Collection<TEntity>();
        }

        public IMongoQueryable<TEntity> GetMongoQueryable()
        {
            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                _dbContext.SessionHandle != null
                    ? collection.AsQueryable(_dbContext.SessionHandle)
                    : collection.AsQueryable()
            );
        }

        public async Task<IMongoQueryable<TEntity>> GetMongoQueryableAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                _dbContext.SessionHandle != null
                    ? collection.AsQueryable(_dbContext.SessionHandle)
                    : collection.AsQueryable()
            );
        }

        public IAggregateFluent<TEntity> GetAggregate()
        {
            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                _dbContext.SessionHandle != null
                    ? collection.Aggregate(_dbContext.SessionHandle)
                    : collection.Aggregate());
        }

        public async Task<IAggregateFluent<TEntity>> GetAggregateAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                _dbContext.SessionHandle != null
                    ? collection.Aggregate(_dbContext.SessionHandle)
                    : collection.Aggregate());
        }
        
        public IQueryable<TEntity> GetQueryable()
        {
            return GetMongoQueryable();
        }

        public async Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default)
        {
            return await GetMongoQueryableAsync(cancellationToken);
        }

        public TEntity Find(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true)
        {
            var query = GetMongoQueryable();
            return query
                .Where(predicate)
                .SingleOrDefault();
        }

        public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var query = await GetMongoQueryableAsync(cancellationToken);
            return await query
                .Where(predicate)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true)
        {
            var query = GetMongoQueryable();
            return query
                .Where(predicate)
                .ToList();
        }

        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var query = await GetMongoQueryableAsync(cancellationToken);
            return await query
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        
        #endregion

        #region Action
        
        protected virtual TQueryable ApplyDataFilters<TQueryable>(TQueryable query)
            where TQueryable : IQueryable<TEntity>
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                query = (TQueryable)query.Where(e => ((ISoftDelete)e).IsDeleted == false);
            }

            return query;
        }
        
        protected virtual IAggregateFluent<TEntity> ApplyDataFilters(IAggregateFluent<TEntity> aggregate)
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                aggregate = aggregate.Match(e => ((ISoftDelete)e).IsDeleted == false);
            }

            return aggregate;
        }
        
        protected virtual void CheckAndSetId(TEntity entity)
        {
            if (!_dbContext.DbContextOptions.AutoGeneratedKey)
            {
                entity.CheckAndSetId();
            }
        }
        
        protected virtual void SetAuditProperties(TEntity entity)
        {
            if (entity is IAuditable auditableEntity)
            {
                var authContext = _authContextProvider.GetAuthContext();
                var userId = authContext.UserId;
                auditableEntity.CheckAndSetAudit(userId);
            }
        }
        
        protected virtual async Task SetAuditPropertiesAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is IAuditable auditableEntity)
            {
                var authContext = await _authContextProvider.GetAuthContextAsync(cancellationToken);
                var userId = authContext.UserId;
                auditableEntity.CheckAndSetAudit(userId);
            }
        }
        
        protected virtual void SetDeleteAuditProperties(TEntity entity)
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                var authContext = _authContextProvider.GetAuthContext();
                var userId = authContext.UserId;
                softDeleteEntity.CheckAndSetDeleteAudit(userId);
            }
        }
        
        protected virtual async Task SetDeleteAuditPropertiesAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                var authContext = await _authContextProvider.GetAuthContextAsync(cancellationToken);
                var userId = authContext.UserId;
                softDeleteEntity.CheckAndSetDeleteAudit(userId);
            }
        }
        
        protected virtual string SetConcurrencyStamp(TEntity entity)
        {
            if (entity is IHasConcurrencyStamp concurrencyEntity)
            {
                return concurrencyEntity.CheckAndSetConcurrencyStamp();
            }

            return null;
        }
        
        protected virtual void PreInsert(TEntity entity)
        {
            CheckAndSetId(entity);
            SetAuditProperties(entity);
        }
        
        protected virtual async Task PreInsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            CheckAndSetId(entity);
            await SetAuditPropertiesAsync(entity, cancellationToken);
        }
        
        protected virtual void PreUpdate(TEntity entity)
        {
            CheckAndSetId(entity);
            SetAuditProperties(entity);
        }
        
        protected virtual async Task PreUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            CheckAndSetId(entity);
            await SetAuditPropertiesAsync(entity, cancellationToken);
        }
        
        protected virtual void PreDelete(TEntity entity)
        {
            CheckAndSetId(entity);
            SetDeleteAuditProperties(entity);
        }
        
        protected virtual async Task PreDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            CheckAndSetId(entity);
            await SetDeleteAuditPropertiesAsync(entity, cancellationToken);
        }
        
        protected virtual bool IsHardDeleted(TEntity entity)
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                return softDeleteEntity.IsHardDeleted();
            }
            else
            {
                return true;
            }
        }

        protected abstract FilterDefinition<TEntity> CreateEntityFilter(TEntity entity, bool withConcurrencyStamp = false);

        protected abstract FilterDefinition<TEntity> CreateEntitiesFilter(IEnumerable<TEntity> entities, bool withConcurrencyStamp = false);

        protected virtual void ThrowOptimisticConcurrencyException()
        {
            ThrowOptimisticConcurrencyException(1, 0);
        }
        
        protected virtual void ThrowOptimisticConcurrencyException(long expectCount, long actualRow)
        {
            throw new MongoDbException($"Database operation expected to affect {expectCount} row but actually affected {actualRow} row. Data may have been modified or deleted since entities were loaded. This exception has been thrown on optimistic concurrency check.");
        }
        
        public TEntity Insert(TEntity entity, bool autoSave = false)
        {
            PreInsert(entity);

            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            if (_dbContext.SessionHandle != null)
            {
                 collection.InsertOne(
                    _dbContext.SessionHandle,
                    entity
                );
            }
            else
            {
                collection.InsertOne(
                    entity
                );
            }

            return entity;
        }

        public async Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            await PreInsertAsync(entity, cancellationToken);

            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            if (_dbContext.SessionHandle != null)
            {
                await collection.InsertOneAsync(
                    _dbContext.SessionHandle,
                    entity,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await collection.InsertOneAsync(
                    entity,
                    cancellationToken: cancellationToken
                );
            }

            return entity;
        }

        public void InsertMany(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            var entityArray = entities.ToArray();

            foreach (var entity in entityArray)
            {
                PreInsert(entity);
            }
            
            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            if (_dbContext.SessionHandle != null)
            {
                collection.InsertMany(_dbContext.SessionHandle, entityArray);
            }
            else
            {
                collection.InsertMany(entityArray);
            }
        }

        public async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entityArray = entities.ToArray();

            foreach (var entity in entityArray)
            {
                await PreInsertAsync(entity, cancellationToken);
            }
            
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();
            
            if (_dbContext.SessionHandle != null)
            {
                await collection.InsertManyAsync(_dbContext.SessionHandle, entityArray, cancellationToken: cancellationToken);
            }
            else
            {
                await collection.InsertManyAsync(entityArray, cancellationToken: cancellationToken);
            }
        }

        public TEntity Update(TEntity entity, bool autoSave = false)
        {
            PreUpdate(entity);
            
            if (entity is ISoftDelete softDeleteEntity && softDeleteEntity.IsDeleted)
            {
                PreDelete(entity);
            }
            
            ReplaceOneResult result;

            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            if (_dbContext.SessionHandle != null)
            {
                result = collection.ReplaceOne(
                    _dbContext.SessionHandle,
                    CreateEntityFilter(entity, true),
                    entity
                );
            }
            else
            {
                result = collection.ReplaceOne(
                    CreateEntityFilter(entity, true),
                    entity
                );
            }

            if (result.MatchedCount <= 0)
            {
                ThrowOptimisticConcurrencyException();
            }

            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            await PreUpdateAsync(entity, cancellationToken);
            
            if (entity is ISoftDelete softDeleteEntity && softDeleteEntity.IsDeleted)
            {
                await PreDeleteAsync(entity, cancellationToken);
            }
            
            ReplaceOneResult result;

            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            if (_dbContext.SessionHandle != null)
            {
                result = await collection.ReplaceOneAsync(
                    _dbContext.SessionHandle,
                    CreateEntityFilter(entity, true),
                    entity,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                result = await collection.ReplaceOneAsync(
                    CreateEntityFilter(entity, true),
                    entity,
                    cancellationToken: cancellationToken
                );
            }

            if (result.MatchedCount <= 0)
            {
                ThrowOptimisticConcurrencyException();
            }

            return entity;
        }
        
        public void UpdateMany(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            var entityArray = entities.ToArray();
            List<WriteModel<TEntity>> replaceRequests = new List<WriteModel<TEntity>>();

            foreach (var entity in entityArray)
            {
                PreUpdate(entity);

                var isSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
                if (isSoftDeleteEntity)
                {
                    PreDelete(entity);
                }

                replaceRequests.Add(new ReplaceOneModel<TEntity>(CreateEntityFilter(entity, true), entity));
            }

            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            BulkWriteResult result;
            
            if (_dbContext.SessionHandle != null)
            {
                result = collection.BulkWrite(_dbContext.SessionHandle, replaceRequests);
            }
            else
            {
                result = collection.BulkWrite(replaceRequests);
            }

            if (result.MatchedCount < entityArray.Length)
            {
                ThrowOptimisticConcurrencyException(entityArray.Length, result.MatchedCount);
            }
        }

        public async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entityArray = entities.ToArray();
            List<WriteModel<TEntity>> replaceRequests = new List<WriteModel<TEntity>>();

            foreach (var entity in entityArray)
            {
                await PreUpdateAsync(entity, cancellationToken);

                var isSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
                if (isSoftDeleteEntity)
                {
                    await PreDeleteAsync(entity, cancellationToken);
                }

                replaceRequests.Add(new ReplaceOneModel<TEntity>(CreateEntityFilter(entity, true), entity));
            }

            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            BulkWriteResult result;

            if (_dbContext.SessionHandle != null)
            {
                result = await collection.BulkWriteAsync(_dbContext.SessionHandle, replaceRequests, cancellationToken: cancellationToken);
            }
            else
            {
                result = await collection.BulkWriteAsync(replaceRequests, cancellationToken: cancellationToken);
            }

            if (result.MatchedCount < entityArray.Length)
            {
                ThrowOptimisticConcurrencyException(entityArray.Length, result.MatchedCount);
            }
        }

        public void Delete(TEntity entity, bool autoSave = false)
        {
            var isHardDelete = IsHardDeleted(entity);
            PreDelete(entity);
            
            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
            {
                ((ISoftDelete)entity).IsDeleted = true;

                ReplaceOneResult result;

                if (_dbContext.SessionHandle != null)
                {
                    result = collection.ReplaceOne(
                        _dbContext.SessionHandle,
                        CreateEntityFilter(entity, true),
                        entity
                    );
                }
                else
                {
                    result = collection.ReplaceOne(
                        CreateEntityFilter(entity, true),
                        entity
                    );
                }

                if (result.MatchedCount <= 0)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }
            else
            {
                DeleteResult result;

                if (_dbContext.SessionHandle != null)
                {
                    result = collection.DeleteOne(
                        _dbContext.SessionHandle,
                        CreateEntityFilter(entity, true)
                    );
                }
                else
                {
                    result = collection.DeleteOne(
                        CreateEntityFilter(entity, true)
                    );
                }

                if (result.DeletedCount <= 0)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }
        }

        public async Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var isHardDelete = IsHardDeleted(entity);
            await PreDeleteAsync(entity, cancellationToken);
            
            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
            {
                ((ISoftDelete)entity).IsDeleted = true;

                ReplaceOneResult result;

                if (_dbContext.SessionHandle != null)
                {
                    result = await collection.ReplaceOneAsync(
                        _dbContext.SessionHandle,
                        CreateEntityFilter(entity, true),
                        entity,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    result = await collection.ReplaceOneAsync(
                        CreateEntityFilter(entity, true),
                        entity,
                        cancellationToken: cancellationToken
                    );
                }

                if (result.MatchedCount <= 0)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }
            else
            {
                DeleteResult result;

                if (_dbContext.SessionHandle != null)
                {
                    result = await collection.DeleteOneAsync(
                        _dbContext.SessionHandle,
                        CreateEntityFilter(entity, true),
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    result = await collection.DeleteOneAsync(
                        CreateEntityFilter(entity, true),
                        cancellationToken
                    );
                }

                if (result.DeletedCount <= 0)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }
        }

        public void DeleteMany(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            var softDeletedEntities = new List<TEntity>();
            var hardDeletedEntities = new List<TEntity>();
            List<WriteModel<TEntity>> replaceRequests = new List<WriteModel<TEntity>>();

            foreach (var entity in entities)
            {
                var isHardDelete = IsHardDeleted(entity);
                PreDelete(entity);
                
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
                {
                    ((ISoftDelete)entity).IsDeleted = true;

                    softDeletedEntities.Add(entity);
                    replaceRequests.Add(new ReplaceOneModel<TEntity>(CreateEntityFilter(entity, true), entity));
                }
                else
                {
                    hardDeletedEntities.Add(entity);
                }
            }

            _dbContext.CheckStateAndConnect();
            var collection = _dbContext.Collection<TEntity>();

            if (softDeletedEntities.Count > 0)
            {
                BulkWriteResult updateResult;

                if (_dbContext.SessionHandle != null)
                {
                    updateResult = collection.BulkWrite(_dbContext.SessionHandle, replaceRequests);
                }
                else
                {
                    updateResult = collection.BulkWrite(replaceRequests);
                }

                if (updateResult.MatchedCount < softDeletedEntities.Count)
                {
                    ThrowOptimisticConcurrencyException(softDeletedEntities.Count, updateResult.MatchedCount);
                }
            }

            if (hardDeletedEntities.Count > 0)
            {
                DeleteResult deleteResult;

                if (_dbContext.SessionHandle != null)
                {
                    deleteResult = collection.DeleteMany(
                        _dbContext.SessionHandle,
                        CreateEntitiesFilter(hardDeletedEntities, true));
                }
                else
                {
                    deleteResult = collection.DeleteMany(
                        CreateEntitiesFilter(hardDeletedEntities, true));
                }

                if (deleteResult.DeletedCount < hardDeletedEntities.Count)
                {
                    ThrowOptimisticConcurrencyException(hardDeletedEntities.Count, deleteResult.DeletedCount);
                }
            }
        }

        public async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var softDeletedEntities = new List<TEntity>();
            var hardDeletedEntities = new List<TEntity>();
            List<WriteModel<TEntity>> replaceRequests = new List<WriteModel<TEntity>>();

            foreach (var entity in entities)
            {
                var isHardDelete = IsHardDeleted(entity);
                await PreDeleteAsync(entity, cancellationToken);
                
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
                {
                    ((ISoftDelete)entity).IsDeleted = true;

                    softDeletedEntities.Add(entity);
                    replaceRequests.Add(new ReplaceOneModel<TEntity>(CreateEntityFilter(entity, true), entity));
                }
                else
                {
                    hardDeletedEntities.Add(entity);
                }
            }

            await _dbContext.CheckStateAndConnectAsync(cancellationToken);
            var collection = _dbContext.Collection<TEntity>();

            if (softDeletedEntities.Count > 0)
            {
                BulkWriteResult updateResult;

                if (_dbContext.SessionHandle != null)
                {
                    updateResult = await collection.BulkWriteAsync(_dbContext.SessionHandle, replaceRequests, cancellationToken: cancellationToken);
                }
                else
                {
                    updateResult = await collection.BulkWriteAsync(replaceRequests, cancellationToken: cancellationToken);
                }

                if (updateResult.MatchedCount < softDeletedEntities.Count)
                {
                    ThrowOptimisticConcurrencyException(softDeletedEntities.Count, updateResult.MatchedCount);
                }
            }

            if (hardDeletedEntities.Count > 0)
            {
                DeleteResult deleteResult;

                if (_dbContext.SessionHandle != null)
                {
                    deleteResult = await collection.DeleteManyAsync(
                        _dbContext.SessionHandle,
                        CreateEntitiesFilter(hardDeletedEntities, true),
                        cancellationToken: cancellationToken);
                }
                else
                {
                    deleteResult = await collection.DeleteManyAsync(
                        CreateEntitiesFilter(hardDeletedEntities, true),
                        cancellationToken: cancellationToken);
                }

                if (deleteResult.DeletedCount < hardDeletedEntities.Count)
                {
                    ThrowOptimisticConcurrencyException(hardDeletedEntities.Count, deleteResult.DeletedCount);
                }
            }
        }

        public void DeleteMany(Expression<Func<TEntity, bool>> predicate, bool autoSave = false)
        {
            var entities = GetMongoQueryable()
                .Where(predicate)
                .ToList();

            DeleteMany(entities, autoSave);
        }

        public async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entities = await (await GetMongoQueryableAsync(cancellationToken))
                .Where(predicate)
                .ToListAsync(cancellationToken);

            await DeleteManyAsync(entities, autoSave, cancellationToken);
        }

        #endregion
    }

    public class MongoDbRepository<TDbContext, TEntity, TKey> : MongoDbRepository<TDbContext, TEntity>
        where TDbContext : class, IMongoDbContext
        where TEntity : class, IEntity<TKey>
    {
        public MongoDbRepository(IAuthContextProvider authContextProvider, TDbContext dbContext) : base(authContextProvider, dbContext)
        {
        }

        protected override FilterDefinition<TEntity> CreateEntityFilter(TEntity entity, bool withConcurrencyStamp = false)
        {
            var idFilter = Builders<TEntity>.Filter.Eq(x=> x.Id, entity.Id);
            
            if (!withConcurrencyStamp || !(entity is IHasConcurrencyStamp entityWithConcurrencyStamp))
            {
                return idFilter;
            }

            var oldConcurrencyStamp = entityWithConcurrencyStamp.CheckAndSetConcurrencyStamp();

            var concurrencyFilter =
                Builders<TEntity>.Filter.Eq(e => ((IHasConcurrencyStamp)e).ConcurrencyStamp, oldConcurrencyStamp);

            return Builders<TEntity>.Filter.And(idFilter, concurrencyFilter);
        }

        protected override FilterDefinition<TEntity> CreateEntitiesFilter(IEnumerable<TEntity> entities, bool withConcurrencyStamp = false)
        {
            if (!withConcurrencyStamp || !typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                var ids = entities.Select(x => x.Id);
                var idFilter = Builders<TEntity>.Filter.In(e => e.Id, ids);
                return idFilter;
            }

            List<FilterDefinition<TEntity>> entityFilters = new List<FilterDefinition<TEntity>>();

            foreach (var entity in entities)
            {
                var idFilter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
                
                if (!(entity is IHasConcurrencyStamp entityWithConcurrencyStamp))
                {
                    entityFilters.Add(idFilter);
                    continue;
                }
                
                var oldConcurrencyStamp = entityWithConcurrencyStamp.CheckAndSetConcurrencyStamp();

                var concurrencyFilter =
                    Builders<TEntity>.Filter.Eq(e => ((IHasConcurrencyStamp)e).ConcurrencyStamp, oldConcurrencyStamp);
                
                entityFilters.Add(Builders<TEntity>.Filter.And(idFilter, concurrencyFilter));
            }

            return Builders<TEntity>.Filter.Or(entityFilters);
        }
        
    }
}
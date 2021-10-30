using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Authentication;
using JustCSharp.Data;
using JustCSharp.Data.Entities;
using JustCSharp.MongoDB.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace JustCSharp.MongoDB.Repositories
{
    public class MongoDbRepository<TDbContext, TEntity>: IMongoDbRepository<TEntity> where TDbContext: IMongoDbContext where TEntity : class, IEntity
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IMongoDbContextProvider _dbContextProvider;

        public string CurrentUser => _serviceProvider.GetService<AuthContextBase>()?.UserId;
        
        public MongoDbRepository(IServiceProvider serviceProvider, IMongoDbContextProvider dbContextProvider)
        {
            _serviceProvider = serviceProvider;
            _dbContextProvider = dbContextProvider;
        }

        #region Data

        protected TDbContext GetDbContext()
        {
            return _dbContextProvider.GetDbContext<TDbContext>();
        }

        protected Task<TDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            return _dbContextProvider.GetDbContextAsync<TDbContext>(cancellationToken);
        }
        
        protected IClientSessionHandle GetSessionHandle()
        {
            return GetDbContext().SessionHandle;
        }
        
        protected async Task<IClientSessionHandle> GetSessionHandleAsync(CancellationToken cancellationToken = default)
        {
            return (await GetDbContextAsync(cancellationToken)).SessionHandle;
        }
        
        public IMongoDatabase GetDatabase()
        {
            return GetDbContext().Database;
        }

        public async Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default)
        {
            return (await GetDbContextAsync(cancellationToken)).Database;
        }

        public IMongoCollection<TEntity> GetCollection()
        {
            return GetDbContext().Collection<TEntity>();
        }

        public async Task<IMongoCollection<TEntity>> GetCollectionAsync(CancellationToken cancellationToken = default)
        {
            return (await GetDbContextAsync(cancellationToken)).Collection<TEntity>();
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
            entity.CheckAndSetId();
        }
        
        protected virtual void SetAuditProperties(TEntity entity)
        {
            if (entity is IAuditable auditableEntity)
            {
                auditableEntity.CheckAndSetAudit(CurrentUser);
            }
        }
        
        protected virtual void SetDeleteAuditProperties(TEntity entity)
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.CheckAndSetDeleteAudit(CurrentUser);
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
        
        protected virtual void PreUpdate(TEntity entity)
        {
            CheckAndSetId(entity);
            SetAuditProperties(entity);
        }
        
        protected virtual void PreDelete(TEntity entity)
        {
            CheckAndSetId(entity);
            SetDeleteAuditProperties(entity);
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
        
        protected virtual FilterDefinition<TEntity> CreateEntityFilter(TEntity entity, bool withConcurrencyStamp = false, string concurrencyStamp = null)
        {
            if (!withConcurrencyStamp || !(entity is IHasConcurrencyStamp entityWithConcurrencyStamp))
            {
                return Builders<TEntity>.Filter.Eq(e => e.GetKey(), entity.GetKey());
            }

            if (concurrencyStamp == null)
            {
                concurrencyStamp = entityWithConcurrencyStamp.ConcurrencyStamp;
            }

            return Builders<TEntity>.Filter.And(
                Builders<TEntity>.Filter.Eq(e => e.GetKey(), entity.GetKey()),
                Builders<TEntity>.Filter.Eq(e => ((IHasConcurrencyStamp)e).ConcurrencyStamp, concurrencyStamp)
            );
        }

        public FilterDefinition<TEntity> CreateEntitiesFilter(IEnumerable<object> ids, bool withConcurrencyStamp = false)
        {
            var filters = new List<FilterDefinition<TEntity>>()
            {
                Builders<TEntity>.Filter.In(e => e.GetKey(), ids),
            };

            if (withConcurrencyStamp)
            {
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                {
                    filters.Add(Builders<TEntity>.Filter.Eq(e => ((ISoftDelete)e).IsDeleted, false));
                }
            }

            return Builders<TEntity>.Filter.And(filters);
        }
        
        protected virtual FilterDefinition<TEntity> CreateEntitiesFilter(IEnumerable<TEntity> entities, bool withConcurrencyStamp = false)
        {
            return CreateEntitiesFilter(entities.Select(s => s.GetKey()), withConcurrencyStamp);
        }
        
        protected virtual void ThrowOptimisticConcurrencyException()
        {
            throw new MongoDbException("Database operation expected to affect 1 row but actually affected 0 row. Data may have been modified or deleted since entities were loaded. This exception has been thrown on optimistic concurrency check.");
        }
        
        public IMongoQueryable<TEntity> GetMongoQueryable()
        {
            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                dbContext.SessionHandle != null
                    ? collection.AsQueryable(dbContext.SessionHandle)
                    : collection.AsQueryable()
            );
        }

        public async Task<IMongoQueryable<TEntity>> GetMongoQueryableAsync(CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                dbContext.SessionHandle != null
                    ? collection.AsQueryable(dbContext.SessionHandle)
                    : collection.AsQueryable()
            );
        }

        public IAggregateFluent<TEntity> GetAggregate()
        {
            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                dbContext.SessionHandle != null
                    ? collection.Aggregate(dbContext.SessionHandle)
                    : collection.Aggregate());
        }

        public async Task<IAggregateFluent<TEntity>> GetAggregateAsync(CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            return ApplyDataFilters(
                dbContext.SessionHandle != null
                    ? collection.Aggregate(dbContext.SessionHandle)
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

        public TEntity Insert(TEntity entity, bool autoSave = false)
        {
            PreInsert(entity);

            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            if (dbContext.SessionHandle != null)
            {
                 collection.InsertOne(
                    dbContext.SessionHandle,
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
            PreInsert(entity);

            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            if (dbContext.SessionHandle != null)
            {
                await collection.InsertOneAsync(
                    dbContext.SessionHandle,
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
            
            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();
            
            BulkWriteResult result;

            List<WriteModel<TEntity>> insertRequests = new List<WriteModel<TEntity>>();
            foreach (var entity in entityArray)
            {
                insertRequests.Add(new InsertOneModel<TEntity>(entity));
            }

            if (dbContext.SessionHandle != null)
            {
                result = collection.BulkWrite(dbContext.SessionHandle, insertRequests);
            }
            else
            {
                result = collection.BulkWrite(insertRequests);
            }

            if (result.MatchedCount < entityArray.Length)
            {
                ThrowOptimisticConcurrencyException();
            }
        }

        public async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entityArray = entities.ToArray();

            foreach (var entity in entityArray)
            {
                PreInsert(entity);
            }
            
            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();
            
            BulkWriteResult result;

            List<WriteModel<TEntity>> insertRequests = new List<WriteModel<TEntity>>();
            foreach (var entity in entityArray)
            {
                insertRequests.Add(new InsertOneModel<TEntity>(entity));
            }

            if (dbContext.SessionHandle != null)
            {
                result = await collection.BulkWriteAsync(dbContext.SessionHandle, insertRequests, cancellationToken: cancellationToken);
            }
            else
            {
                result = await collection.BulkWriteAsync(insertRequests, cancellationToken: cancellationToken);
            }

            if (result.MatchedCount < entityArray.Length)
            {
                ThrowOptimisticConcurrencyException();
            }
        }

        public TEntity Update(TEntity entity, bool autoSave = false)
        {
            PreUpdate(entity);
            
            if (entity is ISoftDelete softDeleteEntity && softDeleteEntity.IsDeleted)
            {
                PreDelete(entity);
            }
            
            var oldConcurrencyStamp = SetConcurrencyStamp(entity);
            ReplaceOneResult result;

            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            if (dbContext.SessionHandle != null)
            {
                result = collection.ReplaceOne(
                    dbContext.SessionHandle,
                    CreateEntityFilter(entity, true, oldConcurrencyStamp),
                    entity
                );
            }
            else
            {
                result = collection.ReplaceOne(
                    CreateEntityFilter(entity, true, oldConcurrencyStamp),
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
            PreUpdate(entity);
            
            if (entity is ISoftDelete softDeleteEntity && softDeleteEntity.IsDeleted)
            {
                PreDelete(entity);
            }
            
            var oldConcurrencyStamp = SetConcurrencyStamp(entity);
            ReplaceOneResult result;

            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            if (dbContext.SessionHandle != null)
            {
                result = await collection.ReplaceOneAsync(
                    dbContext.SessionHandle,
                    CreateEntityFilter(entity, true, oldConcurrencyStamp),
                    entity,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                result = await collection.ReplaceOneAsync(
                    CreateEntityFilter(entity, true, oldConcurrencyStamp),
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

            foreach (var entity in entityArray)
            {
                PreUpdate(entity);

                var isSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
                if (isSoftDeleteEntity)
                {
                    PreDelete(entity);
                }

                SetConcurrencyStamp(entity);
            }

            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            BulkWriteResult result;

            List<WriteModel<TEntity>> replaceRequests = new List<WriteModel<TEntity>>();
            foreach (var entity in entityArray)
            {
                replaceRequests.Add(new ReplaceOneModel<TEntity>(CreateEntityFilter(entity), entity));
            }
            
            if (dbContext.SessionHandle != null)
            {
                result = collection.BulkWrite(dbContext.SessionHandle, replaceRequests);
            }
            else
            {
                result = collection.BulkWrite(replaceRequests);
            }

            if (result.MatchedCount < entityArray.Length)
            {
                ThrowOptimisticConcurrencyException();
            }
        }

        public async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var entityArray = entities.ToArray();

            foreach (var entity in entityArray)
            {
                PreUpdate(entity);

                var isSoftDeleteEntity = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
                if (isSoftDeleteEntity)
                {
                    PreDelete(entity);
                }

                SetConcurrencyStamp(entity);
            }

            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            BulkWriteResult result;

            List<WriteModel<TEntity>> replaceRequests = new List<WriteModel<TEntity>>();
            foreach (var entity in entityArray)
            {
                replaceRequests.Add(new ReplaceOneModel<TEntity>(CreateEntityFilter(entity), entity));
            }
            
            if (dbContext.SessionHandle != null)
            {
                result = await collection.BulkWriteAsync(dbContext.SessionHandle, replaceRequests, cancellationToken: cancellationToken);
            }
            else
            {
                result = await collection.BulkWriteAsync(replaceRequests, cancellationToken: cancellationToken);
            }

            if (result.MatchedCount < entityArray.Length)
            {
                ThrowOptimisticConcurrencyException();
            }
        }

        public void Delete(TEntity entity, bool autoSave = false)
        {
            var isHardDelete = IsHardDeleted(entity);
            PreDelete(entity);
            
            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            var oldConcurrencyStamp = SetConcurrencyStamp(entity);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
            {
                ((ISoftDelete)entity).IsDeleted = true;

                ReplaceOneResult result;

                if (dbContext.SessionHandle != null)
                {
                    result = collection.ReplaceOne(
                        dbContext.SessionHandle,
                        CreateEntityFilter(entity, true, oldConcurrencyStamp),
                        entity
                    );
                }
                else
                {
                    result = collection.ReplaceOne(
                        CreateEntityFilter(entity, true, oldConcurrencyStamp),
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

                if (dbContext.SessionHandle != null)
                {
                    result = collection.DeleteOne(
                        dbContext.SessionHandle,
                        CreateEntityFilter(entity, true, oldConcurrencyStamp)
                    );
                }
                else
                {
                    result = collection.DeleteOne(
                        CreateEntityFilter(entity, true, oldConcurrencyStamp)
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
            PreDelete(entity);
            
            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            var oldConcurrencyStamp = SetConcurrencyStamp(entity);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
            {
                ((ISoftDelete)entity).IsDeleted = true;

                ReplaceOneResult result;

                if (dbContext.SessionHandle != null)
                {
                    result = await collection.ReplaceOneAsync(
                        dbContext.SessionHandle,
                        CreateEntityFilter(entity, true, oldConcurrencyStamp),
                        entity,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    result = await collection.ReplaceOneAsync(
                        CreateEntityFilter(entity, true, oldConcurrencyStamp),
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

                if (dbContext.SessionHandle != null)
                {
                    result = await collection.DeleteOneAsync(
                        dbContext.SessionHandle,
                        CreateEntityFilter(entity, true, oldConcurrencyStamp),
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    result = await collection.DeleteOneAsync(
                        CreateEntityFilter(entity, true, oldConcurrencyStamp),
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
            var softDeletedEntities = new Dictionary<TEntity, string>();
            var hardDeletedEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                var isHardDelete = IsHardDeleted(entity);
                PreDelete(entity);
                
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
                {
                    ((ISoftDelete)entity).IsDeleted = true;

                    softDeletedEntities.Add(entity, SetConcurrencyStamp(entity));
                }
                else
                {
                    hardDeletedEntities.Add(entity);
                }
            }

            var dbContext = GetDbContext();
            var collection = dbContext.Collection<TEntity>();

            if (softDeletedEntities.Count > 0)
            {
                BulkWriteResult updateResult;

                var replaceRequests = new List<WriteModel<TEntity>>(
                    softDeletedEntities.Select(entity => new ReplaceOneModel<TEntity>(
                        CreateEntityFilter(entity.Key, true, entity.Value), entity.Key))
                );

                if (dbContext.SessionHandle != null)
                {
                    updateResult = collection.BulkWrite(dbContext.SessionHandle, replaceRequests);
                }
                else
                {
                    updateResult = collection.BulkWrite(replaceRequests);
                }

                if (updateResult.MatchedCount < softDeletedEntities.Count)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }

            if (hardDeletedEntities.Count > 0)
            {
                DeleteResult deleteResult;
                var hardDeletedEntitiesCount = hardDeletedEntities.Count;

                if (dbContext.SessionHandle != null)
                {
                    deleteResult = collection.DeleteMany(
                        dbContext.SessionHandle,
                        CreateEntitiesFilter(hardDeletedEntities));
                }
                else
                {
                    deleteResult = collection.DeleteMany(
                        CreateEntitiesFilter(hardDeletedEntities));
                }

                if (deleteResult.DeletedCount < hardDeletedEntitiesCount)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }
        }

        public async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            var softDeletedEntities = new Dictionary<TEntity, string>();
            var hardDeletedEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                var isHardDelete = IsHardDeleted(entity);
                PreDelete(entity);
                
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !isHardDelete)
                {
                    ((ISoftDelete)entity).IsDeleted = true;

                    softDeletedEntities.Add(entity, SetConcurrencyStamp(entity));
                }
                else
                {
                    hardDeletedEntities.Add(entity);
                }
            }

            var dbContext = await GetDbContextAsync(cancellationToken);
            var collection = dbContext.Collection<TEntity>();

            if (softDeletedEntities.Count > 0)
            {
                BulkWriteResult updateResult;

                var replaceRequests = new List<WriteModel<TEntity>>(
                    softDeletedEntities.Select(entity => new ReplaceOneModel<TEntity>(
                        CreateEntityFilter(entity.Key, true, entity.Value), entity.Key))
                );

                if (dbContext.SessionHandle != null)
                {
                    updateResult = await collection.BulkWriteAsync(dbContext.SessionHandle, replaceRequests, cancellationToken: cancellationToken);
                }
                else
                {
                    updateResult = await collection.BulkWriteAsync(replaceRequests, cancellationToken: cancellationToken);
                }

                if (updateResult.MatchedCount < softDeletedEntities.Count)
                {
                    ThrowOptimisticConcurrencyException();
                }
            }

            if (hardDeletedEntities.Count > 0)
            {
                DeleteResult deleteResult;
                var hardDeletedEntitiesCount = hardDeletedEntities.Count;

                if (dbContext.SessionHandle != null)
                {
                    deleteResult = await collection.DeleteManyAsync(
                        dbContext.SessionHandle,
                        CreateEntitiesFilter(hardDeletedEntities),
                        cancellationToken: cancellationToken);
                }
                else
                {
                    deleteResult = await collection.DeleteManyAsync(
                        CreateEntitiesFilter(hardDeletedEntities),
                        cancellationToken: cancellationToken);
                }

                if (deleteResult.DeletedCount < hardDeletedEntitiesCount)
                {
                    ThrowOptimisticConcurrencyException();
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
}
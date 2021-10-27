using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.Utility.Extensions;

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly Dictionary<Type, IDatabase> _databases;

        public UnitOfWork()
        {
            _databases = new Dictionary<Type, IDatabase>();
        }

        public bool IsTransactional { get; private set; }

        public IDatabase FindDatabase([NotNull] Type type)
        {
            return _databases.GetOrDefault(type);
        }

        public void AddDatabase([NotNull] Type type, [NotNull] IDatabase database)
        {
            if (_databases.ContainsKey(type))
            {
                throw new InfrastructureException("There is already a database API in this unit of work with given type: " + type.Name);
            }

            _databases.Add(type, database);
        }

        public IDatabase GetOrAddDatabase([NotNull] Type type, Func<IDatabase> factory)
        {
            var db = FindDatabase(type);
            if (db == null)
            {
                db = factory();
                AddDatabase(type, db);
            }

            return db;
        }

        public bool IsInTransaction()
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        var inTransaction = (databaseApi as ISupportTransaction).IsInTransaction();
                        if (!inTransaction)
                        {
                            return false;
                        }
                    }
                    catch { }
                }
            }

            return true;
        }

        public async Task<bool> IsInTransactionAsync(CancellationToken cancellationToken = default)
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        var inTransaction = await (databaseApi as ISupportTransaction).IsInTransactionAsync(cancellationToken);
                        if (!inTransaction)
                        {
                            return false;
                        }
                    }
                    catch { }
                }
            }

            return true;
        }

        public void Begin()
        {
            this.IsTransactional = true;
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        (databaseApi as ISupportTransaction).Begin();
                    }
                    catch { }
                }
            }
        }

        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            this.IsTransactional = true;
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        await (databaseApi as ISupportTransaction).BeginAsync(cancellationToken);
                    }
                    catch { }
                }
            }
        }

        public void Commit()
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        (databaseApi as ISupportTransaction).Commit();
                    }
                    catch { }
                }
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        await (databaseApi as ISupportTransaction).CommitAsync(cancellationToken);
                    }
                    catch { }
                }
            }
        }

        public void Rollback()
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        (databaseApi as ISupportTransaction).Rollback();
                    }
                    catch { }
                }
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportTransaction)
                {
                    try
                    {
                        await (databaseApi as ISupportTransaction).RollbackAsync(cancellationToken);
                    }
                    catch { }
                }
            }
        }

        public void SaveChanges()
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportSaveChange)
                {
                    try
                    {
                        (databaseApi as ISupportSaveChange).SaveChanges();
                    }
                    catch { }
                }
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var databaseApi in GetAllActiveDatabases())
            {
                if (databaseApi is ISupportSaveChange)
                {
                    try
                    {
                        await (databaseApi as ISupportSaveChange).SaveChangesAsync(cancellationToken);
                    }
                    catch { }
                }
            }
        }
        
        private IReadOnlyList<IDatabase> GetAllActiveDatabases()
        {
            return _databases.Values.ToList();
        }
    }
}
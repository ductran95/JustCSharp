using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using JustCSharp.Utility.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable SuspiciousTypeConversion.Global

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkBase: IUnitOfWork
    {
        protected readonly ILazyServiceProvider _serviceProvider;
        protected readonly Dictionary<Type, IDatabase> _databases;
        protected readonly Dictionary<Type, ITransaction> _transactions;

        protected ILogger Logger => _serviceProvider.GetLogger(GetType());
        
        public UnitOfWorkBase(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _databases = new Dictionary<Type, IDatabase>();
            _transactions = new Dictionary<Type, ITransaction>();
        }

        public virtual bool IsTransactional { get; private set; }

        public virtual IDatabase FindDatabase([NotNull] Type type)
        {
            return _databases.GetOrDefault(type);
        }

        public virtual void AddDatabase([NotNull] Type type, [NotNull] IDatabase database)
        {
            if (_databases.ContainsKey(type))
            {
                throw new InfrastructureException("There is already a database API in this unit of work with given type: " + type.Name);
            }

            _databases.Add(type, database);
        }

        public virtual IDatabase GetOrAddDatabase([NotNull] Type type, Func<IDatabase> factory)
        {
            var db = FindDatabase(type);
            if (db == null)
            {
                db = factory();
                AddDatabase(type, db);
            }

            return db;
        }
        
        public virtual ITransaction FindTransaction([NotNull] Type type)
        {
            return _transactions.GetOrDefault(type);
        }

        public virtual void AddTransaction([NotNull] Type type, [NotNull] ITransaction database)
        {
            if (_databases.ContainsKey(type))
            {
                throw new InfrastructureException("There is already a transaction API in this unit of work with given type: " + type.Name);
            }

            _transactions.Add(type, database);
        }

        public virtual ITransaction GetOrAddTransaction([NotNull] Type type, Func<ITransaction> factory)
        {
            var db = FindTransaction(type);
            if (db == null)
            {
                db = factory();
                AddTransaction(type, db);
            }

            return db;
        }

        public virtual bool IsInTransaction()
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

        public virtual async Task<bool> IsInTransactionAsync(CancellationToken cancellationToken = default)
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

        public virtual void Begin()
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

        public virtual async Task BeginAsync(CancellationToken cancellationToken = default)
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

        public virtual void Commit()
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

        public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
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

        public virtual void Rollback()
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

        public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
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

        public virtual void SaveChanges()
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

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
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
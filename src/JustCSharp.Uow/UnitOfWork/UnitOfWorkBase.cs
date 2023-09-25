using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Utility.Extensions;

// ReSharper disable SuspiciousTypeConversion.Global

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkBase: IUnitOfWork
    {
        protected readonly ILazyServiceProvider _serviceProvider;
        protected readonly Dictionary<Type, IDatabase> _databases;
        protected UnitOfWorkTransaction? _currentTransaction;

        public ITransaction? CurrentTransaction => _currentTransaction;

        // private ILogger Logger => _serviceProvider.GetLogger(typeof(UnitOfWorkBase));
        
        public UnitOfWorkBase(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _databases = new Dictionary<Type, IDatabase>();
        }

        public virtual bool IsTransactional { get; private set; }

        public virtual IDatabase? FindDatabase(Type type)
        {
            return _databases.GetOrDefault(type);
        }

        public virtual void AddDatabase(Type type, IDatabase database)
        {
            if (_databases.ContainsKey(type))
            {
                throw new InfrastructureException("There is already a database API in this unit of work with given type: " + type.Name);
            }

            if (database is ISupportTransaction databaseSupportTransaction && databaseSupportTransaction.CurrentTransaction != null)
            {
                if (_currentTransaction == null)
                {
                    _currentTransaction = new UnitOfWorkTransaction();
                }
                
                _currentTransaction.ChildrenTransactions.Add(databaseSupportTransaction.CurrentTransaction);
            }

            _databases.Add(type, database);
        }

        public virtual IDatabase GetOrAddDatabase(Type type, Func<IDatabase> factory)
        {
            var db = FindDatabase(type);
            if (db == null)
            {
                db = factory();
                AddDatabase(type, db);
            }

            return db;
        }
        
        public virtual bool IsInTransaction()
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    var inTransaction = databaseSupportTransaction.IsInTransaction();
                    if (!inTransaction)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual async Task<bool> IsInTransactionAsync(CancellationToken cancellationToken = default)
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    var inTransaction = await databaseSupportTransaction.IsInTransactionAsync(cancellationToken);
                    if (!inTransaction)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual void BeginTransaction()
        {
            IsTransactional = true;
            
            if (_currentTransaction == null)
            {
                _currentTransaction = new UnitOfWorkTransaction();
            }
            
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    var inTransaction = databaseSupportTransaction.IsInTransaction();
                    if (!inTransaction)
                    {
                        databaseSupportTransaction.BeginTransaction();
                        _currentTransaction.ChildrenTransactions.Add(databaseSupportTransaction.CurrentTransaction!);
                    }
                }
            }
        }

        public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            IsTransactional = true;
            
            if (_currentTransaction == null)
            {
                _currentTransaction = new UnitOfWorkTransaction();
            }
            
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    var inTransaction = await databaseSupportTransaction.IsInTransactionAsync(cancellationToken);
                    if (!inTransaction)
                    {
                        await databaseSupportTransaction.BeginTransactionAsync(cancellationToken);
                        _currentTransaction.ChildrenTransactions.Add(databaseSupportTransaction.CurrentTransaction!);
                    }
                }
            }
        }

        public virtual void CommitTransaction()
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    databaseSupportTransaction.CommitTransaction();
                }
            }
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    await databaseSupportTransaction.CommitTransactionAsync(cancellationToken);
                }
            }
        }

        public virtual void RollbackTransaction()
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    databaseSupportTransaction.RollbackTransaction();
                }
            }
        }

        public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportTransaction databaseSupportTransaction)
                {
                    await databaseSupportTransaction.RollbackTransactionAsync(cancellationToken);
                }
            }
        }

        public virtual void SaveChanges()
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportSaveChange databaseSupportSaveChange)
                {
                    databaseSupportSaveChange.SaveChanges();
                }
            }
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var database in GetAllActiveDatabases())
            {
                if (database is ISupportSaveChange databaseSupportSaveChange)
                {
                    await databaseSupportSaveChange.SaveChangesAsync(cancellationToken);
                }
            }
        }
        
        private IReadOnlyList<IDatabase> GetAllActiveDatabases()
        {
            return _databases.Values.ToList();
        }
    }
}
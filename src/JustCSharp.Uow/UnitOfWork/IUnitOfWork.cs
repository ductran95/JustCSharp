using System;
using System.Diagnostics.CodeAnalysis;

namespace JustCSharp.Uow.UnitOfWork
{
    public interface IUnitOfWork: ISupportTransaction, ISupportSaveChange
    {
        bool IsTransactional { get; }
        IDatabase? FindDatabase(Type type);
        void AddDatabase(Type type, IDatabase database);
        IDatabase GetOrAddDatabase(Type type, Func<IDatabase> factory);
    }
}
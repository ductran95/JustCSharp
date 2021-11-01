using System;
using System.Diagnostics.CodeAnalysis;

namespace JustCSharp.Uow.UnitOfWork
{
    public interface IUnitOfWork: ISupportTransaction, ISupportSaveChange
    {
        bool IsTransactional { get; }
        IDatabase FindDatabase([NotNull] Type type);
        void AddDatabase([NotNull] Type type, [NotNull] IDatabase database);
        IDatabase GetOrAddDatabase([NotNull] Type type, Func<IDatabase> factory);
        ITransaction FindTransaction([NotNull] Type type);
        void AddTransaction([NotNull] Type type, [NotNull] ITransaction database);
        ITransaction GetOrAddTransaction([NotNull] Type type, Func<ITransaction> factory);
    }
}
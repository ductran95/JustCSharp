using System.Collections.Generic;

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkTransaction: ITransaction
    {
        public List<ITransaction> ChildrenTransactions { get; set; }

        public UnitOfWorkTransaction()
        {
            ChildrenTransactions = new List<ITransaction>();
        }
    }
}


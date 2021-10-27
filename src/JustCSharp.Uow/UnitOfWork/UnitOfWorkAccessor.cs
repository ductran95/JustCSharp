namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkAccessor: IUnitOfWorkAccessor
    {
        public IUnitOfWork UnitOfWork { get; set; }
    }
}
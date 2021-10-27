namespace JustCSharp.Uow.UnitOfWork
{
    public interface IUnitOfWorkAccessor
    {
        IUnitOfWork UnitOfWork { get; set; }
    }
}
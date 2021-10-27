using JustCSharp.Data;

namespace JustCSharp.Uow
{
    public interface IRepository<TEntity>:IReadonlyRepository<TEntity>, IWriteonlyRepository<TEntity> where TEntity: IEntity
    {
        
    }
}
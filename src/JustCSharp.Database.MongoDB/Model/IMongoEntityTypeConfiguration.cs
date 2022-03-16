namespace JustCSharp.Database.MongoDB.Model
{
    public interface IMongoEntityTypeConfiguration<TEntity> where TEntity: class
    {
        void Configure(IMongoEntityModel<TEntity> builder);
    }
}
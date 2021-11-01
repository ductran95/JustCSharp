using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JustCSharp.Utility.Extensions;

namespace JustCSharp.MongoDB.Model
{
    public class MongoModelBuilder
    {
        private readonly Dictionary<Type, object> _entityModels;

        public MongoModelBuilder()
        {
            _entityModels = new Dictionary<Type, object>();
        }

        public void Entity<TEntity>(Action<IMongoEntityModel<TEntity>> buildAction = null)
        {
            var model = (IMongoEntityModel<TEntity>) _entityModels.GetOrAdd(typeof(TEntity), () => new MongoEntityModel<TEntity>());

            buildAction?.Invoke(model);
        }

        public void Entity([NotNull] Type entityType, Action<IMongoEntityModel> buildAction = null)
        {
            var model = (IMongoEntityModel) _entityModels.GetOrAdd(entityType, () => Activator.CreateInstance(
                typeof(MongoEntityModel<>).MakeGenericType(entityType)));

            buildAction?.Invoke(model);
        }

        public IReadOnlyList<IMongoEntityModel> GetEntities()
        {
            return _entityModels.Values.Cast<IMongoEntityModel>().ToList();
        }
    }
}
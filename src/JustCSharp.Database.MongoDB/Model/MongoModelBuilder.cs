using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JustCSharp.Utility.Extensions;

namespace JustCSharp.Database.MongoDB.Model
{
    public class MongoModelBuilder
    {
        private readonly Dictionary<Type, object> _entityModels;

        public MongoModelBuilder()
        {
            _entityModels = new Dictionary<Type, object>();
        }

        public virtual void Entity<TEntity>(Action<IMongoEntityModel<TEntity>> buildAction = null)
        {
            var model = (IMongoEntityModel<TEntity>) _entityModels.GetOrAdd(typeof(TEntity), () => new MongoEntityModel<TEntity>());

            buildAction?.Invoke(model);
        }

        public virtual void Entity([NotNull] Type entityType, Action<IMongoEntityModel> buildAction = null)
        {
            var model = (IMongoEntityModel) _entityModels.GetOrAdd(entityType, () => Activator.CreateInstance(
                typeof(MongoEntityModel<>).MakeGenericType(entityType)));

            buildAction?.Invoke(model);
        }
        
        public virtual MongoModelBuilder ApplyConfiguration<TEntity>(IMongoEntityTypeConfiguration<TEntity> configuration)
            where TEntity : class
        {
            var model = (IMongoEntityModel<TEntity>) _entityModels.GetOrAdd(typeof(TEntity), () => new MongoEntityModel<TEntity>());
            configuration.Configure(model);

            return this;
        }
        
        public virtual MongoModelBuilder ApplyConfigurationsFromAssembly(
            Assembly assembly,
            Func<Type, bool> predicate = null)
        {
            var applyEntityConfigurationMethod = typeof(MongoModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ApplyConfiguration)
                         && e.ContainsGenericParameters
                         && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                         == typeof(IMongoEntityTypeConfiguration<>));

            foreach (var type in assembly.GetConstructibleTypes().OrderBy(t => t.FullName))
            {
                // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
                if (type.GetConstructor(Type.EmptyTypes) == null
                    || (!predicate?.Invoke(type) ?? false))
                {
                    continue;
                }

                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == typeof(IMongoEntityTypeConfiguration<>))
                    {
                        var target = applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(this, new[] { Activator.CreateInstance(type) });
                    }
                }
            }
            
            return this;
        }

        public virtual IReadOnlyList<IMongoEntityModel> GetEntities()
        {
            return _entityModels.Values.Cast<IMongoEntityModel>().ToList();
        }
    }
}
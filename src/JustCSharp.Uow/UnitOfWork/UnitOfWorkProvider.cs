using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkProvider: IUnitOfWorkProvider
    {
        private readonly IServiceProvider _serviceProvider;
        
        public UnitOfWorkProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public IUnitOfWork GetUnitOfWork()
        {
            var uowAccessor = _serviceProvider.GetRequiredService<IUnitOfWorkAccessor>();
            if (uowAccessor.UnitOfWork == null)
            {
                uowAccessor.UnitOfWork = CreateUnitOfWork();
            }

            return uowAccessor.UnitOfWork;
        }

        public Task<IUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetUnitOfWork());
        }
        
        protected IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork();
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkProvider: IUnitOfWorkProvider
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected IUnitOfWork _unitOfWork;
        
        protected ILogger Logger => _serviceProvider.GetLogger(GetType());

        public UnitOfWorkProvider(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUnitOfWork UnitOfWork => GetUnitOfWork();

        public virtual IUnitOfWork GetUnitOfWork()
        {
            if (_unitOfWork == null)
            {
                _unitOfWork = CreateUnitOfWork();
            }

            return _unitOfWork;
        }

        public virtual async Task<IUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken = default)
        {
            if (_unitOfWork == null)
            {
                _unitOfWork = await CreateUnitOfWorkAsync(cancellationToken);
            }

            return _unitOfWork;
        }
        
        protected virtual IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWorkBase(_serviceProvider);
        }
        
        protected virtual async Task<IUnitOfWork> CreateUnitOfWorkAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new UnitOfWorkBase(_serviceProvider));
        }
    }

    public abstract class UnitOfWorkProviderOfT<TUnitOfWork> : IUnitOfWorkProviderOfT<TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected TUnitOfWork _unitOfWork;
        
        protected ILogger Logger => _serviceProvider.GetLogger(GetType());

        protected UnitOfWorkProviderOfT(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUnitOfWork UnitOfWork => GetUnitOfWork();
        public TUnitOfWork UnitOfWorkOfT => GetUnitOfWorkOfT();
        
        public virtual IUnitOfWork GetUnitOfWork()
        {
            return GetUnitOfWorkOfT();
        }

        public virtual async Task<IUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken = default)
        {
            return await GetUnitOfWorkOfTAsync(cancellationToken);
        }
        
        public virtual TUnitOfWork GetUnitOfWorkOfT()
        {
            if (_unitOfWork == null)
            {
                _unitOfWork = CreateUnitOfWork();
            }

            return _unitOfWork;
        }

        public virtual async Task<TUnitOfWork> GetUnitOfWorkOfTAsync(CancellationToken cancellationToken = default)
        {
            if (_unitOfWork == null)
            {
                _unitOfWork = await CreateUnitOfWorkAsync(cancellationToken);
            }

            return _unitOfWork;
        }

        protected abstract TUnitOfWork CreateUnitOfWork();
        protected abstract Task<TUnitOfWork> CreateUnitOfWorkAsync(CancellationToken cancellationToken = default);
    }
}
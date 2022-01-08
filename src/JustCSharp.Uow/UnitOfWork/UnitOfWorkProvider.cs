using System.Threading;
using System.Threading.Tasks;
using JustCSharp.Core.DependencyInjection;
using JustCSharp.Core.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkProvider: IUnitOfWorkProvider
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected IUnitOfWork _unitOfWork;
        
        private ILogger Logger => _serviceProvider.GetLogger(typeof(UnitOfWorkProvider));

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

    public abstract class UnitOfWorkProvider<TUnitOfWork> : IUnitOfWorkProvider<TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork
    {
        protected readonly ILazyServiceProvider _serviceProvider;

        protected TUnitOfWork _unitOfWork;
        
        private ILogger Logger => _serviceProvider.GetLogger(typeof(UnitOfWorkProvider<>));

        protected UnitOfWorkProvider(ILazyServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TUnitOfWork UnitOfWork => GetUnitOfWork();

        IUnitOfWork IUnitOfWorkProvider.UnitOfWork => UnitOfWork;

        public TUnitOfWork GetUnitOfWork()
        {
            if (_unitOfWork == null)
            {
                _unitOfWork = CreateUnitOfWork();
            }

            return _unitOfWork;
        }

        public async Task<TUnitOfWork> GetUnitOfWorkAsync(CancellationToken cancellationToken)
        {
            if (_unitOfWork == null)
            {
                _unitOfWork = await CreateUnitOfWorkAsync(cancellationToken);
            }

            return _unitOfWork;
        }

        IUnitOfWork IUnitOfWorkProvider.GetUnitOfWork()
        {
            return GetUnitOfWork();
        }

        async Task<IUnitOfWork> IUnitOfWorkProvider.GetUnitOfWorkAsync(CancellationToken cancellationToken)
        {
            return await GetUnitOfWorkAsync(cancellationToken);
        }

        protected abstract TUnitOfWork CreateUnitOfWork();

        protected abstract Task<TUnitOfWork> CreateUnitOfWorkAsync(CancellationToken cancellationToken = default);
    }
}
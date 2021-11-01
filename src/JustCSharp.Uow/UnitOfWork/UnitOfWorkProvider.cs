using System;
using System.Threading;
using System.Threading.Tasks;

namespace JustCSharp.Uow.UnitOfWork
{
    public class UnitOfWorkProvider: IUnitOfWorkProvider
    {
        protected IUnitOfWork _unitOfWork;

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
            return new UnitOfWorkBase();
        }
        
        protected virtual async Task<IUnitOfWork> CreateUnitOfWorkAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new UnitOfWorkBase());
        }
    }

    public abstract class UnitOfWorkProviderOfT<TUnitOfWork> : IUnitOfWorkProviderOfT<TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork
    {
        protected TUnitOfWork _unitOfWork;

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
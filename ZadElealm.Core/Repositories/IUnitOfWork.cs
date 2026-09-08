using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        Task<int> Complete();
        Task<int> Complete(CancellationToken cancellationToken);
        Task BeginTransactionAsync();
        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task CommitTransactionAsync();
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync();
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }
}

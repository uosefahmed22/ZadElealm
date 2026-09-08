using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Core.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetEntityAsync(int id);
        Task<T> GetEntityAsync(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken);
        Task<T> GetEntityWithSpecAsync(ISpecification<T> spec);
        Task<T> GetEntityWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken);
       
        //With no tracking
        Task<IReadOnlyList<T>> GetAllWithNoTrackingAsync();
        Task<IReadOnlyList<T>> GetAllWithNoTrackingAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<T>> GetAllWithSpecNoTrackingAsync(ISpecification<T> spec);
        Task<IReadOnlyList<T>> GetAllWithSpecNoTrackingAsync(ISpecification<T> spec, CancellationToken cancellationToken);
        Task<T> GetEntityWithSpecNoTrackingAsync(ISpecification<T> spec);
        Task<T> GetEntityWithSpecNoTrackingAsync(ISpecification<T> spec, CancellationToken cancellationToken);
        Task<T> GetEntityWithNoTrackingAsync(int id);
        Task<T> GetEntityWithNoTrackingAsync(int id, CancellationToken cancellationToken);
        
        Task<int> CountAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken);
        Task AddAsync(T entity);
        Task AddAsync(T entity, CancellationToken cancellationToken);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
    }
}

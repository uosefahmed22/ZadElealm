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
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetEntityWithSpecAsync(ISpecification<T> spec);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec);
       
        //With no tracking
        Task<IReadOnlyList<T>> GetAllWithNoTrackingAsync();
        Task<IReadOnlyList<T>> GetAllWithSpecNoTrackingAsync(ISpecification<T> spec);
        Task<T> GetEntityWithSpecNoTrackingAsync(ISpecification<T> spec);
        Task<T> GetEntityWithNoTrackingAsync(int id);
        
        Task<int> CountAsync(ISpecification<T> spec);
        Task AddAsync(T entity);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
    }
}

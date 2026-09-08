using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> GetEntityAsync(int id)
            => await GetEntityAsync(id, CancellationToken.None);

        public async Task<T> GetEntityAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext.Set<T>().FindAsync([id], cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entity with id: {ex.Message}");
            }
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
            => await GetAllAsync(CancellationToken.None);

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext.Set<T>().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entities: {ex.Message}");
            }
        }
        public async Task<T> GetEntityWithSpecAsync(ISpecification<T> spec)
            => await GetEntityWithSpecAsync(spec, CancellationToken.None);

        public async Task<T> GetEntityWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            try
            {
                return await ApplySpecification(spec)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entity with specification: {ex.Message}");
            }
        }
        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
            => await GetAllWithSpecAsync(spec, CancellationToken.None);

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            try
            {
                return await ApplySpecification(spec)
                    .AsSplitQuery()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entities with specification: {ex.Message}");
            }
        }

        //With no tracking
        public async Task<IReadOnlyList<T>> GetAllWithNoTrackingAsync()
            => await GetAllWithNoTrackingAsync(CancellationToken.None);

        public async Task<IReadOnlyList<T>> GetAllWithNoTrackingAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entities: {ex.Message}");
            }
        }
        public async Task<IReadOnlyList<T>> GetAllWithSpecNoTrackingAsync(ISpecification<T> spec)
            => await GetAllWithSpecNoTrackingAsync(spec, CancellationToken.None);

        public async Task<IReadOnlyList<T>> GetAllWithSpecNoTrackingAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            try
            {
                return await ApplySpecification(spec)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entities with specification: {ex.Message}");
            }
        }
        public async Task<T> GetEntityWithSpecNoTrackingAsync(ISpecification<T> spec)
            => await GetEntityWithSpecNoTrackingAsync(spec, CancellationToken.None);

        public async Task<T> GetEntityWithSpecNoTrackingAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            try
            {
                return await ApplySpecification(spec)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entity with specification: {ex.Message}");
            }
        }
        public async Task<T> GetEntityWithNoTrackingAsync(int id)
            => await GetEntityWithNoTrackingAsync(id, CancellationToken.None);

        public async Task<T> GetEntityWithNoTrackingAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext.Set<T>().AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve entity with id: {ex.Message}");
            }
        }



        public async Task<int> CountAsync(ISpecification<T> spec)
            => await CountAsync(spec, CancellationToken.None);

        public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken)
        {
            return await ApplySpecification(spec).CountAsync(cancellationToken);
        }
        public async Task AddAsync(T entity)
            => await AddAsync(entity, CancellationToken.None);
        public async Task AddAsync(T entity, CancellationToken cancellationToken)
            => await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        public void Update(T entity)
        => _dbContext.Set<T>().Update(entity);
        public void UpdateRange(IEnumerable<T> entities)
            => _dbContext.Set<T>().UpdateRange(entities);
        public void Delete(T entity)
        => _dbContext.Set<T>().Remove(entity);
        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>(), spec);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        public Hashtable _reposatories;
        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _reposatories = new Hashtable();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            if (!_reposatories.Contains(typeof(TEntity)))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
                _reposatories.Add(typeof(TEntity), repository);
            }
            return _reposatories[typeof(TEntity)] as IGenericRepository<TEntity>;
        }
        public Task<int> Complete()
        => _dbContext.SaveChangesAsync();
        public ValueTask DisposeAsync()
       => _dbContext.DisposeAsync();
        public async Task BeginTransactionAsync()
        {
            await _dbContext.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            await _dbContext.Database.CommitTransactionAsync();
        }
        public async Task RollbackTransactionAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }
    }
}

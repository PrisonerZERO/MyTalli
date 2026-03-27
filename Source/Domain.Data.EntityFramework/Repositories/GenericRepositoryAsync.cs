namespace My.Talli.Domain.Data.EntityFramework.Repositories;

using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

/// <summary>Repository</summary>
public class GenericRepositoryAsync<TEntity> : IRepositoryAsync<TEntity> where TEntity : class, IIdentifiable
{
    #region <Variables>

    protected readonly DbSet<TEntity> _dbSet;
    protected readonly TalliDbContext _dbContext;


    #endregion

    #region <Constructors>

    public GenericRepositoryAsync(TalliDbContext dbContext)
    {
        _dbSet = dbContext.Set<TEntity>();
        _dbContext = dbContext;
    }


    #endregion

    #region <Methods>

    public virtual async Task AddAsync(TEntity entity)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            await _dbSet.AddAsync(entity);
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            return await _dbSet.ToListAsync();
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual async Task<TEntity?> GetByIdAsync(long id)
    {
        await _dbContext.ConcurrencyLock.WaitAsync();

        try
        {
            return await _dbSet.FindAsync(id);
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual void Remove(TEntity entity)
    {
        _dbContext.ConcurrencyLock.Wait();

        try
        {
            _dbSet.Remove(entity);
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }

    public virtual void Update(TEntity entity)
    {
        _dbContext.ConcurrencyLock.Wait();

        try
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        finally
        {
            _dbContext.ConcurrencyLock.Release();
        }
    }


    #endregion
}

namespace My.Talli.Domain.Data.EntityFramework.Repositories;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using My.Talli.Domain.Data.Interfaces;
using My.Talli.Domain.Entities.Interfaces;

/// <summary>Repository</summary>
public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class, IIdentifiable
{
    #region <Variables>

    protected readonly TalliDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    #endregion

    #region <Constructors>

    public GenericRepository(TalliDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    #endregion

    #region <Methods>

    public virtual async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Modified;
    }

    #endregion
}

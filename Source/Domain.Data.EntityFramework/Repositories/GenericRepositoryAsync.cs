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

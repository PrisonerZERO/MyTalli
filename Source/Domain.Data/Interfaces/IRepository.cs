namespace My.Talli.Domain.Data.Interfaces;

using System.Linq.Expressions;
using My.Talli.Domain.Entities.Interfaces;

/// <summary>Repository</summary>
public interface IRepository<T> where T : class, IIdentifiable
{
    #region <Methods>

    Task<T?> GetByIdAsync(long id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);

    #endregion
}

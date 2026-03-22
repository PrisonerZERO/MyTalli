namespace My.Talli.UnitTesting.Infrastructure.Stubs;

using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
using System.Linq.Expressions;

/// <summary>Stub</summary>
public class AuditableRepositoryStub<TEntity> : IAuditableRepositoryAsync<TEntity> where TEntity : class, IAuditableIdentifiable
{
	#region <Variables>

	private readonly IdentityProvider _identityProvider;
	private readonly List<TEntity> _store = [];

	#endregion

	#region <Properties>

	public IAuditResolver<TEntity> AuditResolver { get; }

	#endregion

	#region <Constructors>

	public AuditableRepositoryStub(IAuditResolver<TEntity> auditResolver, IdentityProvider identityProvider)
	{
		AuditResolver = auditResolver;
		_identityProvider = identityProvider;
	}

	#endregion

	#region <Methods>

	public Task AddAsync(TEntity entity)
	{
		if (entity.Id == 0)
			entity.Id = _identityProvider.Next<TEntity>();

		_store.Add(entity);
		
		return Task.CompletedTask;
	}

	public Task DeleteAsync(TEntity entity)
	{
		var existing = _store.FirstOrDefault(e => e.Id == entity.Id);
		if (existing is not null) 
			_store.Remove(existing);

		return Task.CompletedTask;
	}

	public Task DeleteRangeAsync(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			var existing = _store.FirstOrDefault(e => e.Id == entity.Id);
			if (existing is not null) 
				_store.Remove(existing);
		}

		return Task.CompletedTask;
	}

	public Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
	{
		var compiled = predicate.Compile();
		return Task.FromResult(_store.Where(compiled));
	}

	public Task<IEnumerable<TEntity>> GetAllAsync()
	{
		return Task.FromResult<IEnumerable<TEntity>>(_store);
	}

	public Task<TEntity?> GetByIdAsync(long id)
	{
		return Task.FromResult(_store.FirstOrDefault(e => e.Id == id));
	}

	public Task InsertAsync(TEntity entity)
	{
		if (entity.Id == 0)
			entity.Id = _identityProvider.Next<TEntity>();

		AuditResolver.Resolve(entity);
		_store.Add(entity);
		
		return Task.CompletedTask;
	}

	public Task InsertRangeAsync(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			if (entity.Id == 0)
				entity.Id = _identityProvider.Next<TEntity>();

			AuditResolver.Resolve(entity);
			_store.Add(entity);
		}

		return Task.CompletedTask;
	}

	public void Remove(TEntity entity)
	{
		var existing = _store.FirstOrDefault(e => e.Id == entity.Id);
		if (existing is not null) 
			_store.Remove(existing);
	}

	public Task<int> SaveChangesAsync()
	{
		return Task.FromResult(0);
	}

	public void Update(TEntity entity)
	{
		var index = _store.FindIndex(e => e.Id == entity.Id);
		if (index >= 0) 
			_store[index] = entity;
	}

	public Task UpdateAsync(TEntity entity)
	{
		AuditResolver.Resolve(entity, updating: true);
		var index = _store.FindIndex(e => e.Id == entity.Id);
		if (index >= 0) 
			_store[index] = entity;

		return Task.CompletedTask;
	}

	public Task UpdateRangeAsync(IEnumerable<TEntity> entities)
	{
		foreach (var entity in entities)
		{
			AuditResolver.Resolve(entity, updating: true);
			var index = _store.FindIndex(e => e.Id == entity.Id);
			if (index >= 0) 
				_store[index] = entity;
		}

		return Task.CompletedTask;
	}

	#endregion
}

namespace My.Talli.Domain.Repositories;

using System.Linq.Expressions;
using AutoMapper;
using Domain.Data.Interfaces;
using Domain.Entities.Interfaces;
using Domain.Framework;

/// <summary>Adapter</summary>
public class RepositoryAdapterAsync<TModel, TEntity>
	where TModel : class
	where TEntity : class, IAuditableIdentifiable
{
	#region <Variables>

	private readonly IAuditableRepositoryAsync<TEntity> _repository;
	private readonly IMapper _mapper;

	#endregion

	#region <Constructors>

	public RepositoryAdapterAsync(IAuditableRepositoryAsync<TEntity> repository, IMapper mapper)
	{
		_mapper = mapper;
		_repository = repository;
	}

	#endregion

	#region <Methods>

	public async Task DeleteAsync(TModel model)
	{
		var entity = MapAndValidate(model);
		await _repository.DeleteAsync(entity);
	}

	public async Task DeleteRangeAsync(IEnumerable<TModel> models)
	{
		Assert.IsNotNull(models);

		var entities = _mapper.Map<IEnumerable<TEntity>>(models);
		await _repository.DeleteRangeAsync(entities);
	}

	public async Task<IEnumerable<TModel>> FindAsync(Expression<Func<TEntity, bool>> predicate)
	{
		var entities = await _repository.FindAsync(predicate);
		return _mapper.Map<IEnumerable<TModel>>(entities);
	}

	public async Task<IEnumerable<TModel>> GetAllAsync()
	{
		var entities = await _repository.GetAllAsync();
		return _mapper.Map<IEnumerable<TModel>>(entities);
	}

	public async Task<TModel?> GetByIdAsync(long id)
	{
		var entity = await _repository.GetByIdAsync(id);
		return entity is null ? null : _mapper.Map<TModel>(entity);
	}

	public async Task<TModel> InsertAsync(TModel model)
	{
		var entity = MapAndValidate(model);
		await _repository.InsertAsync(entity);

		_mapper.Map(entity, model);
		return model;
	}

	public async Task InsertRangeAsync(IEnumerable<TModel> models)
	{
		Assert.IsNotNull(models);

		var entities = _mapper.Map<IEnumerable<TEntity>>(models);
		await _repository.InsertRangeAsync(entities);
	}

	public async Task<TModel> UpdateAsync(TModel model)
	{
		var entity = MapAndValidate(model);
		await _repository.UpdateAsync(entity);

		_mapper.Map(entity, model);
		return model;
	}

	public async Task UpdateRangeAsync(IEnumerable<TModel> models)
	{
		Assert.IsNotNull(models);

		var entities = _mapper.Map<IEnumerable<TEntity>>(models);
		await _repository.UpdateRangeAsync(entities);
	}

	private TEntity MapAndValidate(TModel model)
	{
		Assert.IsNotNull(model);
		return _mapper.Map<TEntity>(model);
	}

	#endregion
}

namespace My.Talli.Domain.Mappers;

/// <summary>Mapper</summary>
public abstract class EntityMapperBase<TModel, TEntity> : IEntityMapper<TModel, TEntity>
	where TModel : class
	where TEntity : class
{
	#region <Methods>

	public abstract void ApplyTo(TEntity source, TModel destination);

	public abstract TEntity ToEntity(TModel model);

	public IEnumerable<TEntity> ToEntities(IEnumerable<TModel> models) => models.Select(ToEntity);

	public abstract TModel ToModel(TEntity entity);

	public IEnumerable<TModel> ToModels(IEnumerable<TEntity> entities) => entities.Select(ToModel);

	#endregion
}

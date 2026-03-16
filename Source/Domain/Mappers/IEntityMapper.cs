namespace My.Talli.Domain.Mappers;

/// <summary>Mapper</summary>
public interface IEntityMapper<TModel, TEntity>
	where TModel : class
	where TEntity : class
{
	void ApplyTo(TEntity source, TModel destination);
	TEntity ToEntity(TModel model);
	IEnumerable<TEntity> ToEntities(IEnumerable<TModel> models);
	TModel ToModel(TEntity entity);
	IEnumerable<TModel> ToModels(IEnumerable<TEntity> entities);
}

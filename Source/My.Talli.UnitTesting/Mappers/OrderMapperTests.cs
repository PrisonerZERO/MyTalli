namespace My.Talli.UnitTesting.Mappers;

using Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Tests</summary>
public class OrderMapperTests
{
	#region <Variables>

	private readonly OrderMapper _mapper = new();

	#endregion

	#region <Methods>

	[Fact]
	public void ApplyTo_OverwritesAllProperties()
	{
		var entity = CreateEntity();
		var model = new MODELS.Order();

		_mapper.ApplyTo(entity, model);

		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.OrderDateTime, model.OrderDateTime);
		Assert.Equal(entity.TaxCharged, model.TaxCharged);
		Assert.Equal(entity.UserId, model.UserId);
	}

	[Fact]
	public void ToEntities_Collection_MapsAll()
	{
		var models = new List<MODELS.Order> { CreateModel(), CreateModel() };
		var entities = _mapper.ToEntities(models).ToList();

		Assert.Equal(2, entities.Count);
	}

	[Fact]
	public void ToEntity_MapsAllProperties()
	{
		var model = CreateModel();
		var entity = _mapper.ToEntity(model);

		Assert.Equal(model.Id, entity.Id);
		Assert.Equal(model.IsDeleted, entity.IsDeleted);
		Assert.Equal(model.IsVisible, entity.IsVisible);
		Assert.Equal(model.OrderDateTime, entity.OrderDateTime);
		Assert.Equal(model.TaxCharged, entity.TaxCharged);
		Assert.Equal(model.UserId, entity.UserId);
	}

	[Fact]
	public void ToModel_MapsAllProperties()
	{
		var entity = CreateEntity();
		var model = _mapper.ToModel(entity);

		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.OrderDateTime, model.OrderDateTime);
		Assert.Equal(entity.TaxCharged, model.TaxCharged);
		Assert.Equal(entity.UserId, model.UserId);
	}

	[Fact]
	public void ToModels_Collection_MapsAll()
	{
		var entities = new List<ENTITIES.Order> { CreateEntity(), CreateEntity() };
		var models = _mapper.ToModels(entities).ToList();

		Assert.Equal(2, models.Count);
	}

	private static ENTITIES.Order CreateEntity() => new()
	{
		Id = 10,
		IsDeleted = false,
		IsVisible = true,
		OrderDateTime = new DateTime(2026, 3, 15, 14, 30, 0),
		TaxCharged = 2.50m,
		UserId = 42,
	};

	private static MODELS.Order CreateModel() => new()
	{
		Id = 10,
		IsDeleted = false,
		IsVisible = true,
		OrderDateTime = new DateTime(2026, 3, 15, 14, 30, 0),
		TaxCharged = 2.50m,
		UserId = 42,
	};

	#endregion
}

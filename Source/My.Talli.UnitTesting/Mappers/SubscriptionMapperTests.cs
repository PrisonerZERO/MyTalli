namespace My.Talli.UnitTesting.Mappers;

using Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Tests</summary>
public class SubscriptionMapperTests
{
	#region <Variables>

	private readonly SubscriptionMapper _mapper = new();

	#endregion

	#region <Methods>

	[Fact]
	public void ApplyTo_OverwritesAllProperties()
	{
		var entity = CreateEntity();
		var model = new MODELS.Subscription();

		_mapper.ApplyTo(entity, model);

		Assert.Equal(entity.CancelledDate, model.CancelledDate);
		Assert.Equal(entity.EndDate, model.EndDate);
		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.OrderItemId, model.OrderItemId);
		Assert.Equal(entity.ProductId, model.ProductId);
		Assert.Equal(entity.RenewalDate, model.RenewalDate);
		Assert.Equal(entity.StartDate, model.StartDate);
		Assert.Equal(entity.Status, model.Status);
		Assert.Equal(entity.UserId, model.UserId);
	}

	[Fact]
	public void ToEntities_Collection_MapsAll()
	{
		var models = new List<MODELS.Subscription> { CreateModel(), CreateModel() };

		var entities = _mapper.ToEntities(models).ToList();

		Assert.Equal(2, entities.Count);
	}

	[Fact]
	public void ToEntity_MapsAllProperties()
	{
		var model = CreateModel();

		var entity = _mapper.ToEntity(model);

		Assert.Equal(model.CancelledDate, entity.CancelledDate);
		Assert.Equal(model.EndDate, entity.EndDate);
		Assert.Equal(model.Id, entity.Id);
		Assert.Equal(model.IsDeleted, entity.IsDeleted);
		Assert.Equal(model.IsVisible, entity.IsVisible);
		Assert.Equal(model.OrderItemId, entity.OrderItemId);
		Assert.Equal(model.ProductId, entity.ProductId);
		Assert.Equal(model.RenewalDate, entity.RenewalDate);
		Assert.Equal(model.StartDate, entity.StartDate);
		Assert.Equal(model.Status, entity.Status);
		Assert.Equal(model.UserId, entity.UserId);
	}

	[Fact]
	public void ToModel_MapsAllProperties()
	{
		var entity = CreateEntity();

		var model = _mapper.ToModel(entity);

		Assert.Equal(entity.CancelledDate, model.CancelledDate);
		Assert.Equal(entity.EndDate, model.EndDate);
		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.OrderItemId, model.OrderItemId);
		Assert.Equal(entity.ProductId, model.ProductId);
		Assert.Equal(entity.RenewalDate, model.RenewalDate);
		Assert.Equal(entity.StartDate, model.StartDate);
		Assert.Equal(entity.Status, model.Status);
		Assert.Equal(entity.UserId, model.UserId);
	}

	[Fact]
	public void ToModels_Collection_MapsAll()
	{
		var entities = new List<ENTITIES.Subscription> { CreateEntity(), CreateEntity() };

		var models = _mapper.ToModels(entities).ToList();

		Assert.Equal(2, models.Count);
	}

	private static ENTITIES.Subscription CreateEntity() => new()
	{
		CancelledDate = new DateTime(2027, 3, 1),
		EndDate = new DateTime(2027, 3, 22),
		Id = 5,
		IsDeleted = false,
		IsVisible = true,
		OrderItemId = 100,
		ProductId = 1,
		RenewalDate = new DateTime(2027, 3, 22),
		StartDate = new DateTime(2026, 3, 22),
		Status = "Active",
		UserId = 42,
	};

	private static MODELS.Subscription CreateModel() => new()
	{
		CancelledDate = new DateTime(2027, 3, 1),
		EndDate = new DateTime(2027, 3, 22),
		Id = 5,
		IsDeleted = false,
		IsVisible = true,
		OrderItemId = 100,
		ProductId = 1,
		RenewalDate = new DateTime(2027, 3, 22),
		StartDate = new DateTime(2026, 3, 22),
		Status = "Active",
		UserId = 42,
	};

	#endregion
}

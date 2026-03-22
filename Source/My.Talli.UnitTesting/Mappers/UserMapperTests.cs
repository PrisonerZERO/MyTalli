namespace My.Talli.UnitTesting.Mappers;

using Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Tests</summary>
public class UserMapperTests
{
	#region <Variables>

	private readonly UserMapper _mapper = new();

	#endregion

	#region <Methods>

	[Fact]
	public void ApplyTo_OverwritesAllProperties()
	{
		var entity = CreateEntity();
		var model = new MODELS.User();

		_mapper.ApplyTo(entity, model);

		Assert.Equal(entity.DisplayName, model.DisplayName);
		Assert.Equal(entity.FirstName, model.FirstName);
		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.InitialProvider, model.InitialProvider);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.LastLoginAt, model.LastLoginAt);
		Assert.Equal(entity.LastName, model.LastName);
		Assert.Equal(entity.PreferredProvider, model.PreferredProvider);
		Assert.Equal(entity.UserPreferences, model.UserPreferences);
	}

	[Fact]
	public void ToEntities_Collection_MapsAll()
	{
		var models = new List<MODELS.User> { CreateModel(), CreateModel() };

		var entities = _mapper.ToEntities(models).ToList();

		Assert.Equal(2, entities.Count);
	}

	[Fact]
	public void ToEntity_MapsAllProperties()
	{
		var model = CreateModel();

		var entity = _mapper.ToEntity(model);

		Assert.Equal(model.DisplayName, entity.DisplayName);
		Assert.Equal(model.FirstName, entity.FirstName);
		Assert.Equal(model.Id, entity.Id);
		Assert.Equal(model.InitialProvider, entity.InitialProvider);
		Assert.Equal(model.IsDeleted, entity.IsDeleted);
		Assert.Equal(model.IsVisible, entity.IsVisible);
		Assert.Equal(model.LastLoginAt, entity.LastLoginAt);
		Assert.Equal(model.LastName, entity.LastName);
		Assert.Equal(model.PreferredProvider, entity.PreferredProvider);
		Assert.Equal(model.UserPreferences, entity.UserPreferences);
	}

	[Fact]
	public void ToModel_DoesNotMapHandlerFields()
	{
		var entity = CreateEntity();

		var model = _mapper.ToModel(entity);

		Assert.False(model.IsNewUser);
		Assert.Empty(model.Roles);
	}

	[Fact]
	public void ToModel_MapsAllProperties()
	{
		var entity = CreateEntity();

		var model = _mapper.ToModel(entity);

		Assert.Equal(entity.DisplayName, model.DisplayName);
		Assert.Equal(entity.FirstName, model.FirstName);
		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.InitialProvider, model.InitialProvider);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.LastLoginAt, model.LastLoginAt);
		Assert.Equal(entity.LastName, model.LastName);
		Assert.Equal(entity.PreferredProvider, model.PreferredProvider);
		Assert.Equal(entity.UserPreferences, model.UserPreferences);
	}

	[Fact]
	public void ToModels_Collection_MapsAll()
	{
		var entities = new List<ENTITIES.User> { CreateEntity(), CreateEntity() };

		var models = _mapper.ToModels(entities).ToList();

		Assert.Equal(2, models.Count);
	}

	private static ENTITIES.User CreateEntity() => new()
	{
		DisplayName = "Test User",
		FirstName = "Test",
		Id = 42,
		InitialProvider = "Google",
		IsDeleted = false,
		IsVisible = true,
		LastLoginAt = new DateTime(2026, 3, 22, 10, 0, 0),
		LastName = "User",
		PreferredProvider = "Google",
		UserPreferences = "{\"funGreetings\":true}",
	};

	private static MODELS.User CreateModel() => new()
	{
		DisplayName = "Test User",
		FirstName = "Test",
		Id = 42,
		InitialProvider = "Google",
		IsDeleted = false,
		IsVisible = true,
		LastLoginAt = new DateTime(2026, 3, 22, 10, 0, 0),
		LastName = "User",
		PreferredProvider = "Google",
		UserPreferences = "{\"funGreetings\":true}",
	};

	#endregion
}

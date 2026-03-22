namespace My.Talli.UnitTesting.Mappers;

using Domain.Mappers;

using ENTITIES = Domain.Entities;
using MODELS = Domain.Models;

/// <summary>Tests</summary>
public class UserAuthenticationGoogleMapperTests
{
	#region <Variables>

	private readonly UserAuthenticationGoogleMapper _mapper = new();

	#endregion

	#region <Methods>

	[Fact]
	public void ApplyTo_OverwritesAllProperties()
	{
		var entity = CreateEntity();
		var model = new MODELS.UserAuthenticationGoogle();

		_mapper.ApplyTo(entity, model);

		Assert.Equal(entity.AvatarUrl, model.AvatarUrl);
		Assert.Equal(entity.DisplayName, model.DisplayName);
		Assert.Equal(entity.Email, model.Email);
		Assert.Equal(entity.EmailVerified, model.EmailVerified);
		Assert.Equal(entity.FirstName, model.FirstName);
		Assert.Equal(entity.GoogleId, model.GoogleId);
		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.LastName, model.LastName);
		Assert.Equal(entity.Locale, model.Locale);
	}

	[Fact]
	public void ToEntities_Collection_MapsAll()
	{
		var models = new List<MODELS.UserAuthenticationGoogle> { CreateModel(), CreateModel() };

		var entities = _mapper.ToEntities(models).ToList();

		Assert.Equal(2, entities.Count);
	}

	[Fact]
	public void ToEntity_MapsAllProperties()
	{
		var model = CreateModel();

		var entity = _mapper.ToEntity(model);

		Assert.Equal(model.AvatarUrl, entity.AvatarUrl);
		Assert.Equal(model.DisplayName, entity.DisplayName);
		Assert.Equal(model.Email, entity.Email);
		Assert.Equal(model.EmailVerified, entity.EmailVerified);
		Assert.Equal(model.FirstName, entity.FirstName);
		Assert.Equal(model.GoogleId, entity.GoogleId);
		Assert.Equal(model.Id, entity.Id);
		Assert.Equal(model.IsDeleted, entity.IsDeleted);
		Assert.Equal(model.IsVisible, entity.IsVisible);
		Assert.Equal(model.LastName, entity.LastName);
		Assert.Equal(model.Locale, entity.Locale);
	}

	[Fact]
	public void ToModel_MapsAllProperties()
	{
		var entity = CreateEntity();

		var model = _mapper.ToModel(entity);

		Assert.Equal(entity.AvatarUrl, model.AvatarUrl);
		Assert.Equal(entity.DisplayName, model.DisplayName);
		Assert.Equal(entity.Email, model.Email);
		Assert.Equal(entity.EmailVerified, model.EmailVerified);
		Assert.Equal(entity.FirstName, model.FirstName);
		Assert.Equal(entity.GoogleId, model.GoogleId);
		Assert.Equal(entity.Id, model.Id);
		Assert.Equal(entity.IsDeleted, model.IsDeleted);
		Assert.Equal(entity.IsVisible, model.IsVisible);
		Assert.Equal(entity.LastName, model.LastName);
		Assert.Equal(entity.Locale, model.Locale);
	}

	[Fact]
	public void ToModels_Collection_MapsAll()
	{
		var entities = new List<ENTITIES.UserAuthenticationGoogle> { CreateEntity(), CreateEntity() };

		var models = _mapper.ToModels(entities).ToList();

		Assert.Equal(2, models.Count);
	}

	private static ENTITIES.UserAuthenticationGoogle CreateEntity() => new()
	{
		AvatarUrl = "https://example.com/avatar.jpg",
		DisplayName = "Test User",
		Email = "test@gmail.com",
		EmailVerified = true,
		FirstName = "Test",
		GoogleId = "google-123456",
		Id = 42,
		IsDeleted = false,
		IsVisible = true,
		LastName = "User",
		Locale = "en",
	};

	private static MODELS.UserAuthenticationGoogle CreateModel() => new()
	{
		AvatarUrl = "https://example.com/avatar.jpg",
		DisplayName = "Test User",
		Email = "test@gmail.com",
		EmailVerified = true,
		FirstName = "Test",
		GoogleId = "google-123456",
		Id = 42,
		IsDeleted = false,
		IsVisible = true,
		LastName = "User",
		Locale = "en",
	};

	#endregion
}

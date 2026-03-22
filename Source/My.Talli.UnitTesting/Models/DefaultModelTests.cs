namespace My.Talli.UnitTesting.Models;

using Domain.Models;

/// <summary>Tests</summary>
public class DefaultModelTests
{
	#region <Methods>

	[Fact]
	public void NewInstance_Id_DefaultsToZero()
	{
		var model = new DefaultModel();

		Assert.Equal(0, model.Id);
	}

	[Fact]
	public void NewInstance_IsDeleted_DefaultsToFalse()
	{
		var model = new DefaultModel();

		Assert.False(model.IsDeleted);
	}

	[Fact]
	public void NewInstance_IsVisible_DefaultsToTrue()
	{
		var model = new DefaultModel();

		Assert.True(model.IsVisible);
	}

	#endregion
}

namespace My.Talli.UnitTesting.Models;

using Domain.Models;

/// <summary>Tests</summary>
public class ValidationResultTests
{
	#region <Methods>

	[Fact]
	public void IsValid_WithValidationError_ReturnsFalse()
	{
		var response = new ActionResponseOf<string>();
		response.ValidationSummary.Add("Something went wrong.");

		Assert.False(response.IsValid);
	}

	[Fact]
	public void NewInstance_IsValid_DefaultsToTrue()
	{
		var response = new ActionResponseOf<string>();

		Assert.True(response.IsValid);
	}

	[Fact]
	public void NewInstance_ValidationSummary_IsEmptyList()
	{
		var response = new ActionResponseOf<string>();

		Assert.NotNull(response.ValidationSummary);
		Assert.Empty(response.ValidationSummary);
	}

	[Fact]
	public void NewInstance_WarningSummary_IsEmptyList()
	{
		var response = new ActionResponseOf<string>();

		Assert.NotNull(response.WarningSummary);
		Assert.Empty(response.WarningSummary);
	}

	#endregion
}

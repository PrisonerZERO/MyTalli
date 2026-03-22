namespace My.Talli.UnitTesting.Models;

using Domain.Models;

/// <summary>Tests</summary>
public class ActionResponseTests
{
	#region <Methods>

	[Fact]
	public void Payload_SetAndGet_ReturnsValue()
	{
		var response = new ActionResponseOf<string> { Payload = "test" };

		Assert.Equal("test", response.Payload);
	}

	[Fact]
	public void Payload_WithIntType_SetAndGet_ReturnsValue()
	{
		var response = new ActionResponseOf<int> { Payload = 42 };

		Assert.Equal(42, response.Payload);
	}

	#endregion
}

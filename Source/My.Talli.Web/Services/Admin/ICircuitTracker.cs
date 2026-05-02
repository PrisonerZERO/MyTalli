namespace My.Talli.Web.Services.Admin;

/// <summary>Service</summary>
public interface ICircuitTracker
{
	#region <Properties>

	int InAppNonAdminCount { get; }


	#endregion

	#region <Events>

	event Action CountChanged;


	#endregion

	#region <Methods>

	void RegisterInAppSession(string sessionId, bool isAdmin);

	void UnregisterInAppSession(string sessionId);


	#endregion
}

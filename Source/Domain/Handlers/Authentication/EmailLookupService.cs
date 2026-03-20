namespace My.Talli.Domain.Handlers.Authentication;

using Domain.Models;
using Domain.Repositories;

using ENTITIES = Domain.Entities;

/// <summary>Service</summary>
public class EmailLookupService
{
	#region <Variables>

	private readonly RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> _appleAuthAdapter;
	private readonly RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> _googleAuthAdapter;
	private readonly RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> _microsoftAuthAdapter;


	#endregion

	#region <Constructors>

	public EmailLookupService(
		RepositoryAdapterAsync<UserAuthenticationApple, ENTITIES.UserAuthenticationApple> appleAuthAdapter,
		RepositoryAdapterAsync<UserAuthenticationGoogle, ENTITIES.UserAuthenticationGoogle> googleAuthAdapter,
		RepositoryAdapterAsync<UserAuthenticationMicrosoft, ENTITIES.UserAuthenticationMicrosoft> microsoftAuthAdapter)
	{
		_appleAuthAdapter = appleAuthAdapter;
		_googleAuthAdapter = googleAuthAdapter;
		_microsoftAuthAdapter = microsoftAuthAdapter;
	}


	#endregion

	#region <Methods>

	public async Task<long?> FindUserIdByEmailAsync(string email)
	{
		var google = (await _googleAuthAdapter.FindAsync(x => x.Email == email)).FirstOrDefault();
		if (google is not null) return google.Id;

		var microsoft = (await _microsoftAuthAdapter.FindAsync(x => x.Email == email)).FirstOrDefault();
		if (microsoft is not null) return microsoft.Id;

		var apple = (await _appleAuthAdapter.FindAsync(x => x.Email == email)).FirstOrDefault();
		if (apple is not null) return apple.Id;

		return null;
	}


	#endregion
}

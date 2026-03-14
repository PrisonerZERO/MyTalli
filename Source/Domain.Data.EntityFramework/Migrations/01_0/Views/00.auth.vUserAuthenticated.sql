-- DROP and CREATE the procedure will loose the security settings.
IF object_id('[auth].[vUserAuthenticated]') IS NULL
    EXEC ('CREATE VIEW [auth].[vUserAuthenticated] AS SELECT 1 AS Id')
GO

ALTER VIEW [auth].[vUserAuthenticated]
AS
    /*
		View: [auth].[vUserAuthenticated]
		Creation Date: 3/14/2026

		Purpose: 
		To lists Authenticated User detail
	*/
	SELECT
		-- User
		users.Id
		, users.DisplayName
		, users.FirstName
		, users.LastName
		, users.InitialProvider
		, users.PreferredProvider

		-- Provider (standardized)
		, NULLIF(CASE users.PreferredProvider
			WHEN 'Apple'     THEN appleUser.AppleId
			WHEN 'Google'    THEN googleUser.GoogleId
			WHEN 'Microsoft' THEN microsoftUser.MicrosoftId
		  END, '') AS ProviderId

		, NULLIF(CASE users.PreferredProvider
			WHEN 'Apple'     THEN appleUser.Email
			WHEN 'Google'    THEN googleUser.Email
			WHEN 'Microsoft' THEN microsoftUser.Email
		  END, '') AS EmailAddress

		, NULLIF(CASE users.PreferredProvider
			WHEN 'Google'    THEN googleUser.AvatarUrl
			ELSE NULL
		  END, '') AS ProviderAvatarUrl

		, CASE users.PreferredProvider
			WHEN 'Google'    THEN CAST(googleUser.EmailVerified AS BIT)
			ELSE NULL
		  END AS ProviderEmailVerified

		, CASE users.PreferredProvider
			WHEN 'Apple'     THEN CAST(appleUser.IsPrivateRelay AS BIT)
			ELSE NULL
		  END AS ProviderIsPrivateRelay

		, NULLIF(CASE users.PreferredProvider
			WHEN 'Google'    THEN googleUser.Locale
			ELSE NULL
		  END, '') AS ProviderLocale

		-- Standard
		, users.UserPreferences
		, users.LastLoginAt
    
		-- Audit
		, users.CreateByUserId
		, users.CreatedOnDateTime
		, users.UpdatedByUserId
		, users.UpdatedOnDate
		, users.IsActive
	FROM [auth].[User] users
	LEFT JOIN [auth].[UserAuthenticationApple]     appleUser     ON appleUser.UserId     = users.Id
	LEFT JOIN [auth].[UserAuthenticationGoogle]    googleUser    ON googleUser.UserId    = users.Id
	LEFT JOIN [auth].[UserAuthenticationMicrosoft] microsoftUser ON microsoftUser.UserId = users.Id
GO
-- Seed initial suggestions (using the first admin user)
IF NOT EXISTS (SELECT 1 FROM [app].[Suggestion])
BEGIN
    DECLARE @UserId BIGINT = (SELECT TOP 1 [Id] FROM [auth].[User] ORDER BY [Id]);

    IF @UserId IS NOT NULL
    BEGIN
        INSERT INTO [app].[Suggestion] ([UserId], [Category], [Description], [Status], [Title], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
        VALUES
            (@UserId, N'Feature', N'Would love a dark mode option for late-night revenue tracking. The current light theme can be harsh on the eyes.', N'Submitted', N'Dark Mode Support', 0, 1, @UserId, DATEADD(DAY, -3, GETUTCDATE())),
            (@UserId, N'Export', N'Allow users to select a custom date range when exporting CSV data, rather than exporting everything at once.', N'Submitted', N'CSV Export with Date Range', 0, 1, @UserId, DATEADD(DAY, -5, GETUTCDATE())),
            (@UserId, N'Integration', N'Use Stripe webhooks so new transactions appear in the dashboard instantly instead of polling.', N'Submitted', N'Stripe Webhook Real-Time Sync', 0, 1, @UserId, DATEADD(DAY, -7, GETUTCDATE())),
            (@UserId, N'Feature', N'Support displaying and converting revenue in different currencies for international sellers.', N'Submitted', N'Multi-Currency Support', 0, 1, @UserId, DATEADD(DAY, -7, GETUTCDATE())),
            (@UserId, N'UI / UX', N'Let users drag and rearrange dashboard cards to prioritize what they care about most.', N'Submitted', N'Customizable Dashboard Widgets', 0, 1, @UserId, DATEADD(DAY, -14, GETUTCDATE())),
            (@UserId, N'Feature', N'Get a weekly digest email with revenue highlights, top-performing platforms, and goal progress.', N'Submitted', N'Weekly Email Summary', 0, 1, @UserId, DATEADD(DAY, -14, GETUTCDATE()));
    END
END
GO

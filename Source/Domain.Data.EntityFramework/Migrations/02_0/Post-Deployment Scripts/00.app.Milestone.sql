-- Seed milestone data for the waitlist progress tracker
-- Beta milestones (MilestoneGroup = 'Beta')
-- Full Launch milestones (MilestoneGroup = 'FullLaunch')

IF NOT EXISTS (SELECT 1 FROM [app].[Milestone])
BEGIN
    SET IDENTITY_INSERT [app].[Milestone] ON;

    INSERT INTO [app].[Milestone] ([Id], [Description], [MilestoneGroup], [SortOrder], [Status], [Title], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES
        (1,  N'Sign in with Google, Apple, and Microsoft',                       N'Beta',       1,  N'Complete',  N'Authentication',          0, 1, 0, GETUTCDATE()),
        (2,  N'Landing page and waitlist progress tracker',                      N'Beta',       2,  N'Complete',  N'Landing Page & Waitlist', 0, 1, 0, GETUTCDATE()),
        (3,  N'Connector & Dashboard',                                           N'Beta',       3,  N'Upcoming',  N'Stripe Integration',      0, 1, 0, GETUTCDATE()),
        (4,  N'Connector & Dashboard',                                           N'Beta',       4,  N'Upcoming',  N'Etsy Integration',        0, 1, 0, GETUTCDATE()),
        (5,  N'Connector & Dashboard',                                           N'Beta',       5,  N'Upcoming',  N'Gumroad Integration',     0, 1, 0, GETUTCDATE()),
        (6,  N'Unified revenue view across all connected platforms',             N'Beta',       6,  N'Upcoming',  N'Aggregate Dashboard',     0, 1, 0, GETUTCDATE()),
        (7,  N'Manage connected platforms and sync status',                      N'Beta',       7,  N'Upcoming',  N'Platforms',               0, 1, 0, GETUTCDATE()),
        (8,  N'Submit and vote on feature ideas',                                N'Beta',       8,  N'Upcoming',  N'Suggestion Box',          0, 1, 0, GETUTCDATE()),
        (9,  N'User profile and preferences',                                   N'Beta',       9,  N'Upcoming',  N'Settings',                0, 1, 0, GETUTCDATE()),
        (10, N'Free and Pro tier management',                                    N'Beta',       10, N'Upcoming',  N'Subscription & Billing',  0, 1, 0, GETUTCDATE()),
        (11, N'Early access for waitlist members — you''ll be first in line',    N'Beta',       11, N'Upcoming',  N'Beta Launch',             0, 1, 0, GETUTCDATE()),
        (12, N'Connector & Dashboard',                                           N'FullLaunch', 1,  N'Upcoming',  N'PayPal Integration',      0, 1, 0, GETUTCDATE()),
        (13, N'Connector & Dashboard',                                           N'FullLaunch', 2,  N'Upcoming',  N'Shopify Integration',     0, 1, 0, GETUTCDATE()),
        (14, N'Set monthly revenue targets and track progress',                  N'FullLaunch', 3,  N'Upcoming',  N'Goals',                   0, 1, 0, GETUTCDATE()),
        (15, N'Download revenue data for tax prep and bookkeeping',              N'FullLaunch', 4,  N'Upcoming',  N'CSV Export',              0, 1, 0, GETUTCDATE()),
        (16, N'Weekly revenue digest via email',                                 N'FullLaunch', 5,  N'Upcoming',  N'Weekly Email Summaries',  0, 1, 0, GETUTCDATE()),
        (17, N'Open to everyone',                                                N'FullLaunch', 6,  N'Upcoming',  N'Full Launch',             0, 1, 0, GETUTCDATE());

    SET IDENTITY_INSERT [app].[Milestone] OFF;
END
GO

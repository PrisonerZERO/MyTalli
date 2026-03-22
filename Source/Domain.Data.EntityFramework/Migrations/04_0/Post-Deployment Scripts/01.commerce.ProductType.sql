-- Seed ProductType: Software Subscription

IF NOT EXISTS (SELECT 1 FROM [commerce].[ProductType] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [commerce].[ProductType] ON;

    INSERT INTO [commerce].[ProductType] ([Id], [ProductTypeName], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES (1, N'Software Subscription', 0, 1, 0, GETUTCDATE());

    SET IDENTITY_INSERT [commerce].[ProductType] OFF;
END
GO

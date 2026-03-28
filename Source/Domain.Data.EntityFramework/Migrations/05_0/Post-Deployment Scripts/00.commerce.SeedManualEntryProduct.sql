-- Seed ProductType: Software Module (Id = 2)

IF NOT EXISTS (SELECT 1 FROM [commerce].[ProductType] WHERE [Id] = 2)
BEGIN
    SET IDENTITY_INSERT [commerce].[ProductType] ON;

    INSERT INTO [commerce].[ProductType] ([Id], [ProductTypeName], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES (2, N'Software Module', 0, 1, 0, GETUTCDATE());

    SET IDENTITY_INSERT [commerce].[ProductType] OFF;
END
GO

-- Seed Product: Manual Entry Module ($3/mo, Id = 3)

IF NOT EXISTS (SELECT 1 FROM [commerce].[Product] WHERE [Id] = 3)
BEGIN
    SET IDENTITY_INSERT [commerce].[Product] ON;

    INSERT INTO [commerce].[Product] ([Id], [ProductTypeId], [VendorId], [ProductName], [VendorPrice], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES (3, 2, 1, N'Manual Entry Module', 3.00, 0, 1, 0, GETUTCDATE());

    SET IDENTITY_INSERT [commerce].[Product] OFF;
END
GO

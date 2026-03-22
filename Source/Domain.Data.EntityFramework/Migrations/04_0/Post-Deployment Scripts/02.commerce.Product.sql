-- Seed Products: Pro Monthly ($12) and Pro Yearly ($99)

IF NOT EXISTS (SELECT 1 FROM [commerce].[Product] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [commerce].[Product] ON;

    INSERT INTO [commerce].[Product] ([Id], [ProductTypeId], [VendorId], [ProductName], [VendorPrice], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES
        (1, 1, 1, N'Pro Monthly', 12.00, 0, 1, 0, GETUTCDATE()),
        (2, 1, 1, N'Pro Yearly',  99.00, 0, 1, 0, GETUTCDATE());

    SET IDENTITY_INSERT [commerce].[Product] OFF;
END
GO

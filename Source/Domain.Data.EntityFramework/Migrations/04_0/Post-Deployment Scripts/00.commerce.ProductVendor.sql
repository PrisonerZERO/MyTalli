-- Seed ProductVendor: MyTalli

IF NOT EXISTS (SELECT 1 FROM [commerce].[ProductVendor] WHERE [Id] = 1)
BEGIN
    SET IDENTITY_INSERT [commerce].[ProductVendor] ON;

    INSERT INTO [commerce].[ProductVendor] ([Id], [VendorName], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES (1, N'MyTalli', 0, 1, 0, GETUTCDATE());

    SET IDENTITY_INSERT [commerce].[ProductVendor] OFF;
END
GO

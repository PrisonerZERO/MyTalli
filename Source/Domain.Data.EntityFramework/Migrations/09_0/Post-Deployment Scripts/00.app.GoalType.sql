-- Seed GoalType lookup data
IF NOT EXISTS (SELECT 1 FROM [app].[GoalType] WHERE [Name] = N'Monthly Revenue Target')
BEGIN
    INSERT INTO [app].[GoalType] ([Name], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES
        (N'Monthly Revenue Target', 0, 1, 0, GETUTCDATE()),
        (N'Yearly Revenue Target', 0, 1, 0, GETUTCDATE()),
        (N'Platform Monthly Target', 0, 1, 0, GETUTCDATE()),
        (N'Growth Rate Target', 0, 1, 0, GETUTCDATE());
END
GO

-- Seed the MaintenanceMode flag (default: off)
-- The MaintenanceModeService reads this row at startup and on every AdminHealthWorker tick.

IF NOT EXISTS (SELECT 1 FROM [app].[SystemSetting] WHERE [SettingKey] = N'MaintenanceMode')
BEGIN
    INSERT INTO [app].[SystemSetting] ([SettingKey], [SettingValue], [IsDeleted], [IsVisible], [CreateByUserId], [CreatedOnDateTime])
    VALUES (N'MaintenanceMode', N'false', 0, 1, 0, GETUTCDATE());
END
GO

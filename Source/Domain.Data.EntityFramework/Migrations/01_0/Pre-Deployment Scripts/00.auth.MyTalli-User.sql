IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'MyTalli-User')
BEGIN
    BEGIN TRY
        CREATE USER [MyTalli-User] FOR LOGIN [MyTalli-User];
        ALTER ROLE db_datareader ADD MEMBER [MyTalli-User];
        ALTER ROLE db_datawriter ADD MEMBER [MyTalli-User];
        GRANT EXECUTE TO [MyTalli-User];
    END TRY
    BEGIN CATCH
        PRINT 'Warning: Could not create user MyTalli-User. Ensure the login exists on the server.';
    END CATCH
END

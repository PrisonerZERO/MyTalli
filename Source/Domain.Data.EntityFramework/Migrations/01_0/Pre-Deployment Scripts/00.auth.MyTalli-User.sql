IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'MyTalli-User')
    AND EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'MyTalli-User')
BEGIN
    CREATE USER [MyTalli-User] FOR LOGIN [MyTalli-User];
    ALTER ROLE db_datareader ADD MEMBER [MyTalli-User];
    ALTER ROLE db_datawriter ADD MEMBER [MyTalli-User];
    GRANT EXECUTE TO [MyTalli-User];
END

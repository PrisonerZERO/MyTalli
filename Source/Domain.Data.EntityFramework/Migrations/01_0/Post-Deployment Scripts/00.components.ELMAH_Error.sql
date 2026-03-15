IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'components')
    EXEC('CREATE SCHEMA [components]')
GO

IF NOT EXISTS (
    SELECT 1
    FROM   INFORMATION_SCHEMA.TABLES
    WHERE  TABLE_SCHEMA = 'components'
    AND    TABLE_NAME = 'ELMAH_Error'
)
BEGIN
    CREATE TABLE [components].[ELMAH_Error]
    (
        [ErrorId]     UNIQUEIDENTIFIER NOT NULL,
        [Application] NVARCHAR(60)  NOT NULL,
        [Host]        NVARCHAR(50)  NOT NULL,
        [Type]        NVARCHAR(100) NOT NULL,
        [Source]      NVARCHAR(60)  NOT NULL,
        [Message]     NVARCHAR(MAX) NOT NULL,
        [User]        NVARCHAR(50)  NOT NULL,
        [StatusCode]  INT NOT NULL,
        [TimeUtc]     DATETIME NOT NULL,
        [Sequence]    INT IDENTITY (1, 1) NOT NULL,
        [AllXml]      NVARCHAR(MAX) NOT NULL
    )

    ALTER TABLE [components].[ELMAH_Error] WITH NOCHECK ADD
        CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY]

    ALTER TABLE [components].[ELMAH_Error] ADD
        CONSTRAINT [DF_ELMAH_Error_ErrorId] DEFAULT (NEWID()) FOR [ErrorId]

    CREATE NONCLUSTERED INDEX [IX_ELMAH_Error_App_Time_Seq] ON [components].[ELMAH_Error]
    (
        [Application]   ASC,
        [TimeUtc]       DESC,
        [Sequence]      DESC
    )
    ON [PRIMARY]
END

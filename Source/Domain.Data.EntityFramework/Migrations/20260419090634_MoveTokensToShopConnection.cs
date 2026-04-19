namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class MoveTokensToShopConnection : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "13_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        // 1. Add the four new token columns to ShopConnection.
        migrationBuilder.AddColumn<string>(
            name: "AccessToken",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "")
            .Annotation("Relational:ColumnOrder", 3);

        migrationBuilder.AddColumn<string>(
            name: "PlatformAccountId",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "")
            .Annotation("Relational:ColumnOrder", 10);

        migrationBuilder.AddColumn<string>(
            name: "RefreshToken",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(max)",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 12);

        migrationBuilder.AddColumn<DateTime>(
            name: "TokenExpiryDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 15);

        // 2. Copy tokens from PlatformConnection down to every ShopConnection
        //    under that PlatformConnection. Single-shop platforms (Stripe, Gumroad,
        //    PayPal, Shopify) still get the tokens on their sole ShopConnection row.
        migrationBuilder.Sql(@"
            UPDATE sc
            SET sc.AccessToken = pc.AccessToken,
                sc.PlatformAccountId = pc.PlatformAccountId,
                sc.RefreshToken = pc.RefreshToken,
                sc.TokenExpiryDateTime = pc.TokenExpiryDateTime
            FROM [app].[ShopConnection] sc
            INNER JOIN [app].[PlatformConnection] pc ON pc.Id = sc.PlatformConnectionId;
        ");

        // 3. Drop the old token columns from PlatformConnection.
        migrationBuilder.DropColumn(
            name: "AccessToken",
            schema: "app",
            table: "PlatformConnection");

        migrationBuilder.DropColumn(
            name: "PlatformAccountId",
            schema: "app",
            table: "PlatformConnection");

        migrationBuilder.DropColumn(
            name: "RefreshToken",
            schema: "app",
            table: "PlatformConnection");

        migrationBuilder.DropColumn(
            name: "TokenExpiryDateTime",
            schema: "app",
            table: "PlatformConnection");

        // 4. Column-order refresh (model-only; physical order unchanged in SQL Server).
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 21)
            .OldAnnotation("Relational:ColumnOrder", 17);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "ShopConnection",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 20)
            .OldAnnotation("Relational:ColumnOrder", 16);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<string>(
            name: "ShopName",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255)
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformShopId",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255)
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "NextSyncDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<DateTime>(
            name: "LastSyncDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<string>(
            name: "LastErrorMessage",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(2000)",
            oldMaxLength: 2000,
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 17)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<bool>(
            name: "IsEnabled",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            defaultValue: true,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: true)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<bool>(
            name: "IsActive",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            defaultValue: true,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 19)
            .OldAnnotation("Relational:ColumnOrder", 15);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "ShopConnection",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 18)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<int>(
            name: "ConsecutiveFailures",
            schema: "app",
            table: "ShopConnection",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 0)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "PlatformConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "PlatformConnection",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "PlatformConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "PlatformConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "PlatformConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "PlatformConnection",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<string>(
            name: "ConnectionStatus",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        // 1. Restore the four token columns on PlatformConnection.
        migrationBuilder.AddColumn<string>(
            name: "AccessToken",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "")
            .Annotation("Relational:ColumnOrder", 2);

        migrationBuilder.AddColumn<string>(
            name: "PlatformAccountId",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "")
            .Annotation("Relational:ColumnOrder", 5);

        migrationBuilder.AddColumn<string>(
            name: "RefreshToken",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(max)",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 6);

        migrationBuilder.AddColumn<DateTime>(
            name: "TokenExpiryDateTime",
            schema: "app",
            table: "PlatformConnection",
            type: "datetime2",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 7);

        // 2. Copy the first shop's tokens back up to the PlatformConnection
        //    (best-effort — if multiple shops exist with different tokens,
        //    only one set round-trips. Dev-only rollback path.)
        migrationBuilder.Sql(@"
            UPDATE pc
            SET pc.AccessToken = sc.AccessToken,
                pc.PlatformAccountId = sc.PlatformAccountId,
                pc.RefreshToken = sc.RefreshToken,
                pc.TokenExpiryDateTime = sc.TokenExpiryDateTime
            FROM [app].[PlatformConnection] pc
            INNER JOIN (
                SELECT PlatformConnectionId, AccessToken, PlatformAccountId, RefreshToken, TokenExpiryDateTime,
                       ROW_NUMBER() OVER (PARTITION BY PlatformConnectionId ORDER BY Id) AS rn
                FROM [app].[ShopConnection]
            ) sc ON sc.PlatformConnectionId = pc.Id AND sc.rn = 1;
        ");

        // 3. Drop the token columns from ShopConnection.
        migrationBuilder.DropColumn(
            name: "AccessToken",
            schema: "app",
            table: "ShopConnection");

        migrationBuilder.DropColumn(
            name: "PlatformAccountId",
            schema: "app",
            table: "ShopConnection");

        migrationBuilder.DropColumn(
            name: "RefreshToken",
            schema: "app",
            table: "ShopConnection");

        migrationBuilder.DropColumn(
            name: "TokenExpiryDateTime",
            schema: "app",
            table: "ShopConnection");

        // 4. Column-order rollback.
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 17)
            .OldAnnotation("Relational:ColumnOrder", 21);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "ShopConnection",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 20);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<string>(
            name: "ShopName",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255)
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformShopId",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<DateTime>(
            name: "NextSyncDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "LastSyncDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "LastErrorMessage",
            schema: "app",
            table: "ShopConnection",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(2000)",
            oldMaxLength: 2000,
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 17);

        migrationBuilder.AlterColumn<bool>(
            name: "IsEnabled",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            defaultValue: true,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 16);

        migrationBuilder.AlterColumn<bool>(
            name: "IsActive",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            defaultValue: true,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: true)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 15)
            .OldAnnotation("Relational:ColumnOrder", 19);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "ShopConnection",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 18);

        migrationBuilder.AlterColumn<int>(
            name: "ConsecutiveFailures",
            schema: "app",
            table: "ShopConnection",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 0)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "PlatformConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "PlatformConnection",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "PlatformConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "PlatformConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "PlatformConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "PlatformConnection",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<string>(
            name: "ConnectionStatus",
            schema: "app",
            table: "PlatformConnection",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);
    }

    #endregion
}

namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddRefreshTokenExpiryToShopConnection : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "14_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 22)
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
            .Annotation("Relational:ColumnOrder", 21)
            .OldAnnotation("Relational:ColumnOrder", 20);

        migrationBuilder.AlterColumn<DateTime>(
            name: "TokenExpiryDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 15);

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
            .Annotation("Relational:ColumnOrder", 15)
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
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 18)
            .OldAnnotation("Relational:ColumnOrder", 17);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 17)
            .OldAnnotation("Relational:ColumnOrder", 16);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 20)
            .OldAnnotation("Relational:ColumnOrder", 19);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "ShopConnection",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 19)
            .OldAnnotation("Relational:ColumnOrder", 18);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefreshTokenExpiryDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 13);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RefreshTokenExpiryDateTime",
            schema: "app",
            table: "ShopConnection");

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
            .OldAnnotation("Relational:ColumnOrder", 22);

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
            .OldAnnotation("Relational:ColumnOrder", 21);

        migrationBuilder.AlterColumn<DateTime>(
            name: "TokenExpiryDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 15)
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
            .OldAnnotation("Relational:ColumnOrder", 15);

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
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 17)
            .OldAnnotation("Relational:ColumnOrder", 18);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "ShopConnection",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 17);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "ShopConnection",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 19)
            .OldAnnotation("Relational:ColumnOrder", 20);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "ShopConnection",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 18)
            .OldAnnotation("Relational:ColumnOrder", 19);
    }

    #endregion
}

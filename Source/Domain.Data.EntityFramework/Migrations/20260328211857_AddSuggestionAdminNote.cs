namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddSuggestionAdminNote : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "07_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Suggestion",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Suggestion",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Suggestion",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Suggestion",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(2000)",
            oldMaxLength: 2000)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Suggestion",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Suggestion",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AddColumn<string>(
            name: "AdminNote",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true)
            .Annotation("Relational:ColumnOrder", 2);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AdminNote",
            schema: "app",
            table: "Suggestion");

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Suggestion",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Suggestion",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Suggestion",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Suggestion",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(2000)",
            oldMaxLength: 2000)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Suggestion",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Suggestion",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            schema: "app",
            table: "Suggestion",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);
    }

    #endregion
}

namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddSubscriptionExpirationTracking : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "19_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "commerce",
            table: "Subscription",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 15)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "commerce",
            table: "Subscription",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<DateTime>(
            name: "StartDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<DateTime>(
            name: "RenewalDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "commerce",
            table: "Subscription",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "commerce",
            table: "Subscription",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "commerce",
            table: "Subscription",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AddColumn<DateTime>(
            name: "ExpirationAcknowledgedAt",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 6);

        migrationBuilder.AddColumn<DateTime>(
            name: "ExpirationEmailSentAt",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 7);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ExpirationAcknowledgedAt",
            schema: "commerce",
            table: "Subscription");

        migrationBuilder.DropColumn(
            name: "ExpirationEmailSentAt",
            schema: "commerce",
            table: "Subscription");

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 16);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "commerce",
            table: "Subscription",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 15);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "commerce",
            table: "Subscription",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<DateTime>(
            name: "StartDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "RenewalDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "commerce",
            table: "Subscription",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "commerce",
            table: "Subscription",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "commerce",
            table: "Subscription",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 13);
    }


    #endregion
}

namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class MakeRevenuePlatformTransactionIdNullable : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "10_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "PlatformTransactionId",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "PlatformTransactionId",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true);
    }

    #endregion
}

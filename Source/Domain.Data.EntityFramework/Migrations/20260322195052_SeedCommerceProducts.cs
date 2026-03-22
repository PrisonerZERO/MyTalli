namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class SeedCommerceProducts : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "04_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "CancelledDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "CancelledDate",
            schema: "commerce",
            table: "Subscription",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);
    }


    #endregion
}

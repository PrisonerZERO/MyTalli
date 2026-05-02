namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddHeartbeatAndSystemSettings : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "16_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Heartbeat",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ExpectedIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                HeartbeatSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LastTickAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Heartbeat", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SystemSetting",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SystemSetting", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Heartbeat_LastTickAt",
            schema: "app",
            table: "Heartbeat",
            column: "LastTickAt");

        migrationBuilder.CreateIndex(
            name: "UQ_Heartbeat_HeartbeatSource",
            schema: "app",
            table: "Heartbeat",
            column: "HeartbeatSource",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_SystemSetting_SettingKey",
            schema: "app",
            table: "SystemSetting",
            column: "SettingKey",
            unique: true);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Heartbeat",
            schema: "app");

        migrationBuilder.DropTable(
            name: "SystemSetting",
            schema: "app");
    }


    #endregion
}

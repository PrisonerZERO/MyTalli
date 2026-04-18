namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddShopConnectionsAndDropSyncQueue : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "11_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SyncQueue",
            schema: "app");

        migrationBuilder.CreateTable(
            name: "ShopConnection",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PlatformConnectionId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                ConsecutiveFailures = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                LastSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                NextSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                PlatformShopId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ShopName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShopConnection", x => x.Id);
                table.ForeignKey(
                    name: "FK_ShopConnection_PlatformConnection",
                    column: x => x.PlatformConnectionId,
                    principalSchema: "app",
                    principalTable: "PlatformConnection",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ShopConnection_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ShopConnectionEtsy",
            schema: "app",
            columns: table => new
            {
                ShopConnectionId = table.Column<long>(type: "bigint", nullable: false),
                CountryCode = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                IsVacationMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                ShopCurrency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                ShopUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShopConnectionEtsy", x => x.ShopConnectionId);
                table.ForeignKey(
                    name: "FK_ShopConnectionEtsy_ShopConnection",
                    column: x => x.ShopConnectionId,
                    principalSchema: "app",
                    principalTable: "ShopConnection",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ShopConnection_NextSyncDateTime_Status",
            schema: "app",
            table: "ShopConnection",
            columns: new[] { "NextSyncDateTime", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_ShopConnection_PlatformConnectionId",
            schema: "app",
            table: "ShopConnection",
            column: "PlatformConnectionId");

        migrationBuilder.CreateIndex(
            name: "IX_ShopConnection_UserId",
            schema: "app",
            table: "ShopConnection",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "UQ_ShopConnection_PlatformConnectionId_PlatformShopId",
            schema: "app",
            table: "ShopConnection",
            columns: new[] { "PlatformConnectionId", "PlatformShopId" },
            unique: true);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ShopConnectionEtsy",
            schema: "app");

        migrationBuilder.DropTable(
            name: "ShopConnection",
            schema: "app");

        migrationBuilder.CreateTable(
            name: "SyncQueue",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                ConsecutiveFailures = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                LastErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                LastSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                NextSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SyncQueue", x => x.Id);
                table.ForeignKey(
                    name: "FK_SyncQueue_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SyncQueue_NextSyncDateTime_Status",
            schema: "app",
            table: "SyncQueue",
            columns: new[] { "NextSyncDateTime", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_SyncQueue_UserId",
            schema: "app",
            table: "SyncQueue",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "UQ_SyncQueue_UserId_Platform",
            schema: "app",
            table: "SyncQueue",
            columns: new[] { "UserId", "Platform" },
            unique: true);
    }

    #endregion
}

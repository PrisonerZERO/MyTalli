namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddPlatformAndGoalTables : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "09_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GoalType",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GoalType", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PlatformConnection",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ConnectionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                PlatformAccountId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TokenExpiryDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformConnection", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlatformConnection_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RevenueEtsy",
            schema: "app",
            columns: table => new
            {
                RevenueId = table.Column<long>(type: "bigint", nullable: false),
                AdjustedFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                AdjustedGross = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                AdjustedNet = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                ListingId = table.Column<long>(type: "bigint", nullable: false),
                ReceiptId = table.Column<long>(type: "bigint", nullable: false),
                ShopCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RevenueEtsy", x => x.RevenueId);
                table.ForeignKey(
                    name: "FK_RevenueEtsy_Revenue",
                    column: x => x.RevenueId,
                    principalSchema: "app",
                    principalTable: "Revenue",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RevenueGumroad",
            schema: "app",
            columns: table => new
            {
                RevenueId = table.Column<long>(type: "bigint", nullable: false),
                DiscoverFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                LicenseKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                SaleId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RevenueGumroad", x => x.RevenueId);
                table.ForeignKey(
                    name: "FK_RevenueGumroad_Revenue",
                    column: x => x.RevenueId,
                    principalSchema: "app",
                    principalTable: "Revenue",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RevenueStripe",
            schema: "app",
            columns: table => new
            {
                RevenueId = table.Column<long>(type: "bigint", nullable: false),
                BalanceTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RiskScore = table.Column<int>(type: "int", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RevenueStripe", x => x.RevenueId);
                table.ForeignKey(
                    name: "FK_RevenueStripe_Revenue",
                    column: x => x.RevenueId,
                    principalSchema: "app",
                    principalTable: "Revenue",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Goal",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GoalTypeId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Goal", x => x.Id);
                table.ForeignKey(
                    name: "FK_Goal_GoalType",
                    column: x => x.GoalTypeId,
                    principalSchema: "app",
                    principalTable: "GoalType",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Goal_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Goal_GoalTypeId",
            schema: "app",
            table: "Goal",
            column: "GoalTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Goal_UserId",
            schema: "app",
            table: "Goal",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_PlatformConnection_UserId",
            schema: "app",
            table: "PlatformConnection",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "UQ_PlatformConnection_UserId_Platform",
            schema: "app",
            table: "PlatformConnection",
            columns: new[] { "UserId", "Platform" },
            unique: true);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Goal", schema: "app");
        migrationBuilder.DropTable(name: "PlatformConnection", schema: "app");
        migrationBuilder.DropTable(name: "RevenueEtsy", schema: "app");
        migrationBuilder.DropTable(name: "RevenueGumroad", schema: "app");
        migrationBuilder.DropTable(name: "RevenueStripe", schema: "app");
        migrationBuilder.DropTable(name: "GoalType", schema: "app");
    }

    #endregion
}

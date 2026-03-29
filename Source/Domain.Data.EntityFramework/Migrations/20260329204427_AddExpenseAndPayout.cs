namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddExpenseAndPayout : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "10_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Expense",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                PlatformTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Expense", x => x.Id);
                table.ForeignKey(
                    name: "FK_Expense_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Payout",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                ExpectedArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                PayoutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                PlatformPayoutId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
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
                table.PrimaryKey("PK_Payout", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payout_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Expense_Platform_ExpenseDate",
            schema: "app",
            table: "Expense",
            columns: new[] { "Platform", "ExpenseDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Expense_UserId",
            schema: "app",
            table: "Expense",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Payout_Platform_PayoutDate",
            schema: "app",
            table: "Payout",
            columns: new[] { "Platform", "PayoutDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Payout_UserId",
            schema: "app",
            table: "Payout",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "UQ_Payout_PlatformPayoutId",
            schema: "app",
            table: "Payout",
            column: "PlatformPayoutId",
            unique: true);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Expense", schema: "app");
        migrationBuilder.DropTable(name: "Payout", schema: "app");
    }

    #endregion
}

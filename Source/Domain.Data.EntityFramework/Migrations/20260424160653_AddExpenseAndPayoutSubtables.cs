namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddExpenseAndPayoutSubtables : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "15_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExpenseEtsy",
            schema: "app",
            columns: table => new
            {
                ExpenseId = table.Column<long>(type: "bigint", nullable: false),
                AdCampaignId = table.Column<long>(type: "bigint", nullable: true),
                LedgerEntryId = table.Column<long>(type: "bigint", nullable: false),
                ListingId = table.Column<long>(type: "bigint", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpenseEtsy", x => x.ExpenseId);
                table.ForeignKey(
                    name: "FK_ExpenseEtsy_Expense",
                    column: x => x.ExpenseId,
                    principalSchema: "app",
                    principalTable: "Expense",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExpenseGumroad",
            schema: "app",
            columns: table => new
            {
                ExpenseId = table.Column<long>(type: "bigint", nullable: false),
                SubscriptionPlanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpenseGumroad", x => x.ExpenseId);
                table.ForeignKey(
                    name: "FK_ExpenseGumroad_Expense",
                    column: x => x.ExpenseId,
                    principalSchema: "app",
                    principalTable: "Expense",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExpenseManual",
            schema: "app",
            columns: table => new
            {
                ExpenseId = table.Column<long>(type: "bigint", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpenseManual", x => x.ExpenseId);
                table.ForeignKey(
                    name: "FK_ExpenseManual_Expense",
                    column: x => x.ExpenseId,
                    principalSchema: "app",
                    principalTable: "Expense",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExpenseStripe",
            schema: "app",
            columns: table => new
            {
                ExpenseId = table.Column<long>(type: "bigint", nullable: false),
                BalanceTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                FeeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpenseStripe", x => x.ExpenseId);
                table.ForeignKey(
                    name: "FK_ExpenseStripe_Expense",
                    column: x => x.ExpenseId,
                    principalSchema: "app",
                    principalTable: "Expense",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PayoutEtsy",
            schema: "app",
            columns: table => new
            {
                PayoutId = table.Column<long>(type: "bigint", nullable: false),
                LedgerEntryId = table.Column<long>(type: "bigint", nullable: false),
                ShopCurrency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayoutEtsy", x => x.PayoutId);
                table.ForeignKey(
                    name: "FK_PayoutEtsy_Payout",
                    column: x => x.PayoutId,
                    principalSchema: "app",
                    principalTable: "Payout",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PayoutGumroad",
            schema: "app",
            columns: table => new
            {
                PayoutId = table.Column<long>(type: "bigint", nullable: false),
                PayoutMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayoutGumroad", x => x.PayoutId);
                table.ForeignKey(
                    name: "FK_PayoutGumroad_Payout",
                    column: x => x.PayoutId,
                    principalSchema: "app",
                    principalTable: "Payout",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PayoutManual",
            schema: "app",
            columns: table => new
            {
                PayoutId = table.Column<long>(type: "bigint", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayoutManual", x => x.PayoutId);
                table.ForeignKey(
                    name: "FK_PayoutManual_Payout",
                    column: x => x.PayoutId,
                    principalSchema: "app",
                    principalTable: "Payout",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PayoutStripe",
            schema: "app",
            columns: table => new
            {
                PayoutId = table.Column<long>(type: "bigint", nullable: false),
                PayoutMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                StatementDescriptor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                StripePayoutId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayoutStripe", x => x.PayoutId);
                table.ForeignKey(
                    name: "FK_PayoutStripe_Payout",
                    column: x => x.PayoutId,
                    principalSchema: "app",
                    principalTable: "Payout",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ExpenseEtsy",    schema: "app");
        migrationBuilder.DropTable(name: "ExpenseGumroad", schema: "app");
        migrationBuilder.DropTable(name: "ExpenseManual",  schema: "app");
        migrationBuilder.DropTable(name: "ExpenseStripe",  schema: "app");
        migrationBuilder.DropTable(name: "PayoutEtsy",     schema: "app");
        migrationBuilder.DropTable(name: "PayoutGumroad",  schema: "app");
        migrationBuilder.DropTable(name: "PayoutManual",   schema: "app");
        migrationBuilder.DropTable(name: "PayoutStripe",   schema: "app");
    }

    #endregion
}

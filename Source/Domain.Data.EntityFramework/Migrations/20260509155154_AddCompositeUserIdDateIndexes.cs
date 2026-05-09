namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddCompositeUserIdDateIndexes : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "17_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Revenue_UserId",
            schema: "app",
            table: "Revenue");

        migrationBuilder.DropIndex(
            name: "IX_Payout_UserId",
            schema: "app",
            table: "Payout");

        migrationBuilder.DropIndex(
            name: "IX_Expense_UserId",
            schema: "app",
            table: "Expense");

        migrationBuilder.CreateIndex(
            name: "IX_Revenue_UserId_TransactionDate",
            schema: "app",
            table: "Revenue",
            columns: new[] { "UserId", "TransactionDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Payout_UserId_PayoutDate",
            schema: "app",
            table: "Payout",
            columns: new[] { "UserId", "PayoutDate" });

        migrationBuilder.CreateIndex(
            name: "IX_Expense_UserId_ExpenseDate",
            schema: "app",
            table: "Expense",
            columns: new[] { "UserId", "ExpenseDate" });
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Revenue_UserId_TransactionDate",
            schema: "app",
            table: "Revenue");

        migrationBuilder.DropIndex(
            name: "IX_Payout_UserId_PayoutDate",
            schema: "app",
            table: "Payout");

        migrationBuilder.DropIndex(
            name: "IX_Expense_UserId_ExpenseDate",
            schema: "app",
            table: "Expense");

        migrationBuilder.CreateIndex(
            name: "IX_Revenue_UserId",
            schema: "app",
            table: "Revenue",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Payout_UserId",
            schema: "app",
            table: "Payout",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_UserId",
            schema: "app",
            table: "Expense",
            column: "UserId");
    }


    #endregion
}

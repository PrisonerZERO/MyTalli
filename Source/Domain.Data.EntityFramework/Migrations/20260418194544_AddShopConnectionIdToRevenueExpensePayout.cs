namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddShopConnectionIdToRevenueExpensePayout : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "12_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<long>(
            name: "UserId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 1);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Revenue",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 18)
            .OldAnnotation("Relational:ColumnOrder", 17);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 17)
            .OldAnnotation("Relational:ColumnOrder", 16);

        migrationBuilder.AlterColumn<DateTime>(
            name: "TransactionDate",
            schema: "app",
            table: "Revenue",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformTransactionId",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<decimal>(
            name: "NetAmount",
            schema: "app",
            table: "Revenue",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<bool>(
            name: "IsRefunded",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDisputed",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<decimal>(
            name: "GrossAmount",
            schema: "app",
            table: "Revenue",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<decimal>(
            name: "FeeAmount",
            schema: "app",
            table: "Revenue",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<string>(
            name: "Currency",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Revenue",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 15);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 15)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AddColumn<long>(
            name: "ShopConnectionId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 1);

        migrationBuilder.AlterColumn<long>(
            name: "UserId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 1);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 15)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "app",
            table: "Payout",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformPayoutId",
            schema: "app",
            table: "Payout",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "Payout",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<DateTime>(
            name: "PayoutDate",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Payout",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Payout",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpectedArrivalDate",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<string>(
            name: "Currency",
            schema: "app",
            table: "Payout",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<decimal>(
            name: "Amount",
            schema: "app",
            table: "Payout",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)")
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AddColumn<long>(
            name: "ShopConnectionId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 1);

        migrationBuilder.AlterColumn<long>(
            name: "UserId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 1);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Expense",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 15)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformTransactionId",
            schema: "app",
            table: "Expense",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "Expense",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Expense",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Expense",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpenseDate",
            schema: "app",
            table: "Expense",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "app",
            table: "Expense",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<string>(
            name: "Currency",
            schema: "app",
            table: "Expense",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Expense",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            schema: "app",
            table: "Expense",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<decimal>(
            name: "Amount",
            schema: "app",
            table: "Expense",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)")
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AddColumn<long>(
            name: "ShopConnectionId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: true)
            .Annotation("Relational:ColumnOrder", 1);

        migrationBuilder.CreateIndex(
            name: "IX_Revenue_ShopConnectionId",
            schema: "app",
            table: "Revenue",
            column: "ShopConnectionId");

        migrationBuilder.CreateIndex(
            name: "IX_Payout_ShopConnectionId",
            schema: "app",
            table: "Payout",
            column: "ShopConnectionId");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_ShopConnectionId",
            schema: "app",
            table: "Expense",
            column: "ShopConnectionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Expense_ShopConnection",
            schema: "app",
            table: "Expense",
            column: "ShopConnectionId",
            principalSchema: "app",
            principalTable: "ShopConnection",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Payout_ShopConnection",
            schema: "app",
            table: "Payout",
            column: "ShopConnectionId",
            principalSchema: "app",
            principalTable: "ShopConnection",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Revenue_ShopConnection",
            schema: "app",
            table: "Revenue",
            column: "ShopConnectionId",
            principalSchema: "app",
            principalTable: "ShopConnection",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Expense_ShopConnection",
            schema: "app",
            table: "Expense");

        migrationBuilder.DropForeignKey(
            name: "FK_Payout_ShopConnection",
            schema: "app",
            table: "Payout");

        migrationBuilder.DropForeignKey(
            name: "FK_Revenue_ShopConnection",
            schema: "app",
            table: "Revenue");

        migrationBuilder.DropIndex(
            name: "IX_Revenue_ShopConnectionId",
            schema: "app",
            table: "Revenue");

        migrationBuilder.DropIndex(
            name: "IX_Payout_ShopConnectionId",
            schema: "app",
            table: "Payout");

        migrationBuilder.DropIndex(
            name: "IX_Expense_ShopConnectionId",
            schema: "app",
            table: "Expense");

        migrationBuilder.DropColumn(
            name: "ShopConnectionId",
            schema: "app",
            table: "Revenue");

        migrationBuilder.DropColumn(
            name: "ShopConnectionId",
            schema: "app",
            table: "Payout");

        migrationBuilder.DropColumn(
            name: "ShopConnectionId",
            schema: "app",
            table: "Expense");

        migrationBuilder.AlterColumn<long>(
            name: "UserId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 1)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Revenue",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 17)
            .OldAnnotation("Relational:ColumnOrder", 18);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 16)
            .OldAnnotation("Relational:ColumnOrder", 17);

        migrationBuilder.AlterColumn<DateTime>(
            name: "TransactionDate",
            schema: "app",
            table: "Revenue",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformTransactionId",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<decimal>(
            name: "NetAmount",
            schema: "app",
            table: "Revenue",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<bool>(
            name: "IsRefunded",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDisputed",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Revenue",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<decimal>(
            name: "GrossAmount",
            schema: "app",
            table: "Revenue",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<decimal>(
            name: "FeeAmount",
            schema: "app",
            table: "Revenue",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<string>(
            name: "Currency",
            schema: "app",
            table: "Revenue",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3)
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Revenue",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 15)
            .OldAnnotation("Relational:ColumnOrder", 16);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Revenue",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 15);

        migrationBuilder.AlterColumn<long>(
            name: "UserId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 1)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 15);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "app",
            table: "Payout",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformPayoutId",
            schema: "app",
            table: "Payout",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255)
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "Payout",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<DateTime>(
            name: "PayoutDate",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Payout",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Payout",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpectedArrivalDate",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<string>(
            name: "Currency",
            schema: "app",
            table: "Payout",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Payout",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Payout",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<decimal>(
            name: "Amount",
            schema: "app",
            table: "Payout",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);

        migrationBuilder.AlterColumn<long>(
            name: "UserId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 1)
            .OldAnnotation("Relational:ColumnOrder", 2);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnDate",
            schema: "app",
            table: "Expense",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 14)
            .OldAnnotation("Relational:ColumnOrder", 15);

        migrationBuilder.AlterColumn<long>(
            name: "UpdatedByUserId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 13)
            .OldAnnotation("Relational:ColumnOrder", 14);

        migrationBuilder.AlterColumn<string>(
            name: "PlatformTransactionId",
            schema: "app",
            table: "Expense",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 8)
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<string>(
            name: "Platform",
            schema: "app",
            table: "Expense",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 7)
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<bool>(
            name: "IsVisible",
            schema: "app",
            table: "Expense",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 10)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            schema: "app",
            table: "Expense",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit")
            .Annotation("Relational:ColumnOrder", 9)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpenseDate",
            schema: "app",
            table: "Expense",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 6)
            .OldAnnotation("Relational:ColumnOrder", 7);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "app",
            table: "Expense",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500)
            .Annotation("Relational:ColumnOrder", 5)
            .OldAnnotation("Relational:ColumnOrder", 6);

        migrationBuilder.AlterColumn<string>(
            name: "Currency",
            schema: "app",
            table: "Expense",
            type: "nvarchar(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(3)",
            oldMaxLength: 3)
            .Annotation("Relational:ColumnOrder", 4)
            .OldAnnotation("Relational:ColumnOrder", 5);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnDateTime",
            schema: "app",
            table: "Expense",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2")
            .Annotation("Relational:ColumnOrder", 12)
            .OldAnnotation("Relational:ColumnOrder", 13);

        migrationBuilder.AlterColumn<long>(
            name: "CreateByUserId",
            schema: "app",
            table: "Expense",
            type: "bigint",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "bigint")
            .Annotation("Relational:ColumnOrder", 11)
            .OldAnnotation("Relational:ColumnOrder", 12);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            schema: "app",
            table: "Expense",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50)
            .Annotation("Relational:ColumnOrder", 3)
            .OldAnnotation("Relational:ColumnOrder", 4);

        migrationBuilder.AlterColumn<decimal>(
            name: "Amount",
            schema: "app",
            table: "Expense",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)")
            .Annotation("Relational:ColumnOrder", 2)
            .OldAnnotation("Relational:ColumnOrder", 3);
    }

    #endregion
}

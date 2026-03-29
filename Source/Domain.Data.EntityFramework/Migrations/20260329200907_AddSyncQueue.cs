namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddSyncQueue : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "08_0";

    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
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

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SyncQueue",
            schema: "app");
    }

    #endregion
}

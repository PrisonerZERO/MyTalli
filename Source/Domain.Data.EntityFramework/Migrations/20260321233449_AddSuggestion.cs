namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddSuggestion : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "03_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Suggestion",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Suggestion", x => x.Id);
                table.ForeignKey(
                    name: "FK_Suggestion_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SuggestionVote",
            schema: "app",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SuggestionId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsVisible = table.Column<bool>(type: "bit", nullable: false),
                CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SuggestionVote", x => x.Id);
                table.ForeignKey(
                    name: "FK_SuggestionVote_Suggestion",
                    column: x => x.SuggestionId,
                    principalSchema: "app",
                    principalTable: "Suggestion",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SuggestionVote_User",
                    column: x => x.UserId,
                    principalSchema: "auth",
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Suggestion_UserId",
            schema: "app",
            table: "Suggestion",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_SuggestionVote_SuggestionId",
            schema: "app",
            table: "SuggestionVote",
            column: "SuggestionId");

        migrationBuilder.CreateIndex(
            name: "IX_SuggestionVote_UserId",
            schema: "app",
            table: "SuggestionVote",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "UQ_SuggestionVote_SuggestionId_UserId",
            schema: "app",
            table: "SuggestionVote",
            columns: new[] { "SuggestionId", "UserId" },
            unique: true);
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SuggestionVote",
            schema: "app");

        migrationBuilder.DropTable(
            name: "Suggestion",
            schema: "app");
    }


    #endregion
}

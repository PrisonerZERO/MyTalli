namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddMilestone : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "02_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "Milestone",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MilestoneGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_Milestone", x => x.Id);
                });
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DropTable(
                name: "Milestone",
                schema: "app");
    }


    #endregion
}

namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <summary>Migration</summary>
public partial class AddDataProtectionKey : DbMigrationBase
{
    #region <Properties>

    protected override string MigrationFolder => "18_0";


    #endregion

    #region <Methods>

    protected override void UpTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "components");

        migrationBuilder.CreateTable(
            name: "DataProtectionKey",
            schema: "components",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataProtectionKey", x => x.Id);
            });
    }

    protected override void DownTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DataProtectionKey",
            schema: "components");
    }


    #endregion
}

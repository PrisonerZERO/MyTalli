namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>Migration base</summary>
public abstract class TalliMigrationBase : Migration
{
    #region <Variables>

    private static readonly string[] PreTableFolders =
    [
        "Pre-Deployment Scripts"
    ];

    private static readonly string[] PostTableFolders =
    [
        "Post-Deployment Scripts",
        "Functions",
        "Views",
        "Stored Procedures",
        "Triggers",
        "Assemblies"
    ];

    #endregion

    #region <Properties>

    protected abstract string MigrationFolder { get; }

    #endregion

    #region <Methods>

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var subFolder in PreTableFolders)
            ExecuteEmbeddedSqlScripts(migrationBuilder, MigrationFolder, subFolder);

        UpTables(migrationBuilder);

        foreach (var subFolder in PostTableFolders)
            ExecuteEmbeddedSqlScripts(migrationBuilder, MigrationFolder, subFolder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        DownTables(migrationBuilder);
    }

    protected abstract void UpTables(MigrationBuilder migrationBuilder);

    protected abstract void DownTables(MigrationBuilder migrationBuilder);

    private void ExecuteEmbeddedSqlScripts(MigrationBuilder migrationBuilder, string migrationFolder, string subFolder)
    {
        var type = GetType();
        var resourceFolder = char.IsDigit(migrationFolder[0]) ? $"_{migrationFolder}" : migrationFolder;
        var path = $"{type.Namespace}.{resourceFolder}.{subFolder.Replace(" ", "_")}.";

        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames()
                                    .Where(r => r.StartsWith(path) && r.EndsWith(".sql"))
                                    .OrderBy(r => r)
                                    .ToList();

        resourceNames.ForEach(resourceName =>
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);

            var sql = reader.ReadToEnd();
            migrationBuilder.Sql(sql);
        });
    }

    #endregion
}

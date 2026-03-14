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

    #region <Methods>

    protected void ExecutePreTableScripts(MigrationBuilder migrationBuilder, string migrationFolder)
    {
        foreach (var subFolder in PreTableFolders)
            ExecuteEmbeddedSqlScripts(migrationBuilder, migrationFolder, subFolder);
    }

    protected void ExecutePostTableScripts(MigrationBuilder migrationBuilder, string migrationFolder)
    {
        foreach (var subFolder in PostTableFolders)
            ExecuteEmbeddedSqlScripts(migrationBuilder, migrationFolder, subFolder);
    }

    private void ExecuteEmbeddedSqlScripts(MigrationBuilder migrationBuilder, string migrationFolder, string subFolder)
    {
        var type = GetType();
        var path = $"{type.Namespace}.{migrationFolder}.{subFolder.Replace(" ", "_")}.";

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

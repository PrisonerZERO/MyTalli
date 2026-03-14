namespace My.Talli.Domain.Data.EntityFramework.Migrations;

using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>Migration base</summary>
public abstract class TalliMigrationBase : Migration
{
    #region <Methods>

    protected virtual void ExecuteEmbeddedSqlScripts(MigrationBuilder migrationBuilder, string parentFolder, string subFolder)
    {
        var type = GetType();
        var path = $"{type.Namespace}.{parentFolder}.{subFolder}.";

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

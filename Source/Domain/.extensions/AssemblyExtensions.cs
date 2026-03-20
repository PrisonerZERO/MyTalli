namespace My.Talli.Domain.Extensions;

using System.Reflection;

/// <summary>Extension</summary>
public static class AssemblyExtensions
{
    #region <Methods>

    public static string GetManifestResourceContent(this Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found in assembly '{assembly.FullName}'.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }


    #endregion
}

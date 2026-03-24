namespace My.Talli.Web.Helpers;

using System.Reflection;

/// <summary>Helper</summary>
public static class LayoutHelper
{
	#region <Variables>

	private static readonly string _versionNumber = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0] ?? "0.0.0";

	#endregion

	#region <Properties>

	public static string CurrentYear => DateTime.Now.Year.ToString();

	public static string VersionNumber => _versionNumber;

	#endregion
}

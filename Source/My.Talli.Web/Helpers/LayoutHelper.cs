namespace My.Talli.Web.Helpers;

using System.Reflection;

/// <summary>Helper</summary>
public static class LayoutHelper
{
	#region <Properties>

	public static string CurrentYear { get { return DateTime.Now.Year.ToString(); } }

	public static string VersionNumber { get { return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0] ?? "0.0.0"; } }

	#endregion
}

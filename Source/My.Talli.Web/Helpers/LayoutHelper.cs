namespace My.Talli.Web.Helpers;

using System.Reflection;

/// <summary>Helper</summary>
public static class LayoutHelper
{
	#region <Properties>

	public static string CurrentYear { get { return DateTime.Now.Year.ToString(); } }

	public static string VersionNumber { get { return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0"; } }

	#endregion
}

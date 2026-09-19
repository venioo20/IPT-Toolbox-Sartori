namespace IPT.Toolbox.App;

internal static class AppPaths
{
    public static string BaseDirectory => AppContext.BaseDirectory;
    public static string ConfigDirectory => Path.Combine(BaseDirectory, "config");
    public static string PackagesFile => Path.Combine(ConfigDirectory, "packages.json");
    public static string ProfilesFile => Path.Combine(ConfigDirectory, "profiles.json");
    public static string LogDirectory => Path.Combine(BaseDirectory, "logs");
}

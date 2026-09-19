namespace IPT.Toolbox.App;

internal static class AppPaths
{
    public static string BaseDirectory => AppContext.BaseDirectory;
    public static string ConfigDirectory => Path.Combine(BaseDirectory, "config");
    public static string PackagesFile => Path.Combine(ConfigDirectory, "packages.json");
    public static string ProfilesFile => Path.Combine(ConfigDirectory, "profiles.json");
    public static string LogDirectory => Path.Combine(BaseDirectory, "logs");
    public static string UserDataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IPT Toolbox Sartori");
    public static string LicenseVaultFile => Path.Combine(UserDataDirectory, "license-vault.dat");
    public static string ProLicenseFile => Path.Combine(UserDataDirectory, "ipt-pro.license");
    public static string CustomPackagesFile => Path.Combine(UserDataDirectory, "custom-packages.json");
}

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IPT.Toolbox.Core.Services;

public static class WingetArguments
{
    public static void ValidateId(string? id)
    {
        if (id is null || id.Length > 200 || !Regex.IsMatch(id, @"\A[A-Za-z0-9][A-Za-z0-9+_-]*(\.[A-Za-z0-9][A-Za-z0-9+_-]*)+\z"))
            throw new ArgumentException("Identifiant WinGet invalide : catalogue refusé.");
    }

    public static ProcessStartInfo Create(string id, bool install)
    {
        ValidateId(id);
        var info = new ProcessStartInfo("winget")
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var arg in new[] { install ? "install" : "list", "--id", id, "--exact", "--source", "winget", "--accept-source-agreements", "--disable-interactivity" })
            info.ArgumentList.Add(arg);
        if (install)
        {
            info.ArgumentList.Add("--silent");
            info.ArgumentList.Add("--accept-package-agreements");
        }
        return info;
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IPT.Toolbox.Core.Models;

namespace IPT.Toolbox.Core.Services;

public sealed class ProLicenseService(string licenseFile)
{
    private const string PublicKeyPem = """
        -----BEGIN PUBLIC KEY-----
        MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEEbNIm41ruMWw2F6VQp9IvNmP5PbV
        46xUpLFiHEbOUOsvzQOfqpFK8v9tiKiLIgcPudlARhtrHdtTopSnGQ9rmg==
        -----END PUBLIC KEY-----
        """;

    public async Task<ProLicenseStatus> LoadStatusAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(licenseFile))
            return new(false, "IPT Toolbox Pro n’est pas activé.");
        try
        {
            return Verify((await File.ReadAllTextAsync(licenseFile, cancellationToken)).Trim());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new(false, "La licence Pro ne peut pas être lue.");
        }
    }

    public ProLicenseStatus Verify(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) return new(false, "Format de clé d’activation incorrect.");
            var payloadBytes = Decode(parts[0]);
            var signature = Decode(parts[1]);
            using var verifier = ECDsa.Create();
            verifier.ImportFromPem(PublicKeyPem);
            if (!verifier.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256))
                return new(false, "Cette clé d’activation n’est pas authentique.");
            var license = JsonSerializer.Deserialize<ProLicense>(payloadBytes);
            if (license is null || string.IsNullOrWhiteSpace(license.LicenseId) || string.IsNullOrWhiteSpace(license.CustomerName))
                return new(false, "Cette clé d’activation est incomplète.");
            if (license.IssuedAtUtc > DateTime.UtcNow.AddMinutes(10))
                return new(false, "La date de cette licence est invalide.");
            if (license.ExpiresAtUtc is { } expires && expires <= DateTime.UtcNow)
                return new(false, $"Licence expirée le {expires.ToLocalTime():dd/MM/yyyy}.", license);
            var validity = license.ExpiresAtUtc is { } end
                ? $"Pro actif jusqu’au {end.ToLocalTime():dd/MM/yyyy}"
                : "Pro définitif actif";
            return new(true, validity, license);
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException or JsonException)
        {
            return new(false, "Clé d’activation invalide.");
        }
    }

    public async Task<ProLicenseStatus> ActivateAsync(string token, CancellationToken cancellationToken = default)
    {
        var status = Verify(token.Trim());
        if (!status.IsValid) return status;
        Directory.CreateDirectory(Path.GetDirectoryName(licenseFile)!);
        var temporary = licenseFile + ".tmp";
        await File.WriteAllTextAsync(temporary, token.Trim(), Encoding.UTF8, cancellationToken);
        File.Move(temporary, licenseFile, true);
        return status;
    }

    private static byte[] Decode(string value)
    {
        value = value.Replace('-', '+').Replace('_', '/');
        value += new string('=', (4 - value.Length % 4) % 4);
        return Convert.FromBase64String(value);
    }
}

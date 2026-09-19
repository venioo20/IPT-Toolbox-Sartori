param(
    [Parameter(Mandatory)] [string] $CustomerName,
    [Parameter(Mandatory)] [ValidateSet('monthly', 'lifetime')] [string] $Plan,
    [Parameter(Mandatory)] [string] $PrivateKeyPath,
    [string] $OutputPath = ".\IPT-Pro-$((Get-Date).ToString('yyyyMMdd-HHmmss')).txt"
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $PrivateKeyPath)) { throw 'Clé privée introuvable.' }

$issued = [DateTime]::UtcNow
$expires = if ($Plan -eq 'monthly') { $issued.AddMonths(1).ToString('O') } else { $null }
$payload = [ordered]@{
    LicenseId = [Guid]::NewGuid().ToString('N')
    CustomerName = $CustomerName.Trim()
    Plan = $Plan
    IssuedAtUtc = $issued.ToString('O')
    ExpiresAtUtc = $expires
    Features = @('license-vault', 'custom-packages')
}
$payloadBytes = [Text.Encoding]::UTF8.GetBytes(($payload | ConvertTo-Json -Compress))
$signer = [Security.Cryptography.ECDsa]::Create()
try {
    $signer.ImportFromPem([IO.File]::ReadAllText($PrivateKeyPath))
    $signature = $signer.SignData($payloadBytes, [Security.Cryptography.HashAlgorithmName]::SHA256)
}
finally { $signer.Dispose() }

function ConvertTo-Base64Url([byte[]] $Bytes) {
    [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

$token = "$(ConvertTo-Base64Url $payloadBytes).$(ConvertTo-Base64Url $signature)"
[IO.File]::WriteAllText((Join-Path (Get-Location) $OutputPath), $token, [Text.Encoding]::UTF8)
Write-Host "Licence $Plan créée pour $CustomerName : $OutputPath"

$ErrorActionPreference = 'Stop'
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnetPath = if ($dotnetCommand) { $dotnetCommand.Source } else { Join-Path $env:ProgramFiles 'dotnet/dotnet.exe' }
& $dotnetPath build (Join-Path $PSScriptRoot 'IPT-Toolbox.sln') -c Release -warnaserror
if ($LASTEXITCODE -ne 0) { throw 'La compilation a échoué.' }

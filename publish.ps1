$ErrorActionPreference = 'Stop'
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnetPath = if ($dotnetCommand) { $dotnetCommand.Source } else { Join-Path $env:ProgramFiles 'dotnet/dotnet.exe' }
$project = Join-Path $PSScriptRoot 'src/IPT.Toolbox.App/IPT.Toolbox.App.csproj'
$out = Join-Path $PSScriptRoot 'publish/win-x64'
& $dotnetPath publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -warnaserror -o $out
if ($LASTEXITCODE -ne 0) { throw 'La publication a échoué.' }
Write-Host "IPT Toolbox Sartori publié dans $out"

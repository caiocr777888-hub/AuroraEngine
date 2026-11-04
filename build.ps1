param([string]$OutputName = 'AuroraEngine.exe')
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$refs = @('System.dll','System.Core.dll','Microsoft.CSharp.dll','System.Runtime.Serialization.dll','System.Xaml.dll','System.Windows.Forms.dll','WPF\WindowsBase.dll','WPF\PresentationCore.dll','WPF\PresentationFramework.dll') | ForEach-Object { '/reference:' + (Join-Path $framework $_) }
$sourceDirectories = @('Editor','Engine','Rendering','Physics','Scripting','Assets')
$sources = @($sourceDirectories | ForEach-Object { Get-ChildItem -LiteralPath (Join-Path $root $_) -Filter '*.cs' -Recurse } | ForEach-Object FullName)
$sources += @(Get-ChildItem -LiteralPath $root -Filter '*Tests.cs' | ForEach-Object FullName)
New-Item -ItemType Directory -Path (Join-Path $root 'bin') -Force | Out-Null
& (Join-Path $framework 'csc.exe') /nologo /target:winexe /optimize+ ('/win32icon:' + (Join-Path $root 'Aurora.ico')) ('/out:' + (Join-Path (Join-Path $root 'bin') $OutputName)) $refs $sources
if ($LASTEXITCODE -ne 0) { throw 'C# compilation failed' }
Write-Host "Built bin\$OutputName"


param([switch]$CheckOnly)
$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path -LiteralPath $csc)) { throw 'C# compiler not found.' }
Push-Location $workspace
try {
    $outputDirectory = Join-Path $workspace 'build\bin'
    if (-not $CheckOnly) { New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null }
    $outputExe = if ($CheckOnly) { Join-Path $env:TEMP ('TutusOptimizer-check-' + [guid]::NewGuid().ToString('N') + '.exe') } else { Join-Path $outputDirectory 'TutusOptimizer.exe' }
    $sources = @(Get-ChildItem -LiteralPath (Join-Path $workspace 'src') -Filter '*.cs' -File -Recurse | Sort-Object FullName | ForEach-Object { $_.FullName })
    if ($sources.Count -eq 0) { throw 'No source files found in src.' }
    $compilerArgs = @('/nologo', '/target:winexe', '/optimize+', '/codepage:65001', '/utf8output', '/warn:4', ('/win32manifest:' + (Join-Path $workspace 'assets\app.manifest')), ('/win32icon:' + (Join-Path $workspace 'assets\app.ico')), '/r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.ServiceProcess.dll,System.Management.dll,System.Xml.dll', "/out:$outputExe") + $sources
    & $csc @compilerArgs
    if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $LASTEXITCODE" }
    if ($CheckOnly) { Remove-Item -LiteralPath $outputExe; Write-Host 'Compilation verified.' }
    else {
        $mainExecutable = Join-Path $workspace "Tutu's Optimizer.exe"
        try {
            Copy-Item -LiteralPath $outputExe -Destination $mainExecutable -Force
            Write-Host "Main executable updated: $mainExecutable"
        } catch {
            Write-Host "Main executable is unavailable: $($_.Exception.Message)"
            Write-Host "New build available: $outputExe"
            Write-Host 'Close the running app and run this build script again to replace the main executable.'
        }
    }
} finally { Pop-Location }

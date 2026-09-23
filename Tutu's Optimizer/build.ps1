$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
}

if (-not (Test-Path $csc)) {
    Write-Error "Compilador C# (csc.exe) não encontrado no .NET Framework."
    exit 1
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Compilando Tutu's Windows Optimizer (.exe)..." -ForegroundColor Yellow
Write-Host "==================================================" -ForegroundColor Cyan

$outputExe = "TutusOptimizer.exe"

$commandLine = "/target:winexe /optimize+ /win32manifest:app.manifest /win32icon:app.ico /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.ServiceProcess.dll,System.Management.dll /out:`"$outputExe`" Program.cs Models.cs TweakEngine.cs MainForm.cs"

Write-Host "Executando compilador C#..." -ForegroundColor Gray
$proc = Start-Process -FilePath $csc -ArgumentList $commandLine -NoNewWindow -Wait -PassThru

if ($proc.ExitCode -eq 0) {
    # Criamos também a cópia com o nome original do projeto
    Copy-Item $outputExe "Tutu's Optimizer.exe" -Force
    Write-Host ""
    Write-Host "==================================================" -ForegroundColor Green
    Write-Host "  [SUCESSO] Aplicativo gerado com sucesso!" -ForegroundColor Green
    Write-Host "==================================================" -ForegroundColor Green
    Get-Item "Tutu's Optimizer.exe" | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
} else {
    Write-Host ""
    Write-Host "  [ERRO] Falha na compilação. Código de saída: $($proc.ExitCode)" -ForegroundColor Red
}
exit $proc.ExitCode

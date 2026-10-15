$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskOutput = Join-Path $taskRoot 'release\windows'
$taskCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $taskCompiler)) { throw '.NET Framework 4.x compiler is required.' }
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
& $taskCompiler /nologo /target:winexe /platform:anycpu /optimize+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /resource:"$taskRoot\assets\audio\pump.wav,ShotgunKeyboard.Audio.pump.wav" /resource:"$taskRoot\assets\audio\double.wav,ShotgunKeyboard.Audio.double.wav" /resource:"$taskRoot\assets\audio\tactical.wav,ShotgunKeyboard.Audio.tactical.wav" /out:"$taskOutput\ShotgunKeyboard.exe" "$taskRoot\windows\ShotgunKeyboard.cs"
if ($LASTEXITCODE -ne 0) { throw 'Windows compilation failed.' }
$taskTest = Start-Process -FilePath (Join-Path $taskOutput 'ShotgunKeyboard.exe') -ArgumentList '--self-test' -Wait -PassThru -WindowStyle Hidden
if ($taskTest.ExitCode -ne 0) { throw 'Windows audio asset self-test failed.' }
Copy-Item -LiteralPath (Join-Path $taskRoot 'docs\WINDOWS.txt') -Destination (Join-Path $taskOutput 'PROCITAJ.txt') -Force
Compress-Archive -LiteralPath (Join-Path $taskOutput 'ShotgunKeyboard.exe'), (Join-Path $taskOutput 'PROCITAJ.txt') -DestinationPath (Join-Path $taskRoot 'release\shotgun-keyboard-windows.zip') -Force
Write-Output 'Windows compiled, audio assets validated, ZIP packaged.'

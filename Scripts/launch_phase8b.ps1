param(
    [ValidateSet('720','768','1080')][string]$Resolution='720',
    [int]$Seconds=480,
    [int]$Port=7777
)
$projectPath=Split-Path -Parent $PSScriptRoot
$gamePath=Join-Path $projectPath 'Builds/Phase8B/BusinessAsUsual.exe'
if(-not (Test-Path -LiteralPath $gamePath)){throw 'Build Phase 8B in Unity before launching.'}
$width=@{'720'=1280;'768'=1366;'1080'=1920}[$Resolution]
$height=@{'720'=720;'768'=768;'1080'=1080}[$Resolution]
$windowArgs=@('-screen-fullscreen','0','-screen-width',"$width",'-screen-height',"$height",'--bau-phase8b','--bau-rescue','--bau-port',"$Port",'--bau-workday-seconds',"$Seconds")
# One visible, interactive game window. Solo/host/join are chosen in its main menu.
Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList $windowArgs -WindowStyle Normal

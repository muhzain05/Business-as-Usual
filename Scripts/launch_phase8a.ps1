param(
    [ValidateSet('solo','desk','host','client','four')][string]$Mode='solo',
    [string]$Address='127.0.0.1',
    [ValidateSet('720','768','1080','1200')][string]$Resolution='1080',
    [int]$Seconds=480,
    [int]$Port=7777
)
$projectPath=Split-Path -Parent $PSScriptRoot
$gamePath=Join-Path $projectPath 'Builds/Phase8A-RescueV2/BusinessAsUsual.exe'
if(-not (Test-Path -LiteralPath $gamePath)){throw 'Build Phase 8A in Unity before launching.'}
$width=@{'720'=1280;'768'=1366;'1080'=1920;'1200'=1920}[$Resolution]
$height=@{'720'=720;'768'=768;'1080'=1080;'1200'=1200}[$Resolution]
$windowArgs=@('-screen-fullscreen','0','-screen-width',"$width",'-screen-height',"$height",'--bau-rescue','--bau-port',"$Port",'--bau-workday-seconds',"$Seconds")
# These are visible interactive game windows for the manual playtest, not a background helper.
if($Mode -eq 'four'){
    Write-Host 'Four local windows are for multiplayer testing. Each window controls a different worker; choose three leads and one assistant.'
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode','host')) -WindowStyle Normal
    Start-Sleep -Seconds 2
    for($staffIndex=0;$staffIndex -lt 3;$staffIndex++){
        Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode','client','--bau-address',$Address)) -WindowStyle Normal
    }
}elseif($Mode -eq 'desk'){
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList $windowArgs -WindowStyle Normal
}else{
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode',$Mode,'--bau-address',$Address)) -WindowStyle Normal
}

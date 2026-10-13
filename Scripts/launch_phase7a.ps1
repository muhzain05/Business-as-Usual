param([ValidateSet('desk','host','client','pair')][string]$Mode='pair',[string]$Address='127.0.0.1')
$projectPath=Split-Path -Parent $PSScriptRoot
$gamePath=Join-Path $projectPath 'Builds/Phase7A/BusinessAsUsual.exe'
if(-not (Test-Path -LiteralPath $gamePath)){throw 'Build Phase 7A in Unity (Business as Usual > Phase 7A > Build Windows Town) before launching.'}
$windowArgs=@('-screen-fullscreen','0','-screen-width','1280','-screen-height','720')
if($Mode -eq 'pair'){
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode','host')) -WindowStyle Normal
    Start-Sleep -Seconds 2
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode','client','--bau-address',$Address)) -WindowStyle Normal
}elseif($Mode -eq 'desk'){
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList $windowArgs -WindowStyle Normal
}else{
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode',$Mode,'--bau-address',$Address)) -WindowStyle Normal
}

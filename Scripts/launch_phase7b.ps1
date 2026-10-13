param([ValidateSet('desk','host','client','pair')][string]$Mode='pair',[string]$Address='127.0.0.1',[ValidateSet('720','1080')][string]$Resolution='1080')
$projectPath=Split-Path -Parent $PSScriptRoot
$gamePath=Join-Path $projectPath 'Builds/Phase7B/BusinessAsUsual.exe'
if(-not (Test-Path -LiteralPath $gamePath)){throw 'Build Phase 7B in Unity before launching.'}
$width=if($Resolution -eq '1080'){1920}else{1280}
$height=if($Resolution -eq '1080'){1080}else{720}
$windowArgs=@('-screen-fullscreen','0','-screen-width',"$width",'-screen-height',"$height")
if($Mode -eq 'pair'){
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode','host')) -WindowStyle Normal
    Start-Sleep -Seconds 2
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode','client','--bau-address',$Address)) -WindowStyle Normal
}elseif($Mode -eq 'desk'){
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList $windowArgs -WindowStyle Normal
}else{
    Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path $gamePath) -ArgumentList ($windowArgs+@('--bau-mode',$Mode,'--bau-address',$Address)) -WindowStyle Normal
}

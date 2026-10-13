param(
    [ValidateSet('host','client','single','menu')][string]$Mode='menu',
    [string]$Address='127.0.0.1'
)
$phase5Root=Split-Path -Parent $PSScriptRoot
$phase5Exe=Join-Path $phase5Root 'Builds/Phase5/BusinessAsUsual.exe'
if(-not (Test-Path -LiteralPath $phase5Exe)){throw 'Build the Phase 5 Windows prototype in Unity first.'}
# Visible interactive game window, intentionally available for player control.
Start-Process -FilePath $phase5Exe -WorkingDirectory (Split-Path $phase5Exe) -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','--bau-mode',$Mode,'--bau-address',$Address)

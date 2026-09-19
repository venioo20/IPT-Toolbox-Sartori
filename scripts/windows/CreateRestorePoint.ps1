# IPT Toolbox - création d'un point de restauration
# À exécuter uniquement après confirmation explicite et avec privilèges administrateur.
param(
    [string]$Description = "IPT Toolbox - Avant modifications"
)

$ErrorActionPreference = "Stop"
Checkpoint-Computer -Description $Description -RestorePointType "MODIFY_SETTINGS"

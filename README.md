# MNG Launcher

Launcher local FIFA 17 + serveur privé. Voir `CAHIER_DES_CHARGES.md`.

## Axe courant — Origin LSX Emulator

FIFA parle à Origin via **TCP 127.0.0.1:3216** (protocole LSX).  
Aujourd’hui **Origin.exe** tient ce port → FIFA n’utilise pas une session MNG.

### Test immédiat

```powershell
# 1) Fermer Origin
Stop-Process -Name Origin -Force -ErrorAction SilentlyContinue

# 2) Démarrer l’émulateur LSX (Node, dans fifa serveur)
cd "..\fifa serveur"
.\tools\run-origin-lsx.ps1

# 3) Lancer FIFA17 — logs attendus:
#    LSX_LISTENING / LSX_CLIENT_CONNECTED / LSX_CHALLENGE_ACCEPTED / LSX_REQUEST
```

Code C# bridge : `OriginLsx\OriginLsxServer.cs`  
Détails : `OriginLsx\README.md`

### Critère de succès

`ORIGIN_CHECK_ONLINE online=1` → Login naturel → out-flags → LoginComplete  
Sans toucher Blaze Auth/10 / SUCC_POKE.

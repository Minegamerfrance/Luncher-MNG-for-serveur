# MNG Launcher — Origin LSX axis

## État port 3216 (vérifié)

```text
127.0.0.1:3216  LISTENING  → Origin.exe (legacy)
```

Donc FIFA parle déjà à **Origin**, pas à MNG. Pour le test LSX MNG :

1. Fermer Origin (`Stop-Process -Name Origin -Force`)
2. Démarrer l’émulateur LSX
3. Lancer FIFA17
4. Chercher `LSX_CLIENT_CONNECTED` / `ORIGIN_CHECK_ONLINE`

## Démarrage rapide (Node — opérationnel)

```powershell
cd "C:\Users\Mineg\Desktop\serveur fifa 17\fifa serveur"
# Origin doit être fermé
.\tools\run-origin-lsx.ps1
```

Ou :

```powershell
npm run start:lsx
```

Session : `active-session.json` (repo) ou `%LOCALAPPDATA%\MNGLauncher\active-session.json`

## C# bridge

`OriginLsx\OriginLsxServer.cs` démarre le même process Node depuis le futur WinForms launcher.

## Critère de succès

```text
LSX_LISTENING
LSX_CLIENT_CONNECTED
LSX_CHALLENGE_ACCEPTED
LSX_REQUEST type=GetInternetConnectedState
LSX_ONLINE_EVENT_SENT
→ Frida: ORIGIN_CHECK_ONLINE online=1
→ Login naturel / out-flags / LoginComplete
```

Ne pas toucher Blaze Auth/10, SUCC_POKE, out-flags poke.

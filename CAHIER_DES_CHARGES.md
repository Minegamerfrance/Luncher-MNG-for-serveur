# Cahier des charges complet — **MNG Launcher**

## 1. Objectif général

Créer une application Windows appelée **MNG Launcher** servant de launcher local pour FIFA 17 et le serveur privé/local associé.

Le launcher doit permettre de :

* créer un compte local avec une adresse e-mail, un mot de passe et une persona ;
* se connecter avec ce compte ;
* créer une session compatible avec le serveur local ;
* démarrer automatiquement le serveur FIFA 17 ;
* tester que les services Redirector, Blaze, Nucleus et FUT fonctionnent ;
* lancer `FIFA17.exe` ;
* afficher les logs en direct ;
* préparer un bridge local Origin/Ebisu pour fournir au jeu les événements de connexion qu’il attend après l’authentification Blaze.

Le launcher doit fonctionner **uniquement en local** et ne doit jamais envoyer les identifiants vers EA.

---

## 2. Nom et identité

Nom de l’application :

```text
MNG Launcher
```

Titre de la fenêtre :

```text
MNG Launcher - FIFA 17 Local
```

Nom de l’exécutable :

```text
MNGLauncher.exe
```

Nom du projet :

```text
MNGLauncher.csproj
```

Dossier de données utilisateur :

```text
%LOCALAPPDATA%\MNGLauncher
```

---

## 3. Technologie

Utiliser :

```text
C#
.NET 8
Windows Forms
Windows x64
```

Publication :

```text
Self-contained
RuntimeIdentifier = win-x64
PublishSingleFile = true si possible
```

Commande de build :

```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true
```

Sortie attendue :

```text
bin\Release\net8.0-windows\win-x64\publish\MNGLauncher.exe
```

---

## 4. Écrans de l’application

### Écran 1 — Connexion

Champs :

```text
Adresse e-mail
Mot de passe
```

Boutons :

```text
Connexion
Créer un compte
Quitter
```

Fonctions :

* vérifier que le compte existe ;
* vérifier le mot de passe ;
* ouvrir l’écran principal après connexion ;
* afficher un message clair si le mot de passe est incorrect ;
* mémoriser éventuellement le dernier e-mail utilisé ;
* ne jamais enregistrer le mot de passe en clair.

### Écran 2 — Création de compte

Champs :

```text
Adresse e-mail
Mot de passe
Confirmation du mot de passe
Nom de persona
```

Exemple :

```text
E-mail : maxence30evrard@gmail.com
Persona : maxence30evrard
```

À la création du compte, générer automatiquement :

```text
UID
Persona ID
Date de création
Salt du mot de passe
Hash du mot de passe
```

Exemple :

```text
UID = 1000000001
PersonaId = 2000000001
PersonaName = maxence30evrard
```

Les IDs doivent être uniques pour chaque compte.

Validation :

* e-mail valide ;
* e-mail non déjà utilisé ;
* mot de passe minimum de 8 caractères ;
* confirmation identique ;
* persona non vide ;
* persona unique si possible.

### Écran 3 — Tableau de bord principal

Afficher en haut :

```text
MNG LAUNCHER
Connecté : adresse@email.com
Persona : NomPersona
UID : 1000000001
```

Afficher aussi l’état du bridge :

```text
Bridge Origin/Ebisu : non démarré
Bridge Origin/Ebisu : en attente
Bridge Origin/Ebisu : connecté
Bridge Origin/Ebisu : erreur
```

#### Champs

```text
Dossier du serveur local
Chemin de FIFA17.exe
```

#### Boutons

```text
Parcourir serveur
Parcourir FIFA17.exe
Enregistrer
Créer la session
Démarrer le serveur
Arrêter le serveur
Tester la connexion
Lancer FIFA 17
Déconnexion
```

Ajouter éventuellement un bouton principal :

```text
Tout démarrer
```

Il doit effectuer automatiquement :

```text
Créer session
→ démarrer serveur
→ attendre les services
→ tester la connexion
→ démarrer le bridge
→ lancer FIFA 17
```

---

## 5. Sauvegarde des comptes

Créer un fichier local, par exemple :

```text
%LOCALAPPDATA%\MNGLauncher\accounts.json
```

Structure recommandée :

```json
{
  "accounts": [
    {
      "email": "maxence30evrard@gmail.com",
      "personaName": "maxence30evrard",
      "uid": 1000000001,
      "personaId": 2000000001,
      "passwordHash": "...",
      "passwordSalt": "...",
      "createdAt": "2026-07-28T20:00:00Z"
    }
  ]
}
```

Sécurité :

* utiliser PBKDF2-SHA256, bcrypt ou Argon2 ;
* générer un salt différent par compte ;
* ne jamais écrire le mot de passe original dans un fichier ou un log ;
* les comptes sont uniquement locaux.

---

## 6. Configuration du launcher

Créer :

```text
%LOCALAPPDATA%\MNGLauncher\settings.json
```

Exemple :

```json
{
  "serverDirectory": "C:\\Users\\Mineg\\Desktop\\serveur fifa 17\\fifa serveur",
  "fifaExecutable": "C:\\Users\\Mineg\\Desktop\\serveur fifa 17\\FIFA 17\\FIFA17.exe",
  "autoStartServer": true,
  "autoCreateSession": true,
  "autoStartBridge": true,
  "lastEmail": "maxence30evrard@gmail.com"
}
```

Le launcher doit vérifier :

* que le dossier serveur existe ;
* que le dossier contient `package.json` ;
* que le dossier contient `src` et `tools` ;
* que `FIFA17.exe` existe réellement.

---

## 7. Création de session

Quand l’utilisateur clique sur **Créer la session**, créer :

```text
%LOCALAPPDATA%\MNGLauncher\active-session.json
```

Exemple :

```json
{
  "email": "maxence30evrard@gmail.com",
  "personaName": "maxence30evrard",
  "uid": 1000000001,
  "personaId": 2000000001,
  "authCode": "LOCAL-FIFA17-AUTH",
  "pctl": "LOCAL-PCTK-1000000001",
  "sessionKey": "LOCAL-SKEY-1000000001",
  "online": true,
  "loggedIn": true,
  "createdAt": "2026-07-28T20:00:00Z"
}
```

Les informations doivent être cohérentes avec la réponse Blaze Auth/10.

Mapping serveur recommandé :

```text
UID  = compte.uid
PID  = compte.personaId
DSNM = compte.personaName
AUTH = LOCAL-FIFA17-AUTH
PCTK = LOCAL-PCTK-{UID}
SKEY = LOCAL-SKEY-{UID}
```

Le serveur Node.js doit pouvoir charger ce fichier au démarrage ou lors de l’authentification.

---

## 8. Intégration avec le serveur Node.js

Le launcher doit démarrer le serveur dans le bon dossier avec :

```powershell
npm run start:auth-origin-layer
```

Variables d’environnement à injecter :

```text
PIPE_ORIGIN_ONLINE_FIX=1
PIPE_ORIGIN_AUTHCODE_FIX=1
PIPE_ORIGIN_VERSION_FIX=1
PIPE_EBISU_FIX=1
```

Configuration Auth actuellement utilisée :

```text
AUTH_REPLY_PROFILE=plst
AUTH_NOTIFY=0
```

Le launcher doit :

* vérifier que `node` est installé ;
* vérifier que `npm` est installé ;
* afficher une erreur claire sinon ;
* démarrer le processus sans bloquer l’interface ;
* lire stdout et stderr en temps réel ;
* afficher les logs dans le journal ;
* conserver la référence du processus ;
* permettre de l’arrêter proprement ;
* empêcher plusieurs instances du serveur.

Commande possible :

```powershell
cmd.exe /c npm run start:auth-origin-layer
```

Working directory :

```text
dossier sélectionné dans "Dossier du serveur local"
```

---

## 9. Test des services

Le bouton **Tester la connexion** doit tester :

| Service        | Adresse           |
| -------------- | ----------------- |
| Redirector TLS | `127.0.0.1:42230` |
| Blaze          | `127.0.0.1:10041` |
| Nucleus HTTP   | `127.0.0.1:4433`  |
| FUT HTTP       | `127.0.0.1:8000`  |

Afficher :

```text
Services disponibles : 4/4
```

Détail :

```text
OK - Redirector TLS 127.0.0.1:42230
OK - Blaze 127.0.0.1:10041
OK - Nucleus HTTP 127.0.0.1:4433
OK - FUT HTTP 127.0.0.1:8000
```

Attention pour Blaze :

Le simple test TCP ouvre puis ferme une connexion sans envoyer de ClientHello. Le serveur peut donc afficher :

```text
Blaze setup failed: socket closed before first byte
```

Ce message ne doit pas être présenté comme une panne lorsque le port répond.

Pour HTTP, faire de vraies requêtes simples, par exemple :

```text
GET http://127.0.0.1:4433/
GET http://127.0.0.1:8000/
```

---

## 10. Lancement de FIFA 17

Le bouton **Lancer FIFA 17** doit vérifier :

* utilisateur connecté ;
* session créée ;
* serveur démarré ;
* services disponibles ;
* chemin de `FIFA17.exe` valide.

Lancement :

```csharp
new ProcessStartInfo
{
    FileName = fifaExecutable,
    WorkingDirectory = Path.GetDirectoryName(fifaExecutable),
    UseShellExecute = true
};
```

Conserver la référence du processus FIFA.

Afficher dans le journal :

```text
FIFA 17 lancé.
PID : 12345
```

Ne pas démarrer plusieurs fois le jeu si un processus `FIFA17.exe` est déjà actif.

---

## 11. Journal intégré

La zone **Journal** doit recevoir les événements en temps réel.

Exemples :

```text
[20:40:10] Session créée.
[20:40:11] Démarrage du serveur local.
[20:40:13] Redirector : en ligne.
[20:40:13] Blaze : en ligne.
[20:40:13] Nucleus : en ligne.
[20:40:13] FUT : en ligne.
[20:40:14] Bridge Origin/Ebisu démarré.
[20:40:15] FIFA 17 lancé.
```

Prévoir :

```text
Effacer les logs
Copier les logs
Ouvrir le dossier des logs
Exporter le journal
```

Créer aussi :

```text
%LOCALAPPDATA%\MNGLauncher\logs\launcher-AAAA-MM-JJ.log
```

Ne jamais écrire les mots de passe ou hashes complets dans le journal.

Supprimer les codes ANSI des logs Node.js pour éviter les caractères :

```text
[35mscope
[39m
```

Utiliser une regex pour nettoyer les couleurs ANSI.

---

## 12. Bridge Origin/Ebisu — partie critique

### Contexte technique déjà prouvé

La chaîne actuelle fonctionne jusqu’à :

```text
Redirector
→ Blaze TLS
→ PreAuth
→ OriginAuthCodeLogin component=1 command=10
→ réponse error=0
→ callback Blaze exécutée
```

Mais ensuite :

```text
callback Auth/10
→ aucune completion du job Login
→ waiter reste BUSY
→ login+0x260 reste 2
→ timeout après environ 35 secondes
→ login+0x260 passe à 16
→ logout component=1 command=70
```

Le vrai problème est donc :

```text
le bridge entre la réponse Auth Blaze et le job local Login n’est jamais déclenché
```

Le launcher doit préparer un module local appelé :

```text
OriginEbisuBridge
```

### Responsabilités du bridge

Le bridge devra pouvoir annoncer à FIFA :

```text
Origin disponible
Utilisateur connecté
Mode online actif
UID disponible
Persona disponible
Auth code disponible
Login local terminé
```

Données fournies :

```text
Email
UID
Persona ID
Persona Name
Auth Code
Online Status
Session State
```

Événements logiques prévus :

```text
OriginStarted
UserLoggedIn
OnlineStateChanged
PersonaAvailable
AuthCodeAvailable
LoginComplete
NetworkLoginEvent
```

Le bridge doit être conçu comme une interface modulaire :

```csharp
public interface IOriginBridge
{
    bool IsRunning { get; }
    bool IsClientConnected { get; }

    Task StartAsync(LocalSession session);
    Task StopAsync();

    event EventHandler<ClientConnectedEventArgs> ClientConnected;
    event EventHandler<LoginCompletedEventArgs> LoginCompleted;
}
```

### Protocoles possibles

Ne pas choisir définitivement le protocole sans preuve.

Préparer plusieurs transports activables :

```text
Named Pipe Windows
Socket TCP localhost
Socket UDP localhost
Shared memory
Window messages
DLL/shim injecté
Hook Frida
```

Architecture :

```text
IOriginTransport
├── NamedPipeOriginTransport
├── TcpOriginTransport
├── SharedMemoryOriginTransport
└── FridaBridgeTransport
```

La première version doit surtout :

* démarrer un listener local ;
* journaliser toute connexion ;
* afficher les bytes/messages reçus ;
* pouvoir répondre avec les informations de session ;
* exposer un bouton de test interne.

---

## 13. État du bridge dans l’interface

Ajouter une section :

```text
Bridge Origin/Ebisu
```

Avec :

```text
État : non démarré
Client FIFA détecté : non
Utilisateur publié : oui/non
Événement LoginComplete : envoyé/non envoyé
Dernier message : ...
```

Boutons :

```text
Démarrer le bridge
Arrêter le bridge
Tester le bridge
Afficher les messages IPC
```

Le launcher ne doit pas afficher « bridge connecté » uniquement parce que le serveur Blaze est en ligne. Il faut réellement détecter une connexion ou un appel du jeu.

---

## 14. Intégration de la session au serveur

Le serveur Node.js doit lire `active-session.json`.

Option recommandée :

Le launcher passe le chemin avec une variable :

```text
MNG_SESSION_FILE=C:\Users\Mineg\AppData\Local\MNGLauncher\active-session.json
```

Le serveur TypeScript charge :

```ts
const sessionFile = process.env.MNG_SESSION_FILE;
```

Il doit utiliser les valeurs pour générer la réponse Auth/10 :

```text
UID
PID
DSNM
PCTK
SKEY
AUTH
```

Ainsi, le compte créé dans le launcher correspond réellement au compte vu dans le jeu.

---

## 15. Interface visuelle

Conserver une interface simple mais moderne.

Palette recommandée :

```text
Fond principal : #101318
Panneaux : #181D24
Accent : orange #FF7A00
Texte : blanc
Succès : vert
Erreur : rouge
Attente : jaune/orange
```

Éléments :

* logo ou texte `MNG LAUNCHER` ;
* cartes d’état pour les quatre services ;
* barre de progression lors du démarrage ;
* boutons désactivés lorsque l’action n’est pas disponible ;
* logs avec couleurs ;
* icône personnalisée `.ico`.

États visuels :

```text
● Hors ligne
● Démarrage
● En ligne
● Erreur
```

---

## 16. Architecture des fichiers

```text
MNGLauncher/
├── MNGLauncher.csproj
├── Program.cs
├── build.ps1
├── README.md
│
├── Forms/
│   ├── LoginForm.cs
│   ├── RegisterForm.cs
│   ├── MainForm.cs
│   ├── SettingsForm.cs
│   └── DiagnosticsForm.cs
│
├── Models/
│   ├── LocalAccount.cs
│   ├── LocalSession.cs
│   ├── LauncherSettings.cs
│   ├── ServiceStatus.cs
│   └── BridgeMessage.cs
│
├── Services/
│   ├── AccountService.cs
│   ├── PasswordService.cs
│   ├── SessionService.cs
│   ├── SettingsService.cs
│   ├── ProcessService.cs
│   ├── ServerService.cs
│   ├── ConnectionTestService.cs
│   ├── LogService.cs
│   └── OriginEbisuBridgeService.cs
│
├── Bridge/
│   ├── IOriginBridge.cs
│   ├── IOriginTransport.cs
│   ├── NamedPipeOriginTransport.cs
│   ├── TcpOriginTransport.cs
│   └── BridgeProtocol.cs
│
└── Assets/
    ├── logo.png
    └── mnglauncher.ico
```

---

## 17. Gestion des erreurs

Tous les boutons doivent avoir un `try/catch`.

Exemples de messages :

```text
Le dossier serveur ne contient pas package.json.
FIFA17.exe est introuvable.
Node.js n’est pas installé.
npm est introuvable.
Le serveur est déjà en cours d’exécution.
Impossible de joindre Blaze sur le port 10041.
La session locale n’a pas encore été créée.
Le bridge Origin/Ebisu n’est pas prêt.
```

Le script `build.ps1` doit s’arrêter dès qu’une commande échoue :

```powershell
$ErrorActionPreference = "Stop"

dotnet restore
if ($LASTEXITCODE -ne 0) {
    throw "Échec de dotnet restore"
}

dotnet publish -c Release -r win-x64 --self-contained true
if ($LASTEXITCODE -ne 0) {
    throw "Échec de dotnet publish"
}
```

---

## 18. Fonction « Tout démarrer »

Ajouter un bouton principal :

```text
JOUER À FIFA 17
```

Séquence :

```text
1. Vérifier le compte
2. Sauvegarder la configuration
3. Créer active-session.json
4. Démarrer le serveur Node.js
5. Attendre les quatre services
6. Démarrer OriginEbisuBridge
7. Attendre l’état Bridge Ready
8. Lancer FIFA17.exe
9. Surveiller le processus FIFA
```

En cas d’échec :

```text
arrêter la séquence
afficher l’étape qui a échoué
laisser les logs visibles
```

---

## 19. Arrêt propre

À la fermeture du launcher :

* demander confirmation si FIFA ou le serveur tourne ;
* arrêter le bridge ;
* arrêter le serveur Node.js ;
* fermer les pipes/sockets ;
* enregistrer les réglages ;
* ne pas supprimer le compte ;
* supprimer ou invalider `active-session.json` si nécessaire.

Bouton :

```text
Tout arrêter
```

---

## 20. Critères de validation

### Comptes

```text
✓ création d’un compte
✓ mot de passe hashé
✓ connexion correcte
✓ refus d’un mauvais mot de passe
✓ UID et Persona ID persistants
```

### Serveur

```text
✓ démarrage npm
✓ logs affichés en direct
✓ arrêt propre
✓ absence de double démarrage
```

### Connexion

```text
✓ Redirector 42230
✓ Blaze 10041
✓ Nucleus 4433
✓ FUT 8000
✓ résultat 4/4
```

### Jeu

```text
✓ chemin FIFA sauvegardé
✓ bon WorkingDirectory
✓ lancement de FIFA17.exe
✓ détection si le jeu est déjà lancé
```

### Session

```text
✓ active-session.json créé
✓ UID/persona cohérents avec Auth/10
✓ serveur utilisant le compte actif
```

### Bridge

Première validation :

```text
✓ service bridge démarré
✓ logs IPC visibles
✓ connexion du jeu détectable
```

Validation finale recherchée :

```text
Origin/Ebisu callback reçu
→ waiter+0x60 quitte BUSY
→ login+0x260 quitte l’état 2
→ pas de timeout FAIL16
→ pas de logout/70
→ accès au menu UT
```

---

## 21. État actuel du projet

Déjà fonctionnel :

```text
Création de compte local
Connexion
Stockage sécurisé du mot de passe
Session locale
Sélection du serveur
Sélection de FIFA17.exe
Démarrage du serveur
Test des quatre services
Lancement de FIFA 17
Affichage des logs
```

Résultat actuel :

```text
Services disponibles : 4/4
Redirector : OK
Blaze : OK
Nucleus : OK
FUT : OK
Auth/10 : error=0
```

Blocage actuel :

```text
Bridge Origin/Ebisu réel non branché
Job Login reste BUSY
Timeout après environ 35 secondes
Logout/70
```

---

## Instruction finale pour Cursor

```text
Continue le projet MNG Launcher existant en C# .NET 8 WinForms.

Ne recrée pas tout depuis zéro.

Conserve les fonctions déjà présentes :
- comptes locaux ;
- connexion ;
- session ;
- configuration ;
- lancement du serveur ;
- tests 4/4 ;
- lancement de FIFA17.exe ;
- journal.

Améliore d’abord la structure et la robustesse.

Ensuite crée un module OriginEbisuBridge local, modulaire et traçable.

Ne simule pas encore arbitrairement un LoginComplete sans connaître le protocole utilisé par FIFA 17.

Le bridge doit commencer par détecter et journaliser les communications locales du jeu, puis répondre avec la session active.

Toutes les données UID, Persona ID, PersonaName et AuthCode doivent provenir de active-session.json et rester cohérentes avec la réponse Blaze Auth/10.

Le critère final est que le waiter Login sorte de BUSY après Auth/10 et que FIFA 17 n’envoie plus logout/70 après 35 secondes.
```

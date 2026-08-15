# 🎮 MNG Launcher — FIFA 17 Local Server

MNG Launcher est un launcher Windows développé pour accompagner le projet **FIFA 17 — Serveur Local**.

Son objectif est de simplifier le lancement, la configuration, les tests et le diagnostic de l'environnement local utilisé pour travailler sur la restauration expérimentale des services en ligne de FIFA 17.

> ⚠️ Ce dépôt contient uniquement **MNG Launcher**.
>
> Le code du serveur est disponible dans un dépôt séparé.

---
<img width="1716" height="917" alt="image" src="https://github.com/user-attachments/assets/721b2094-d664-482b-862c-7b0807472f02" />
---

# 🌐 Serveur FIFA 17

Le serveur possède maintenant son propre dépôt GitHub :

👉 **https://github.com/Minegamerfrance/FIFA-17---Serveur-Local**

C'est sur ce dépôt que se trouve le travail concernant le serveur local, les services réseau et les recherches autour du fonctionnement des anciens services FIFA 17.

---

# 🚀 Objectif de MNG Launcher

Pendant le développement du serveur, de nombreux composants doivent être lancés et surveillés.

MNG Launcher a été créé pour centraliser ces opérations dans une seule interface.

Il permet notamment de faciliter :

- le démarrage du serveur local ;
- le lancement de FIFA 17 ;
- la création d'une session locale ;
- la configuration des chemins ;
- l'affichage des logs ;
- le lancement des outils de test ;
- le diagnostic des connexions ;
- les différents tests nécessaires au développement du serveur.

L'objectif est d'éviter d'avoir à lancer manuellement plusieurs scripts et programmes à chaque test.

---

# 🖥️ Fonctionnalités

### ⚙️ Configuration

Le launcher permet de configurer :

- le chemin du serveur ;
- le chemin de `FIFA17.exe` ;
- les informations de la session locale ;
- différents paramètres nécessaires aux tests.

### ▶️ Contrôle

Depuis l'interface, il est possible de :

- démarrer le serveur ;
- arrêter le serveur ;
- lancer FIFA 17 ;
- créer une session locale ;
- tester les services ;
- suivre l'état des différents composants.

### 📋 Logs

MNG Launcher centralise également les informations de diagnostic afin de faciliter le développement et les tests.

---

# 🔌 Origin LSX

Une partie du launcher travaille autour de la communication entre FIFA 17 et Origin.

FIFA 17 utilise notamment le protocole **LSX** pour certaines communications locales.

Le projet contient une implémentation expérimentale :

`OriginLsx/OriginLsxServer.cs`

L'objectif est de comprendre et reproduire les échanges nécessaires au fonctionnement de l'environnement local.

---

# 🛠️ Technologies

MNG Launcher est principalement développé avec :

- **C#**
- **.NET 8**
- **Windows Forms**
- **PowerShell**
- **TCP / LSX**

Plateforme actuellement ciblée :

`Windows x64`

---

# 📁 Structure du projet

```text
MNG Launcher/
│
├── OriginLsx/
│   ├── OriginLsxServer.cs
│   └── README.md
│
├── LauncherSettings.cs
├── MainForm.cs
├── Program.cs
├── MNGLauncher.csproj
├── app.manifest
├── build.ps1
├── CAHIER_DES_CHARGES.md
└── README.md
```

Les fichiers générés lors de la compilation (`bin/`, `obj/`, etc.) ne sont volontairement pas inclus dans le dépôt.

---

# 🔨 Compilation

Le projet nécessite **.NET 8 SDK**.

Cloner le dépôt puis utiliser :

```powershell
dotnet restore
dotnet build
```

Un script :

```text
build.ps1
```

est également disponible pour faciliter certaines opérations de compilation.

---

# 🔗 Les deux projets

## 🖥️ MNG Launcher

Ce dépôt contient :

- l'interface du launcher ;
- les outils de lancement ;
- la configuration ;
- les outils de diagnostic ;
- l'intégration LSX côté launcher.

## 🌐 FIFA 17 — Serveur Local

Le serveur est développé séparément :

**https://github.com/Minegamerfrance/FIFA-17---Serveur-Local**

Le serveur constitue le cœur du travail de recherche et de reconstruction des services nécessaires à l'environnement FIFA 17 local.

---

# 🤝 Nous recherchons de l'aide

Le projet est encore expérimental et en développement actif.

Toute personne ayant des connaissances dans les domaines suivants peut contribuer :

- C# / .NET
- développement réseau
- reverse engineering
- analyse de protocoles
- Origin / LSX
- services FIFA
- debugging
- PowerShell

Vous pouvez utiliser les **Issues** pour signaler un problème ou proposer une piste.

Les **Pull Requests** sont également les bienvenues.

---

# ⚠️ Disclaimer

Ce projet est un projet communautaire, expérimental et non commercial.

Il n'est **ni affilié, ni approuvé, ni sponsorisé par Electronic Arts (EA)**.

Aucun fichier du jeu FIFA 17 n'est fourni dans ce dépôt.

Les utilisateurs doivent disposer légalement de leurs propres fichiers du jeu et des éléments nécessaires à leurs expérimentations.

---

# 📌 État du projet

🚧 **Développement actif**

MNG Launcher évolue parallèlement au projet **FIFA 17 — Serveur Local**.

Le launcher sera progressivement amélioré à mesure que de nouveaux besoins apparaîtront pendant le développement et les tests du serveur.

⭐ Si le projet vous intéresse, vous pouvez suivre les deux dépôts et contribuer aux recherches.

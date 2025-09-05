# Remarkable SleepScreen Manager

![screenshot](Assets/app_screenshot.png) <!-- optionnel si tu ajoutes une capture -->

Une petite application **Windows (WPF, .NET 8)** qui permet de changer facilement l’écran de veille (*sleep screen*) d’une tablette **reMarkable** (Paper Pro ou RM2).  

---

## ✨ Fonctionnalités
- Connexion simple à la tablette via **SSH/SFTP** (mot de passe root).
- Sélection et **prévisualisation** d’une image PNG.
- **Redimensionnement automatique** à la résolution de la tablette :
  - Paper Pro → `2160×1620`
  - RM2 (support à venir) → `1404×1872`
- Upload et application directe sur la tablette (avec redémarrage de l’UI).
- Bouton **Restore** qui restaure l’image par défaut incluse dans l’app (`Assets/suspended.png`).
- Interface légère et portable : un seul `.exe`.

---

## 🖼️ Captures
*(ajoute une ou plusieurs images ici — par ex. `doc/screenshot.png`)*

---

## 🔧 Installation
1. **Télécharge** la dernière release (ou compile depuis les sources).
2. Active le **mode développeur** sur ta reMarkable :
   - Settings → General → Software version → Developer Mode → ON
   - Note l’**IP** (souvent `10.11.99.1` en USB) et le **mot de passe root**.
3. Lance `RemarkableSleepScreenManager.exe`.
4. Renseigne :
   - **IP** : `10.11.99.1`
   - **Utilisateur** : `root`
   - **Mot de passe** : affiché sur la tablette
5. Clique sur **Tester** → la connexion doit afficher `uname -a`.
6. Choisis une image, clique sur **Uploader & Appliquer**.
7. Mets la tablette en veille → ton nouvel écran apparaît 🎉.

---

## 🛠️ Compilation (développeurs)
Prérequis :  
- **Visual Studio 2022+**  
- **.NET 8 SDK**  
- **Windows 10/11**  

Étapes :  
```bash
git clone https://github.com/<ton-compte>/RemarkableSleepScreenManager.git
cd RemarkableSleepScreenManager
# Ouvre la solution dans Visual Studio
# Build → Rebuild Solution

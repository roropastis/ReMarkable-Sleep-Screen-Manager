using System;
using System.Globalization;

namespace RemarkableSleepScreenManager.Localization
{
    public static class ResourceManager
    {
        private static CultureInfo _currentCulture = CultureInfo.CurrentCulture;

        public static event EventHandler? CultureChanged;

        public static CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    CultureInfo.CurrentCulture = value;
                    CultureInfo.CurrentUICulture = value;
                    CultureChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static string GetString(string key)
        {
            // Simple hardcoded strings for now
            return key switch
            {
                "AppTitle" => "ReMarkable SleepScreen Manager", // Toujours en anglais
                "Connection" => _currentCulture.Name.StartsWith("fr") ? "Connexion" : "Connection",
                "IpAddress" => _currentCulture.Name.StartsWith("fr") ? "Adresse IP" : "IP Address",
                "Username" => _currentCulture.Name.StartsWith("fr") ? "Utilisateur" : "Username",
                "Password" => _currentCulture.Name.StartsWith("fr") ? "Mot de passe" : "Password",
                "Test" => _currentCulture.Name.StartsWith("fr") ? "Tester" : "Test",
                "Local" => "Local",
                "AutoRotation" => _currentCulture.Name.StartsWith("fr") ? "Rotation auto" : "Auto Rotation",
                "Gallery" => _currentCulture.Name.StartsWith("fr") ? "Galerie" : "Gallery",
                "Image" => "Image (Paper Pro: 2160×1620)",
                "ChooseImage" => _currentCulture.Name.StartsWith("fr") ? "Choisir image…" : "Choose image...",
                "AutoResize" => _currentCulture.Name.StartsWith("fr") ? "Redimensionner automatiquement à 2160×1620" : "Auto-resize to 2160×1620",
                "Actions" => "Actions",
                "UploadApply" => _currentCulture.Name.StartsWith("fr") ? "Uploader & Appliquer" : "Upload & Apply",
                "RestoreDefault" => _currentCulture.Name.StartsWith("fr") ? "Restaurer l'image par défaut" : "Restore default image",
                "OutputLog" => _currentCulture.Name.StartsWith("fr") ? "Sortie / Log" : "Output / Log",
                "Language" => _currentCulture.Name.StartsWith("fr") ? "Langue" : "Language",
                "English" => "English",
                "French" => "Français",
                "ChooseFolder" => _currentCulture.Name.StartsWith("fr") ? "Choisir dossier…" : "Choose folder...",
                "UploadFolder" => _currentCulture.Name.StartsWith("fr") ? "Uploader dossier" : "Upload folder",
                "Install" => _currentCulture.Name.StartsWith("fr") ? "Installer" : "Install",
                "Uninstall" => _currentCulture.Name.StartsWith("fr") ? "Désinstaller" : "Uninstall",
                "TestNow" => _currentCulture.Name.StartsWith("fr") ? "Tester maintenant" : "Test now",
                "Download" => _currentCulture.Name.StartsWith("fr") ? "Télécharger" : "Download",
                "InstallFromGallery" => _currentCulture.Name.StartsWith("fr") ? "Installer" : "Install",
                "BrowseGallery" => _currentCulture.Name.StartsWith("fr") ? "Parcourez et installez un écran de veille depuis la galerie en ligne." : "Browse and install a sleep screen from the online gallery.",
                "AutoRotationDescription" => _currentCulture.Name.StartsWith("fr") ? "Installe une rotation automatique du sleep screen (Paper Pro). Les images conseillées : 2160x1620 portrait. Le changement est visible après une sortie du deep sleep (~12 min)." : "Installs automatic sleep screen rotation (Paper Pro). Recommended images: 2160x1620 portrait. Changes are visible after deep sleep exit (~12 min).",
                "Log" => _currentCulture.Name.StartsWith("fr") ? "Log" : "Log",
                "Copyright" => "© 2025 r0ma1n – ReMarkable SleepScreen Manager v0.0.2", // Toujours en anglais
                "ChooseImageButton" => _currentCulture.Name.StartsWith("fr") ? "Choisir image…" : "Choose image...",
                "AutoResizeCheckBox" => _currentCulture.Name.StartsWith("fr") ? "Redimensionner automatiquement à 2160×1620" : "Auto-resize to 2160×1620",
                "UploadApplyButton" => _currentCulture.Name.StartsWith("fr") ? "Uploader & Appliquer" : "Upload & Apply",
                "RestoreDefaultButton" => _currentCulture.Name.StartsWith("fr") ? "Restaurer l'image par défaut" : "Restore default image",
                "OutputLogLabel" => _currentCulture.Name.StartsWith("fr") ? "Sortie / Log" : "Output / Log",
                "ChooseFolderButton" => _currentCulture.Name.StartsWith("fr") ? "Choisir dossier…" : "Choose folder...",
                "UploadFolderButton" => _currentCulture.Name.StartsWith("fr") ? "Uploader dossier" : "Upload folder",
                "InstallButton" => _currentCulture.Name.StartsWith("fr") ? "Installer" : "Install",
                "UninstallButton" => _currentCulture.Name.StartsWith("fr") ? "Désinstaller" : "Uninstall",
                "TestNowButton" => _currentCulture.Name.StartsWith("fr") ? "Tester maintenant" : "Test now",
                "DownloadButton" => _currentCulture.Name.StartsWith("fr") ? "Télécharger" : "Download",
                "InstallFromGalleryButton" => _currentCulture.Name.StartsWith("fr") ? "Installer" : "Install",
                "IpAddressLabel" => _currentCulture.Name.StartsWith("fr") ? "Adresse IP" : "IP Address",
                "UsernameLabel" => _currentCulture.Name.StartsWith("fr") ? "Utilisateur" : "Username",
                "PasswordLabel" => _currentCulture.Name.StartsWith("fr") ? "Mot de passe" : "Password",
                
                // Status messages
                "TestingConnection" => _currentCulture.Name.StartsWith("fr") ? "Test de connexion..." : "Testing connection...",
                "Connected" => _currentCulture.Name.StartsWith("fr") ? "Connecté" : "Connected",
                "ConnectionFailed" => _currentCulture.Name.StartsWith("fr") ? "Connexion échouée" : "Connection failed",
                "ConnectionError" => _currentCulture.Name.StartsWith("fr") ? "Erreur de connexion" : "Connection error",
                "Error" => _currentCulture.Name.StartsWith("fr") ? "Erreur" : "Error",
                "Ready" => _currentCulture.Name.StartsWith("fr") ? "Prêt" : "Ready",
                "Completed" => _currentCulture.Name.StartsWith("fr") ? "Terminé ✔" : "Completed ✓",
                "Restored" => _currentCulture.Name.StartsWith("fr") ? "Restauré ✔" : "Restored ✓",
                "Downloaded" => _currentCulture.Name.StartsWith("fr") ? "Téléchargé ✔" : "Downloaded ✓",
                "Installed" => _currentCulture.Name.StartsWith("fr") ? "Installé ✔" : "Installed ✓",
                "Uninstalled" => _currentCulture.Name.StartsWith("fr") ? "Désinstallé ✔" : "Uninstalled ✓",
                "RotationInstalled" => _currentCulture.Name.StartsWith("fr") ? "Rotation installée ✔" : "Rotation installed ✓",
                
                // Log messages
                "ImageSelected" => _currentCulture.Name.StartsWith("fr") ? "Image sélectionnée" : "Image selected",
                "ImageResized" => _currentCulture.Name.StartsWith("fr") ? "Image redimensionnée" : "Image resized",
                "PutTabletToSleep" => _currentCulture.Name.StartsWith("fr") ? "Mets la tablette en veille pour voir l'écran." : "Put the tablet to sleep to see the new screen.",
                "OriginalScreenRestored" => _currentCulture.Name.StartsWith("fr") ? "L'écran d'origine (bundled) a été restauré." : "Original bundled screen has been restored.",
                "DownloadedTo" => _currentCulture.Name.StartsWith("fr") ? "Téléchargé" : "Downloaded",
                "InstalledFromGallery" => _currentCulture.Name.StartsWith("fr") ? "Installé depuis galerie" : "Installed from gallery",
                "ConnectionTestSuccessful" => _currentCulture.Name.StartsWith("fr") ? "Test de connexion réussi" : "Connection test successful",
                "ConnectionTestFailed" => _currentCulture.Name.StartsWith("fr") ? "Test de connexion échoué" : "Connection test failed",
                "FolderSelected" => _currentCulture.Name.StartsWith("fr") ? "Dossier sélectionné" : "Folder selected",
                "Uploaded" => _currentCulture.Name.StartsWith("fr") ? "Uploadé" : "Uploaded",
                "AutoRotationInstalled" => _currentCulture.Name.StartsWith("fr") ? "Rotation auto (hook) installée. Visible au deep sleep (~12 min)." : "Auto rotation (hook) installed. Visible after deep sleep (~12 min).",
                "AutoRotationUninstalled" => _currentCulture.Name.StartsWith("fr") ? "Rotation auto désinstallée et tous les fichiers supprimés." : "Auto rotation uninstalled and all files removed.",
                "TestCompleted" => _currentCulture.Name.StartsWith("fr") ? "Test effectué (script + restart xochitl)." : "Test completed (script + restart xochitl).",
                
                // Process messages
                "Applying" => _currentCulture.Name.StartsWith("fr") ? "Application en cours..." : "Applying...",
                "Restoring" => _currentCulture.Name.StartsWith("fr") ? "Restauration..." : "Restoring...",
                "LoadingGallery" => _currentCulture.Name.StartsWith("fr") ? "Chargement de la galerie..." : "Loading gallery...",
                "GalleryLoaded" => _currentCulture.Name.StartsWith("fr") ? "Galerie chargée" : "Gallery loaded",
                "GalleryError" => _currentCulture.Name.StartsWith("fr") ? "Erreur galerie" : "Gallery error",
                "Downloading" => _currentCulture.Name.StartsWith("fr") ? "Téléchargement..." : "Downloading...",
                "InstallingFromGallery" => _currentCulture.Name.StartsWith("fr") ? "Installation depuis galerie..." : "Installing from gallery...",
                "UploadingImages" => _currentCulture.Name.StartsWith("fr") ? "Upload des images..." : "Uploading images...",
                "InstallingRotation" => _currentCulture.Name.StartsWith("fr") ? "Installation rotation (hook)..." : "Installing rotation (hook)...",
                "UninstallingRotation" => _currentCulture.Name.StartsWith("fr") ? "Désinstallation rotation..." : "Uninstalling rotation...",
                "TestingRotation" => _currentCulture.Name.StartsWith("fr") ? "Test rotation..." : "Testing rotation...",
                
                _ => key
            };
        }

        public static void SetLanguage(string languageCode)
        {
            try
            {
                CurrentCulture = new CultureInfo(languageCode);
            }
            catch (CultureNotFoundException)
            {
                // Fallback to default culture
                CurrentCulture = CultureInfo.CurrentCulture;
            }
        }

        public static string[] GetAvailableLanguages()
        {
            return new[] { "en-US", "fr-FR" };
        }
    }
}

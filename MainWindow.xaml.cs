using Microsoft.Win32;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Text;
using WF = System.Windows.Forms; // alias FolderBrowserDialog
using Win32OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfMessageBox = System.Windows.MessageBox;
using RemarkableSleepScreenManager.Localization;


namespace RemarkableSleepScreenManager
{
    public partial class MainWindow : Window
    {
        private string? _imagePath;
        
        // Méthode pour charger l'image depuis les ressources embarquées
        private static string GetEmbeddedImagePath()
        {
            // Créer un fichier temporaire avec l'image embarquée
            var tempPath = Path.Combine(Path.GetTempPath(), "suspended_embedded.png");
            
            // Si le fichier temporaire n'existe pas, le recréer
            if (!File.Exists(tempPath))
            {
                using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RemarkableSleepScreenManager.Assets.suspended.png");
                if (resourceStream != null)
                {
                    using var fileStream = File.Create(tempPath);
                    resourceStream.CopyTo(fileStream);
                }
            }
            
            return tempPath;
        }
        
        // juste sous `private string? _imagePath;`
        private static readonly string BundledOriginal = GetEmbeddedImagePath();

        // --- Galerie: modèles & client HTTP ---
        public class GalleryCatalog { public string? Updated { get; set; } public List<GalleryItem> Items { get; set; } = new(); }
        public class GalleryItem
        {
            public string Id { get; set; } = "";
            public string Title { get; set; } = "";
            public string Author { get; set; } = "";
            public string License { get; set; } = "";
            public string Device { get; set; } = "paperpro";
            public string Resolution { get; set; } = "2160x1620";
            public string preview_url { get; set; } = "";
            public string download_url { get; set; } = "";
            public List<string> Tags { get; set; } = new();
        }

        private static readonly HttpClient _http = new HttpClient();
        // URL de ton JSON servi par GitHub Pages via /docs
        private const string GalleryIndexUrl =
            "https://roropastis.github.io/ReMarkable-Sleep-Screen-Manager/gallery/index.json";

        private readonly ObservableCollection<GalleryItem> _galleryItems = new();

        // Script côté tablette : choisit un PNG aléatoire et le copie en suspended.png (avec log)
        private const string ChangeSleepSh = @"#!/bin/sh
set -e
FOLDER=/home/root/screens
DEST=/usr/share/remarkable/suspended.png
LOG=/home/root/change-sleep.log

echo ""$(date '+%F %T') - start"" >> ""$LOG""

set -- ""$FOLDER""/*.png
[ -e ""$1"" ] || { echo ""no png in $FOLDER"" >> ""$LOG""; exit 0; }

count=$#
idx=$((RANDOM % count + 1))
file=$(eval echo \${$idx})

mount -o remount,rw /
cp ""$file"" ""$DEST""

echo ""$(date '+%F %T') - set $(basename ""$file"")"" >> ""$LOG""
exit 0
";

        // Drop-in systemd : remplace ExecStart pour exécuter d’abord le script, puis xochitl
        private const string XochitlDropIn = @"[Service]
ExecStart=
ExecStart=/bin/sh -c '/home/root/change-sleep.sh; exec /usr/bin/xochitl --system'
";


        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize language selector
            InitializeLanguageSelector();
            
            // Subscribe to language changes
            ResourceManager.CultureChanged += OnResourceManagerCultureChanged;
            
            // Initialize UI texts
            UpdateUITexts();
        }

        private void InitializeLanguageSelector()
        {
            // Set French as default language
            ResourceManager.SetLanguage("fr-FR");
            
            // Set French as selected in the combo box
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == "fr-FR")
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void OnResourceManagerCultureChanged(object sender, EventArgs e)
        {
            // Update UI texts when language changes
            UpdateUITexts();
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem && 
                selectedItem.Tag is string languageCode)
            {
                ResourceManager.SetLanguage(languageCode);
            }
        }

        private void UpdateUITexts()
        {
            // Update main window title
            Title = ResourceManager.GetString("AppTitle");
            
            // Update header
            HeaderText.Text = ResourceManager.GetString("AppTitle");
            
            // Update status bar
            StatusText.Text = ResourceManager.GetString("Ready");
            
            // Update connection group box
            ConnectionGroupBox.Header = ResourceManager.GetString("Connection");
            
            // Update tab headers
            LocalTab.Header = ResourceManager.GetString("Local");
            AutoRotationTab.Header = ResourceManager.GetString("AutoRotation");
            GalleryTab.Header = ResourceManager.GetString("Gallery");
            
            // Update descriptions
            AutoRotationDescription.Text = ResourceManager.GetString("AutoRotationDescription");
            GalleryDescription.Text = ResourceManager.GetString("BrowseGallery");
            
            // Update buttons and labels
            ChooseImageButton.Content = ResourceManager.GetString("ChooseImageButton");
            AutoResizeBox.Content = ResourceManager.GetString("AutoResizeCheckBox");
            UploadApplyButton.Content = ResourceManager.GetString("UploadApplyButton");
            RestoreDefaultButton.Content = ResourceManager.GetString("RestoreDefaultButton");
            OutputLogLabel.Text = ResourceManager.GetString("OutputLogLabel");
            
            ChooseFolderButton.Content = ResourceManager.GetString("ChooseFolderButton");
            UploadFolderButton.Content = ResourceManager.GetString("UploadFolderButton");
            InstallButton.Content = ResourceManager.GetString("InstallButton");
            UninstallButton.Content = ResourceManager.GetString("UninstallButton");
            TestNowButton.Content = ResourceManager.GetString("TestNowButton");
            RotationLogLabel.Text = ResourceManager.GetString("Log");
            
            // Update connection labels
            IpAddressLabel.Text = ResourceManager.GetString("IpAddressLabel");
            UsernameLabel.Text = ResourceManager.GetString("UsernameLabel");
            PasswordLabel.Text = ResourceManager.GetString("PasswordLabel");
            TestButton.Content = ResourceManager.GetString("Test");
            
            // Update language label
            LanguageLabel.Text = ResourceManager.GetString("Language") + ":";
            
            // Update copyright
            CopyrightText.Text = ResourceManager.GetString("Copyright");
            
            // Update gallery button texts if gallery is loaded
            if (GalleryList.Items.Count > 0)
            {
                UpdateGalleryButtonTexts();
            }
        }

        // Helpers UI
        private void Log(string s)
        {
            LogBox.AppendText($"{DateTime.Now:HH:mm:ss}  {s}\n");
            LogBox.ScrollToEnd();
        }
        private void SetStatus(string s) => StatusText.Text = s;

        //installation de la rotation automatique

        private void UploadText(SftpClient sftp, string remotePath, string content)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            sftp.UploadFile(ms, remotePath, true);
        }

        private void EnsureDir(SftpClient sftp, string path)
        {
            if (sftp.Exists(path)) return;
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string cur = "";
            foreach (var p in parts)
            {
                cur += "/" + p;
                if (!sftp.Exists(cur)) sftp.CreateDirectory(cur);
            }
        }


        // Parcourir image
        private void OnBrowseImage(object sender, RoutedEventArgs e)
        {
            var dlg = new Win32OpenFileDialog { Title = "Choisir une image PNG", Filter = "PNG (*.png)|*.png" };
            if (dlg.ShowDialog() == true)
            {
                _imagePath = dlg.FileName;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(_imagePath);
                    bmp.EndInit();
                    Preview.Source = bmp;
                    Log($"Image sélectionnée: {_imagePath}");
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(ex.Message, "Lecture image", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Test de connexion (uname -a)
        private async void OnTestClick(object sender, RoutedEventArgs e)
        {
            // SNAPSHOT UI
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Test de connexion...");
                await Task.Run(() =>
                {
                    using var client = CreateSshClient(host, user, pass);
                    client.Connect();
                    var resp = client.RunCommand("uname -a");
                    Dispatcher.Invoke(() => Log($"Connecté. {resp.Result.Trim()}"));
                    client.Disconnect();
                });

                SetStatus("OK");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Connexion", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // Uploader & Appliquer
        private async void OnApplyClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_imagePath))
            {
                WpfMessageBox.Show("Choisis une image PNG.");
                return;
            }

            // SNAPSHOT UI
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;
            var autoResize = AutoResizeBox.IsChecked == true;

            try
            {
                SetStatus("Application en cours...");

                var tmp = _imagePath;
                if (autoResize)
                {
                    tmp = await Task.Run(() => ImageUtil.ResizeTo2160x1620(_imagePath!));
                    Log($"Image redimensionnée → {tmp}");
                }

                await Task.Run(() =>
                {
                    using (var sftp = CreateSftpClient(host, user, pass))
                    {
                        sftp.Connect();
                        using var fs = File.OpenRead(tmp);
                        sftp.UploadFile(fs, "/home/root/suspended.png", true);
                        sftp.Disconnect();
                    }

                    using var ssh = CreateSshClient(host, user, pass);
                    ssh.Connect();
                    string cmd = "mount -o remount,rw / && " +
                                 "mv /home/root/suspended.png /usr/share/remarkable/suspended.png && " +
                                 "systemctl restart xochitl";
                    var r = ssh.RunCommand(cmd);
                    Dispatcher.Invoke(() => Log(r.Result + r.Error));
                    ssh.Disconnect();
                });


                SetStatus("Terminé ✔");
                Log("Mets la tablette en veille pour voir l’écran.");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Uploader & Appliquer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Restaurer le backup
        private async void OnRestoreClick(object sender, RoutedEventArgs e)
        {
            // SNAPSHOT UI
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Restauration...");
                if (!File.Exists(BundledOriginal))
                {
                    WpfMessageBox.Show(
                        $"Fichier original introuvable : {BundledOriginal}\n" +
                        "Ajoute Assets\\suspended_original.png au projet (Content, Copy always).",
                        "Restore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetStatus("Prêt");
                    return;
                }

                await Task.Run(() =>
                {
                    using (var sftp = CreateSftpClient(host, user, pass))
                    {
                        sftp.Connect();
                        using var fs = File.OpenRead(BundledOriginal);
                        sftp.UploadFile(fs, "/home/root/suspended.png", true);
                        sftp.Disconnect();
                    }

                    using var ssh = CreateSshClient(host, user, pass);
                    ssh.Connect();
                    var r = ssh.RunCommand(
                        "mount -o remount,rw / && " +
                        "mv /home/root/suspended.png /usr/share/remarkable/suspended.png && " +
                        "systemctl restart xochitl");
                    Dispatcher.Invoke(() => Log(r.Result + r.Error));
                    ssh.Disconnect();
                });


                SetStatus("Restauré ✔");
                Log("L’écran d’origine (bundled) a été restauré.");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Restore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Fabrique clients SSH/SFTP (mot de passe ou clé)
        private SshClient CreateSshClient(string host, string user, string? pass)
        {
            var client = new SshClient(host, user, pass);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        private SftpClient CreateSftpClient(string host, string user, string? pass)
        {
            var client = new SftpClient(host, 22, user, pass);
            client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(10);
            return client;
        }

        private async void OnGalleryTabLoaded(object sender, RoutedEventArgs e)
        {
            if (GalleryList.Items.Count > 0) return; // déjà chargé
            try
            {
                SetStatus("Chargement de la galerie...");
                var json = await _http.GetStringAsync(GalleryIndexUrl);
                var cat = JsonSerializer.Deserialize<GalleryCatalog>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _galleryItems.Clear();
                foreach (var it in cat?.Items ?? new()) _galleryItems.Add(it);
                GalleryList.ItemsSource = _galleryItems;
                SetStatus("Galerie chargée");
                
                // Update gallery button texts after loading
                UpdateGalleryButtonTexts();
            }
            catch (Exception ex)
            {
                SetStatus("Erreur galerie");
                WpfMessageBox.Show(ex.Message, "Chargement galerie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateGalleryButtonTexts()
        {
            // Update gallery button texts in the template
            foreach (var item in GalleryList.Items)
            {
                var container = GalleryList.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.ListViewItem;
                if (container != null)
                {
                    var border = FindVisualChild<Border>(container, "Tile");
                    if (border != null)
                    {
                        var downloadButton = FindVisualChild<System.Windows.Controls.Button>(border, "DownloadButton");
                        var installButton = FindVisualChild<System.Windows.Controls.Button>(border, "InstallButton");
                        
                        if (downloadButton != null)
                            downloadButton.Content = ResourceManager.GetString("DownloadButton");
                        if (installButton != null)
                            installButton.Content = ResourceManager.GetString("InstallFromGalleryButton");
                    }
                }
            }
        }

        private T? FindVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t && (child as FrameworkElement)?.Name == name)
                    return t;
                
                var childOfChild = FindVisualChild<T>(child, name);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private async void OnDownloadFromGallery(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not GalleryItem item) return;
            try
            {
                SetStatus("Téléchargement...");
                var bytes = await _http.GetByteArrayAsync(item.download_url);
                var tmp = Path.Combine(Path.GetTempPath(), $"rm_{item.Id}_{Guid.NewGuid():N}.png");
                await File.WriteAllBytesAsync(tmp, bytes);
                Log($"Téléchargé → {tmp}");
                SetStatus("Téléchargé ✔");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Télécharger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnInstallFromGallery(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not GalleryItem item) return;

            // Snapshot UI (ne pas lire les contrôles depuis Task.Run)
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Installation depuis galerie...");

                // 1) Télécharger l'image
                var bytes = await _http.GetByteArrayAsync(item.download_url);
                var tmp = Path.Combine(Path.GetTempPath(), $"rm_{item.Id}_{Guid.NewGuid():N}.png");
                await File.WriteAllBytesAsync(tmp, bytes);

                // 2) Redimensionner si nécessaire (Paper Pro attendu 2160x1620)
                var final = tmp;
                if (item.Device.Equals("paperpro", StringComparison.OrdinalIgnoreCase) && !string.Equals(item.Resolution, "2160x1620", StringComparison.OrdinalIgnoreCase))
                    final = await Task.Run(() => ImageUtil.ResizeTo2160x1620(tmp));

                // 3) Upload + mv + restart (même logique que OnApplyClick)
                await Task.Run(() =>
                {
                    using (var sftp = CreateSftpClient(host, user, pass))
                    {
                        sftp.Connect();
                        using var fs = File.OpenRead(final);
                        sftp.UploadFile(fs, "/home/root/suspended.png", true);
                        sftp.Disconnect();
                    }

                    using var ssh = CreateSshClient(host, user, pass);
                    ssh.Connect();
                    var r = ssh.RunCommand(
                        "mount -o remount,rw / && " +
                        "mv /home/root/suspended.png /usr/share/remarkable/suspended.png && " +
                        "systemctl restart xochitl");
                    Dispatcher.Invoke(() => Log(r.Result + r.Error));
                    ssh.Disconnect();
                });

                SetStatus("Installé ✔");
                Log($"Installé depuis galerie: {item.Title}");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Installer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBrowseScreensFolder(object sender, RoutedEventArgs e)
        {
            var dlg = new WF.FolderBrowserDialog { Description = "Choisir un dossier contenant des PNG (1620x2160 recommandé)" };
            if (dlg.ShowDialog() == WF.DialogResult.OK)
            {
                ScreensFolderBox.Text = dlg.SelectedPath;
                RotationLogBox.AppendText($"Dossier sélectionné: {dlg.SelectedPath}\n");
                RotationLogBox.ScrollToEnd();
            }
        }

        private async void OnUploadScreensClick(object sender, RoutedEventArgs e)
        {
            var local = ScreensFolderBox.Text.Trim();
            if (string.IsNullOrEmpty(local) || !Directory.Exists(local))
            {
                WpfMessageBox.Show("Choisis un dossier d’images PNG.");
                return;
            }

            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Upload des images...");
                await Task.Run(() =>
                {
                    using var sftp = CreateSftpClient(host, user, pass);
                    sftp.Connect();
                    EnsureDir(sftp, "/home/root/screens");

                    foreach (var path in Directory.GetFiles(local, "*.png"))
                    {
                        using var fs = File.OpenRead(path);
                        var remote = "/home/root/screens/" + Path.GetFileName(path);
                        sftp.UploadFile(fs, remote, true);
                        Dispatcher.Invoke(() =>
                        {
                            RotationLogBox.AppendText($"Upload: {Path.GetFileName(path)}\n");
                            RotationLogBox.ScrollToEnd();
                        });
                    }
                    sftp.Disconnect();
                });
                SetStatus("OK");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Uploader dossier", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnInstallRotationHook(object sender, RoutedEventArgs e)
        {
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Installation rotation (hook)...");
                await Task.Run(() =>
                {
                    using var sftp = CreateSftpClient(host, user, pass);
                    sftp.Connect();
                    // script
                    UploadText(sftp, "/home/root/change-sleep.sh", ChangeSleepSh);
                    // drop-in
                    EnsureDir(sftp, "/etc/systemd/system/xochitl.service.d");
                    UploadText(sftp, "/etc/systemd/system/xochitl.service.d/change-sleep.conf", XochitlDropIn);
                    sftp.Disconnect();

                    using var ssh = CreateSshClient(host, user, pass);
                    ssh.Connect();
                    // droits + CRLF -> LF
                    ssh.RunCommand("chmod +x /home/root/change-sleep.sh");
                    ssh.RunCommand("sed -i 's/\\r$//' /home/root/change-sleep.sh");
                    // reload + restart
                    ssh.RunCommand("systemctl daemon-reexec");
                    ssh.RunCommand("systemctl restart xochitl");
                    ssh.Disconnect();
                });

                SetStatus("Rotation installée ✔");
                RotationLogBox.AppendText("Rotation auto (hook) installée. Visible au deep sleep (~12 min).\n");
                RotationLogBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Installer rotation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnUninstallRotation(object sender, RoutedEventArgs e)
        {
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Désinstallation rotation...");
                await Task.Run(() =>
                {
                    using var ssh = CreateSshClient(host, user, pass);
                    ssh.Connect();

                    // Supprimer drop-in systemd
                    ssh.RunCommand("rm -f /etc/systemd/system/xochitl.service.d/change-sleep.conf");

                    // Supprimer script
                    ssh.RunCommand("rm -f /home/root/change-sleep.sh");

                    // Supprimer log
                    ssh.RunCommand("rm -f /home/root/change-sleep.log");

                    // Supprimer dossier screens et son contenu (png)
                    ssh.RunCommand("rm -rf /home/root/screens");

                    // Reload systemd et restart xochitl
                    ssh.RunCommand("systemctl daemon-reexec");
                    ssh.RunCommand("systemctl restart xochitl");

                    ssh.Disconnect();
                });
                SetStatus("Désinstallé ✔");
                RotationLogBox.AppendText("Rotation auto désinstallée et tous les fichiers supprimés.\n");
                RotationLogBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Désinstaller rotation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void OnTestRotationNow(object sender, RoutedEventArgs e)
        {
            var host = IpBox.Text.Trim();
            var user = UserBox.Text.Trim();
            var pass = PassBox.Password;

            try
            {
                SetStatus("Test rotation...");
                await Task.Run(() =>
                {
                    using var ssh = CreateSshClient(host, user, pass);
                    ssh.Connect();
                    ssh.RunCommand("/home/root/change-sleep.sh");
                    ssh.RunCommand("systemctl restart xochitl");
                    ssh.Disconnect();
                });
                SetStatus("OK");
                RotationLogBox.AppendText("Test effectué (script + restart xochitl).\n");
                RotationLogBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                WpfMessageBox.Show(ex.Message, "Tester rotation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Util redimensionnement simple (System.Drawing.Windows-only)
        internal static class ImageUtil
        {
            // Redimensionne en PNG 1620x2160 portrait ; renvoie un chemin temp.
            public static string ResizeTo2160x1620(string path)
            {
                using var src = System.Drawing.Image.FromFile(path);
                // ← dimensions correctes (largeur 1620, hauteur 2160)
                using var bmp = new Bitmap(1620, 2160);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.FillRectangle(System.Drawing.Brushes.White, 0, 0, 1620, 2160);

                    var ratio = Math.Min(1620f / src.Width, 2160f / src.Height);
                    var w = (int)(src.Width * ratio);
                    var h = (int)(src.Height * ratio);
                    var x = (1620 - w) / 2;
                    var y = (2160 - h) / 2;
                    g.DrawImage(src, x, y, w, h);
                }
                var tmp = Path.Combine(Path.GetTempPath(), $"rm_suspended_{Guid.NewGuid():N}.png");
                bmp.Save(tmp, ImageFormat.Png);
                return tmp;
            }

        }

    }
}

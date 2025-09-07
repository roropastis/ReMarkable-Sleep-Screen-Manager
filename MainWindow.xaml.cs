using Microsoft.Win32;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text.Json;
using System.Collections.ObjectModel;


namespace RemarkableSleepScreenManager
{
    public partial class MainWindow : Window
    {
        private string? _imagePath;
        // juste sous `private string? _imagePath;`
        private static readonly string BundledOriginal =
            System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "suspended.png");

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
            public string PreviewUrl { get; set; } = "";
            public string DownloadUrl { get; set; } = "";
            public List<string> Tags { get; set; } = new();
        }

        private static readonly HttpClient _http = new HttpClient();
        // URL de ton JSON servi par GitHub Pages via /docs
        private const string GalleryIndexUrl =
            "https://roropastis.github.io/ReMarkable-Sleep-Screen-Manager/gallery/index.json";

        private readonly ObservableCollection<GalleryItem> _galleryItems = new();



        public MainWindow()
        {
            InitializeComponent();

        }

        // Helpers UI
        private void Log(string s)
        {
            LogBox.AppendText($"{DateTime.Now:HH:mm:ss}  {s}\n");
            LogBox.ScrollToEnd();
        }
        private void SetStatus(string s) => StatusText.Text = s;


        // Parcourir image
        private void OnBrowseImage(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Title = "Choisir une image PNG", Filter = "PNG (*.png)|*.png" };
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
                    MessageBox.Show(ex.Message, "Lecture image", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(ex.Message, "Connexion", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // Uploader & Appliquer
        private async void OnApplyClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_imagePath))
            {
                MessageBox.Show("Choisis une image PNG.");
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
                MessageBox.Show(ex.Message, "Uploader & Appliquer", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show(
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
                MessageBox.Show(ex.Message, "Restore", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
            catch (Exception ex)
            {
                SetStatus("Erreur galerie");
                MessageBox.Show(ex.Message, "Chargement galerie", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnDownloadFromGallery(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not GalleryItem item) return;
            try
            {
                SetStatus("Téléchargement...");
                var bytes = await _http.GetByteArrayAsync(item.DownloadUrl);
                var tmp = Path.Combine(Path.GetTempPath(), $"rm_{item.Id}_{Guid.NewGuid():N}.png");
                await File.WriteAllBytesAsync(tmp, bytes);
                Log($"Téléchargé → {tmp}");
                SetStatus("Téléchargé ✔");
            }
            catch (Exception ex)
            {
                SetStatus("Erreur");
                MessageBox.Show(ex.Message, "Télécharger", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var bytes = await _http.GetByteArrayAsync(item.DownloadUrl);
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
                MessageBox.Show(ex.Message, "Installer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Util redimensionnement simple (System.Drawing.Windows-only)
        internal static class ImageUtil
        {
            // Redimensionne en PNG 2160x1620 avec fond blanc ; renvoie un chemin temp.
            public static string ResizeTo2160x1620(string path)
            {
                using var src = Image.FromFile(path);
                using var bmp = new Bitmap(2160, 1620);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.FillRectangle(Brushes.White, 0, 0, 2160, 1620);

                    var ratio = Math.Min(2160f / src.Width, 1620f / src.Height);
                    var w = (int)(src.Width * ratio);
                    var h = (int)(src.Height * ratio);
                    var x = (2160 - w) / 2;
                    var y = (1620 - h) / 2;
                    g.DrawImage(src, x, y, w, h);
                }
                var tmp = Path.Combine(Path.GetTempPath(), $"rm_suspended_{Guid.NewGuid():N}.png");
                bmp.Save(tmp, ImageFormat.Png);
                return tmp;
            }
        }
    }
}

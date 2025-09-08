using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RemarkableSleepScreenManager.Localization;
using RemarkableSleepScreenManager.Models;
using RemarkableSleepScreenManager.Services;
using RemarkableSleepScreenManager.Commands;
using WF = System.Windows.Forms;
using WpfMessageBox = System.Windows.MessageBox;
using Win32OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Reflection;

namespace RemarkableSleepScreenManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ISshService _sshService;
        private readonly IImageService _imageService;
        private readonly IGalleryService _galleryService;
        private readonly ConnectionSettings _connectionSettings;

        private string _imagePath = "";
        private BitmapImage? _previewImage;
        private string _statusText = "";
        private string _logText = "";
        private string _rotationLogText = "";
        private string _screensFolderPath = "";
        private bool _autoResize = true;
        private bool _isLoading = false;

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

        public MainViewModel(ISshService sshService, IImageService imageService, IGalleryService galleryService)
        {
            _sshService = sshService;
            _imageService = imageService;
            _galleryService = galleryService;
            _connectionSettings = new ConnectionSettings();
            
            StatusText = ResourceManager.GetString("Ready");
            
            // Initialize commands
            TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync());
            BrowseImageCommand = new RelayCommand(BrowseImage);
            ApplyImageCommand = new RelayCommand(async () => await ApplyImageAsync());
            RestoreDefaultCommand = new RelayCommand(async () => await RestoreDefaultAsync());
            BrowseScreensFolderCommand = new RelayCommand(BrowseScreensFolder);
            UploadScreensCommand = new RelayCommand(async () => await UploadScreensAsync());
            InstallRotationCommand = new RelayCommand(async () => await InstallRotationAsync());
            UninstallRotationCommand = new RelayCommand(async () => await UninstallRotationAsync());
            TestRotationCommand = new RelayCommand(async () => await TestRotationAsync());
            DownloadFromGalleryCommand = new RelayCommand<GalleryItem>(async (item) => await DownloadFromGalleryAsync(item));
            InstallFromGalleryCommand = new RelayCommand<GalleryItem>(async (item) => await InstallFromGalleryAsync(item));
            LoadGalleryCommand = new RelayCommand(async () => await LoadGalleryAsync());
            SetLanguageCommand = new RelayCommand<string>(SetLanguage);

            // Initialize gallery
            GalleryItems = _galleryService.GalleryItems;
        }

        public ConnectionSettings ConnectionSettings => _connectionSettings;
        public ObservableCollection<GalleryItem> GalleryItems { get; }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage? PreviewImage
        {
            get => _previewImage;
            set
            {
                if (_previewImage != value)
                {
                    _previewImage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LogText
        {
            get => _logText;
            set
            {
                if (_logText != value)
                {
                    _logText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RotationLogText
        {
            get => _rotationLogText;
            set
            {
                if (_rotationLogText != value)
                {
                    _rotationLogText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ScreensFolderPath
        {
            get => _screensFolderPath;
            set
            {
                if (_screensFolderPath != value)
                {
                    _screensFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoResize
        {
            get => _autoResize;
            set
            {
                if (_autoResize != value)
                {
                    _autoResize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands
        public RelayCommand TestConnectionCommand { get; }
        public RelayCommand BrowseImageCommand { get; }
        public RelayCommand ApplyImageCommand { get; }
        public RelayCommand RestoreDefaultCommand { get; }
        public RelayCommand BrowseScreensFolderCommand { get; }
        public RelayCommand UploadScreensCommand { get; }
        public RelayCommand InstallRotationCommand { get; }
        public RelayCommand UninstallRotationCommand { get; }
        public RelayCommand TestRotationCommand { get; }
        public RelayCommand<GalleryItem> DownloadFromGalleryCommand { get; }
        public RelayCommand<GalleryItem> InstallFromGalleryCommand { get; }
        public RelayCommand LoadGalleryCommand { get; }
        public RelayCommand<string> SetLanguageCommand { get; }

        private async Task TestConnectionAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = ResourceManager.GetString("TestingConnection");
                
                var success = await _sshService.TestConnectionAsync(_connectionSettings);
                if (success)
                {
                    StatusText = ResourceManager.GetString("Connected");
                    Log(ResourceManager.GetString("ConnectionTestSuccessful"));
                }
                else
                {
                    StatusText = ResourceManager.GetString("ConnectionFailed");
                    Log(ResourceManager.GetString("ConnectionTestFailed"));
                }
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("ConnectionError")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BrowseImage()
        {
            var dialog = new Win32OpenFileDialog
            {
                Title = "Choose a PNG image",
                Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                ImagePath = dialog.FileName;
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(ImagePath);
                    bitmap.EndInit();
                    PreviewImage = bitmap;
                    Log($"{ResourceManager.GetString("ImageSelected")}: {ImagePath}");
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(ex.Message, "Image Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ApplyImageAsync()
        {
            if (string.IsNullOrEmpty(ImagePath))
            {
                WpfMessageBox.Show("Please select an image first.");
                return;
            }

            try
            {
                IsLoading = true;
                StatusText = "Applying image...";

                var imageToUpload = ImagePath;
                if (AutoResize)
                {
                    imageToUpload = await _imageService.ResizeToPaperProAsync(ImagePath);
                    Log($"{ResourceManager.GetString("ImageResized")} → {imageToUpload}");
                }

                await _sshService.UploadFileAsync(_connectionSettings, imageToUpload, "/home/root/suspended.png");
                
                var command = "mount -o remount,rw / && " +
                             "mv /home/root/suspended.png /usr/share/remarkable/suspended.png && " +
                             "systemctl restart xochitl";
                
                var result = await _sshService.ExecuteCommandAsync(_connectionSettings, command);
                Log(result);

                StatusText = ResourceManager.GetString("Completed");
                Log(ResourceManager.GetString("PutTabletToSleep"));
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Upload & Apply Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RestoreDefaultAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Restoring...";

                var bundledOriginal = GetEmbeddedImagePath();
                if (!File.Exists(bundledOriginal))
                {
                    WpfMessageBox.Show(
                        $"Embedded image could not be extracted: {bundledOriginal}\n" +
                        "The application may be corrupted.",
                        "Restore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusText = "Ready";
                    return;
                }

                await _sshService.UploadFileAsync(_connectionSettings, bundledOriginal, "/home/root/suspended.png");
                
                var command = "mount -o remount,rw / && " +
                             "mv /home/root/suspended.png /usr/share/remarkable/suspended.png && " +
                             "systemctl restart xochitl";
                
                var result = await _sshService.ExecuteCommandAsync(_connectionSettings, command);
                Log(result);

                StatusText = ResourceManager.GetString("Restored");
                Log(ResourceManager.GetString("OriginalScreenRestored"));
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BrowseScreensFolder()
        {
            var dialog = new WF.FolderBrowserDialog
            {
                Description = "Choose a folder containing PNG files (1620x2160 recommended)"
            };

            if (dialog.ShowDialog() == WF.DialogResult.OK)
            {
                ScreensFolderPath = dialog.SelectedPath;
                RotationLog($"{ResourceManager.GetString("FolderSelected")}: {dialog.SelectedPath}");
            }
        }

        private async Task UploadScreensAsync()
        {
            if (string.IsNullOrEmpty(ScreensFolderPath) || !Directory.Exists(ScreensFolderPath))
            {
                WpfMessageBox.Show("Please select a folder containing PNG images.");
                return;
            }

            try
            {
                IsLoading = true;
                StatusText = "Uploading images...";

                await _sshService.EnsureDirectoryExistsAsync(_connectionSettings, "/home/root/screens");

                var pngFiles = Directory.GetFiles(ScreensFolderPath, "*.png");
                foreach (var file in pngFiles)
                {
                    var remotePath = "/home/root/screens/" + Path.GetFileName(file);
                    await _sshService.UploadFileAsync(_connectionSettings, file, remotePath);
                    RotationLog($"{ResourceManager.GetString("Uploaded")}: {Path.GetFileName(file)}");
                }

                StatusText = ResourceManager.GetString("OK");
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Upload Folder Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task InstallRotationAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Installing rotation (hook)...";

                const string changeSleepSh = @"#!/bin/sh
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
exit 0";

                const string xochitlDropIn = @"[Service]
ExecStart=
ExecStart=/bin/sh -c '/home/root/change-sleep.sh; exec /usr/bin/xochitl --system'";

                await _sshService.UploadTextAsync(_connectionSettings, changeSleepSh, "/home/root/change-sleep.sh");
                await _sshService.EnsureDirectoryExistsAsync(_connectionSettings, "/etc/systemd/system/xochitl.service.d");
                await _sshService.UploadTextAsync(_connectionSettings, xochitlDropIn, "/etc/systemd/system/xochitl.service.d/change-sleep.conf");

                var commands = new[]
                {
                    "chmod +x /home/root/change-sleep.sh",
                    "sed -i 's/\\r$//' /home/root/change-sleep.sh",
                    "systemctl daemon-reexec",
                    "systemctl restart xochitl"
                };

                foreach (var cmd in commands)
                {
                    await _sshService.ExecuteCommandAsync(_connectionSettings, cmd);
                }

                StatusText = ResourceManager.GetString("RotationInstalled");
                RotationLog(ResourceManager.GetString("AutoRotationInstalled"));
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Install Rotation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UninstallRotationAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Uninstalling rotation...";

                var commands = new[]
                {
                    "rm -f /etc/systemd/system/xochitl.service.d/change-sleep.conf",
                    "rm -f /home/root/change-sleep.sh",
                    "rm -f /home/root/change-sleep.log",
                    "rm -rf /home/root/screens",
                    "systemctl daemon-reexec",
                    "systemctl restart xochitl"
                };

                foreach (var cmd in commands)
                {
                    await _sshService.ExecuteCommandAsync(_connectionSettings, cmd);
                }

                StatusText = ResourceManager.GetString("Uninstalled");
                RotationLog(ResourceManager.GetString("AutoRotationUninstalled"));
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Uninstall Rotation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task TestRotationAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Testing rotation...";

                await _sshService.ExecuteCommandAsync(_connectionSettings, "/home/root/change-sleep.sh");
                await _sshService.ExecuteCommandAsync(_connectionSettings, "systemctl restart xochitl");

                StatusText = ResourceManager.GetString("OK");
                RotationLog(ResourceManager.GetString("TestCompleted"));
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Test Rotation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DownloadFromGalleryAsync(GalleryItem? item)
        {
            if (item == null) return;

            try
            {
                IsLoading = true;
                StatusText = ResourceManager.GetString("Downloading");

                var tempPath = await _imageService.DownloadImageAsync(item.DownloadUrl);
                Log($"{ResourceManager.GetString("DownloadedTo")} → {tempPath}");
                StatusText = ResourceManager.GetString("Downloaded");
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task InstallFromGalleryAsync(GalleryItem? item)
        {
            if (item == null) return;

            try
            {
                IsLoading = true;
                StatusText = "Installing from gallery...";

                // Download image
                var tempPath = await _imageService.DownloadImageAsync(item.DownloadUrl);

                // Resize if necessary
                var finalPath = tempPath;
                if (item.Device.Equals("paperpro", StringComparison.OrdinalIgnoreCase) && 
                    !string.Equals(item.Resolution, "2160x1620", StringComparison.OrdinalIgnoreCase))
                {
                    finalPath = await _imageService.ResizeToPaperProAsync(tempPath);
                }

                // Upload and apply
                await _sshService.UploadFileAsync(_connectionSettings, finalPath, "/home/root/suspended.png");
                
                var command = "mount -o remount,rw / && " +
                             "mv /home/root/suspended.png /usr/share/remarkable/suspended.png && " +
                             "systemctl restart xochitl";
                
                var result = await _sshService.ExecuteCommandAsync(_connectionSettings, command);
                Log(result);

                StatusText = ResourceManager.GetString("Installed");
                Log($"{ResourceManager.GetString("InstalledFromGallery")}: {item.Title}");
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("Error");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadGalleryAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = ResourceManager.GetString("LoadingGallery");
                
                await _galleryService.LoadGalleryAsync();
                StatusText = ResourceManager.GetString("GalleryLoaded");
            }
            catch (Exception ex)
            {
                StatusText = ResourceManager.GetString("GalleryError");
                Log($"{ResourceManager.GetString("Error")}: {ex.Message}");
                WpfMessageBox.Show(ex.Message, "Gallery Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetLanguage(string languageCode)
        {
            ResourceManager.SetLanguage(languageCode);
        }

        private void Log(string message)
        {
            LogText += $"{DateTime.Now:HH:mm:ss}  {message}\n";
        }

        private void RotationLog(string message)
        {
            RotationLogText += $"{DateTime.Now:HH:mm:ss}  {message}\n";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

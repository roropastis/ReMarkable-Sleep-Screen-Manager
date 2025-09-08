using System.ComponentModel;

namespace RemarkableSleepScreenManager.Models
{
    public class GalleryItem : INotifyPropertyChanged
    {
        private string _id = "";
        private string _title = "";
        private string _author = "";
        private string _license = "";
        private string _device = "paperpro";
        private string _resolution = "2160x1620";
        private string _previewUrl = "";
        private string _downloadUrl = "";
        private List<string> _tags = new();

        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                if (_author != value)
                {
                    _author = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }

        public string License
        {
            get => _license;
            set
            {
                if (_license != value)
                {
                    _license = value;
                    OnPropertyChanged(nameof(License));
                }
            }
        }

        public string Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged(nameof(Device));
                }
            }
        }

        public string Resolution
        {
            get => _resolution;
            set
            {
                if (_resolution != value)
                {
                    _resolution = value;
                    OnPropertyChanged(nameof(Resolution));
                }
            }
        }

        public string PreviewUrl
        {
            get => _previewUrl;
            set
            {
                if (_previewUrl != value)
                {
                    _previewUrl = value;
                    OnPropertyChanged(nameof(PreviewUrl));
                }
            }
        }

        public string DownloadUrl
        {
            get => _downloadUrl;
            set
            {
                if (_downloadUrl != value)
                {
                    _downloadUrl = value;
                    OnPropertyChanged(nameof(DownloadUrl));
                }
            }
        }

        public List<string> Tags
        {
            get => _tags;
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged(nameof(Tags));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GalleryCatalog
    {
        public string? Updated { get; set; }
        public List<GalleryItem> Items { get; set; } = new();
    }
}

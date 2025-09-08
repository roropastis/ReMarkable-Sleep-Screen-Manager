using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RemarkableSleepScreenManager.Models;

namespace RemarkableSleepScreenManager.Services
{
    public interface IGalleryService
    {
        Task<GalleryCatalog> LoadGalleryAsync();
        ObservableCollection<GalleryItem> GalleryItems { get; }
    }
}

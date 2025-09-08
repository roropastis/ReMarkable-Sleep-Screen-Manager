using System.Threading.Tasks;

namespace RemarkableSleepScreenManager.Services
{
    public interface IImageService
    {
        Task<string> ResizeToPaperProAsync(string imagePath);
        Task<string> DownloadImageAsync(string url);
        bool IsValidImageFile(string filePath);
    }
}

using System.Threading.Tasks;
using RemarkableSleepScreenManager.Models;

namespace RemarkableSleepScreenManager.Services
{
    public interface ISshService
    {
        Task<bool> TestConnectionAsync(ConnectionSettings settings);
        Task<string> ExecuteCommandAsync(ConnectionSettings settings, string command);
        Task UploadFileAsync(ConnectionSettings settings, string localPath, string remotePath);
        Task UploadTextAsync(ConnectionSettings settings, string content, string remotePath);
        Task EnsureDirectoryExistsAsync(ConnectionSettings settings, string remotePath);
    }
}

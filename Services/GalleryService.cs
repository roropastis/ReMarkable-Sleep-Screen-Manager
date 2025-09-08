using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RemarkableSleepScreenManager.Models;

namespace RemarkableSleepScreenManager.Services
{
    public class GalleryService : IGalleryService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string GalleryIndexUrl = "https://roropastis.github.io/ReMarkable-Sleep-Screen-Manager/gallery/index.json";

        public ObservableCollection<GalleryItem> GalleryItems { get; } = new();

        public async Task<GalleryCatalog> LoadGalleryAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync(GalleryIndexUrl);
                var catalog = JsonSerializer.Deserialize<GalleryCatalog>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (catalog?.Items != null)
                {
                    GalleryItems.Clear();
                    foreach (var item in catalog.Items)
                    {
                        GalleryItems.Add(item);
                    }
                }

                return catalog ?? new GalleryCatalog();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load gallery: {ex.Message}", ex);
            }
        }
    }
}

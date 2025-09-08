using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RemarkableSleepScreenManager.Services
{
    public class ImageService : IImageService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<string> ResizeToPaperProAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                using var src = Image.FromFile(imagePath);
                // Paper Pro dimensions: 1620x2160 (portrait)
                using var bmp = new Bitmap(1620, 2160);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.FillRectangle(Brushes.White, 0, 0, 1620, 2160);

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
            });
        }

        public async Task<string> DownloadImageAsync(string url)
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            var tmp = Path.Combine(Path.GetTempPath(), $"rm_downloaded_{Guid.NewGuid():N}.png");
            await File.WriteAllBytesAsync(tmp, bytes);
            return tmp;
        }

        public bool IsValidImageFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
        }
    }
}

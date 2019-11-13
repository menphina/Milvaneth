using Milvaneth.Common;
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace Milvaneth.Service
{
    public static class IconManagementService
    {
        public static Bitmap HqLayer
        {
            get
            {
                if (_hqLayer == null)
                    GetHqLayer();

                return _hqLayer;
            }
        }
        public static Bitmap NotFound
        {
            get
            {
                if (_notFound == null)
                    GetNotFound();

                return _notFound;
            }
        }

        private static ZipArchive _storeHandle;
        private static Bitmap _hqLayer;
        private static Bitmap _notFound;

        public static void Initialize()
        {
            var save = Helper.GetMilFilePathRaw("iconstore.pack");
            _storeHandle = ZipFile.OpenRead(save);
            LoggingManagementService.WriteLine($"{nameof(IconManagementService)} initialized", "IcoMgmt");
        }

        public static void Dispose()
        {
            try { _storeHandle.Dispose(); } catch { /* ignored */ }
            LoggingManagementService.WriteLine($"{nameof(IconManagementService)} uninitialized", "IcoMgmt");
        }

        public static BitmapImage GetIcon(int itemId, bool isHq)
        {
            itemId = Math.Abs(itemId);
            var iconNq = GetItemIconNq(itemId);

            if (!isHq) return BitmapToImageSource(iconNq);

            var iconHq = new Bitmap(40, 40);
            
            for (var x = 0; x < 40; x++)
            for (var y = 0; y < 40; y++)
            {
                var lp = HqLayer.GetPixel(x, y);
                var np = iconNq.GetPixel(x, y);
                var a = (double)lp.A / 255;
                var r = lp.R * a + np.R * (1 - a);
                var g = lp.G * a + np.G * (1 - a);
                var b = lp.B * a + np.B * (1 - a);
                iconHq.SetPixel(x, y, Color.FromArgb(np.A, (int)r, (int)g, (int)b));
            }

            return BitmapToImageSource(iconHq);
        }

        public static void MergeIconStore()
        {
            var save = Helper.GetMilFilePathRaw("iconstore.pack");
            var root = Helper.GetMilFilePathRaw("");
            var di = new DirectoryInfo(root);
            var fis = di.EnumerateFiles($"iconstore_delta_*.pack");
            foreach (var fi in fis)
            {
                Helper.MergeZip(save, fi.FullName);
            }
        }

        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private static void GetHqLayer()
        {
            var entry = _storeHandle.GetEntry("hq_layer_mask.png") ?? throw new InvalidOperationException("Invalid icon store file (HQ)");

            using (var reader = entry.Open())
            {
                _hqLayer = new Bitmap(reader);
            }
        }

        private static void GetNotFound()
        {
            var entry = _storeHandle.GetEntry($"not_found.png") ?? throw new InvalidOperationException("Invalid icon store file (NF)");

            using (var reader = entry.Open())
            {
                _notFound = new Bitmap(reader);
            }
        }

        private static Bitmap GetItemIconNq(int icon)
        {
            var entry = _storeHandle.GetEntry($"{icon}.png");

            if (entry == null) return NotFound;

            using (var reader = entry.Open())
            {
                return new Bitmap(reader);
            }
        }
    }
}

using Toqe.Downloader.Business.Download;
using Toqe.Downloader.Business.Observer;

namespace Milvaneth.Communication.Download
{
    public class DownloadInfo
    {
        private DownloadProgressMonitor _dpm;
        private DownloadSpeedMonitor _dsm;
        private MultiPartDownload _mpd;

        public DownloadInfo(DownloadProgressMonitor dpm, DownloadSpeedMonitor dsm, MultiPartDownload mpd, string src, string dst)
        {
            _dpm = dpm;
            _dsm = dsm;
            _mpd = mpd;
            Url = src;
            File = dst;
        }

        public readonly string Url;
        public readonly string File;
        public long DownloadedBytes => _dpm.GetCurrentProgressInBytes(_mpd);
        public long TotalBytes => _dpm.GetTotalFilesizeInBytes(_mpd);
        public int CurrentSpeedBytes => _dsm.GetCurrentBytesPerSecond();
        public float DownloadedPercentage => _dpm.GetCurrentProgressPercentage(_mpd);
        public bool Finished { get; set; }
    }
}

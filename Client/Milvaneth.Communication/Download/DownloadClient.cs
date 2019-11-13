using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toqe.Downloader.Business.Contract.Events;
using Toqe.Downloader.Business.Download;
using Toqe.Downloader.Business.DownloadBuilder;
using Toqe.Downloader.Business.Observer;
using Toqe.Downloader.Business.Utils;

namespace Milvaneth.Communication.Download
{
    internal class DownloadClient
    {
        public OnFileDownloadCompleteDelegate OnFileDownloadComplete;

        private DownloadProgressMonitor progressMonitor;

        public DownloadClient()
        {
            progressMonitor = new DownloadProgressMonitor();
        }

        public DownloadInfo StartDownload(string src, string dst)
        {
            var url = new Uri(src);
            var file = new System.IO.FileInfo(dst);
            var requestBuilder = new SimpleWebRequestBuilder();
            var dlChecker = new DownloadChecker();
            var httpDlBuilder = new SimpleDownloadBuilder(requestBuilder, dlChecker);
            var rdlBuilder = new ResumingDownloadBuilder(3000, 5000, 5, httpDlBuilder);
            var download = new MultiPartDownload(url, 4096, 4, rdlBuilder, requestBuilder, dlChecker, null);
            var dlSaver = new DownloadToFileSaver(file);
            var speedMonitor = new DownloadSpeedMonitor(128);
            var info = new DownloadInfo(progressMonitor, speedMonitor, download, src, dst);
            speedMonitor.Attach(download);
            progressMonitor.Attach(download);
            dlSaver.Attach(download);
            download.DownloadCompleted += (x) => OnCompleted(x, info);
            download.Start();
            return info;
        }

        public DownloadInfo[] StartDownload(Dictionary<string, string> downloads)
        {
            var resultCollection = new ConcurrentBag<DownloadInfo>();
            Parallel.ForEach(downloads, x =>
            {
                resultCollection.Add(StartDownload(x.Key, x.Value));
            });
            return resultCollection.ToArray();
        }

        private void OnCompleted(DownloadEventArgs args, DownloadInfo info)
        {
            args.Download.DetachAllHandlers();
            info.Finished = true;
            OnFileDownloadComplete?.Invoke(info);
        }
    }
}

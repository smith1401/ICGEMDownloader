using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;

namespace ICGEMDownloader
{
    public class StaticModel : ObservableObject, IModel
    {
        private bool shouldBeDownloaded;
        private bool downloaded;
        private string info;
        private long? totalBytes;
        private long? bytesReceived;
        public string Name { get; set; }
        public int Degree { get; set; }
        public string Data { get; set; }
        public string DownLink { get; set; }

        public bool ShouldBeDownloaded
        {
            get => shouldBeDownloaded;
            set { shouldBeDownloaded = value; RaisePropertyChanged(); }
        }

        public bool Downloaded
        {
            get => downloaded;
            set { downloaded = value; RaisePropertyChanged(); }
        }

        public string Info
        {
            get => info;
            set { info = value; RaisePropertyChanged(); }
        }

        public long? TotalBytes
        {
            get => totalBytes;
            set { totalBytes = value; RaisePropertyChanged(); }
        }

        public long? BytesReceived
        {
            get => bytesReceived;
            set { bytesReceived = value; RaisePropertyChanged(); }
        }


        public int Year { get; set; }
        public async Task Download()
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (sender, args) =>
                {
                    TotalBytes = args.TotalBytesToReceive / 1000;
                    BytesReceived = args.BytesReceived / 1000;
                    Info = $"Downloading {Name} ({BytesReceived}/{TotalBytes}) kB";
                };
                webClient.DownloadFileCompleted += (sender, args) => Downloaded = true;
                    
                //webClient.DownloadFileAsync(new Uri(DownLink), $"models/{Name}.zip");
                await webClient.DownloadFileTaskAsync(DownLink, $"models/{Name}.zip");
            }
        }
    }
}

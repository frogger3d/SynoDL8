namespace SynoDL8.DesignViewModels
{
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using SynoDL8.Model;
    using SynoDL8.Services;
    using SynoDL8.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reactive.Threading.Tasks;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;

    public class MainDesignViewModel
    {
        public MainDesignViewModel()
        {
            this.UploadSpeed = "Upload 5 KBps";
            this.DownloadSpeed = "Download 50 KBps";
            this.HostInfo = @"Geoffrey @ http://mydiskstation:5000";
            this.Message = "Dumb message";
        }

        public string HostInfo { get; set; }

        public string Url { get; set; }

        public string Message { get; set; }

        public ReactiveList<DownloadTaskViewModel> Content { get; set; }

        public string UploadSpeed { get; set; }

        public string DownloadSpeed { get; set; }
    }
}

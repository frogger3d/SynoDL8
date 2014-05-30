namespace SynoDL8.ViewModels
{
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using SynoDL8.Model;
    using SynoDL8.Services;
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

    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        private readonly ISynologyService SynologyService;
        private readonly ObservableAsPropertyHelper<string> uploadSpeed;
        private readonly ObservableAsPropertyHelper<string> downloadSpeed;

        private string message;
        private ReactiveList<DownloadTaskViewModel> content;
        private string url;

        private IDisposable listSubscription;
        private IDisposable statisticsSubscription;

        public MainViewModel(ISynologyService synologyService)
        {
            this.SynologyService = synologyService;

            this.Content = new ReactiveList<DownloadTaskViewModel>();

            var available = new BehaviorSubject<bool>(true);

            this.VersionsCommand = new ReactiveCommand(available);
            this.InfoCommand = new ReactiveCommand(available);
            this.ListCommand = new ReactiveCommand(available);
            this.CreateCommand = new ReactiveCommand(available);

            Observable.CombineLatest(VersionsCommand.IsExecuting, InfoCommand.IsExecuting, ListCommand.IsExecuting, CreateCommand.IsExecuting, (v, i, l, c) => !(v || i || l || c))
                      .Subscribe(n => available.OnNext(n));

            this.VersionsCommand.RegisterAsyncTask(_ => this.Versions()).Subscribe(v => DisplayDialog(v));
            this.InfoCommand.RegisterAsyncTask(_ => this.Info()).Subscribe(v => DisplayDialog(v));
            this.ListCommand.RegisterAsyncTask(_ => this.List()).Subscribe();
            this.CreateCommand.RegisterAsyncTask(url => this.Create(url)).Subscribe();

            this.listSubscription = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(4))
                                              .ObserveOnDispatcher()
                                              .Select(t => Observable.FromAsync(this.List))
                                              .Switch()
                                              .Subscribe();

            var statistics = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(4))
                                       .Select(_ => Observable.FromAsync(this.SynologyService.GetStatisticsAsync))
                                       .Switch()                                       
                                       .Publish();

            this.uploadSpeed = statistics.Select(s => (s == null) ? "" : string.Format("Upload: {0:0} KBps", s.DownloadSpeed / 1000.0))
                                           .ToProperty(this, v => v.UploadSpeed);
            this.downloadSpeed = statistics.Select(s => (s == null) ? "" : string.Format("Download: {0:0} KBps", s.DownloadSpeed / 1000.0))
                                           .ToProperty(this, v => v.DownloadSpeed);

            this.statisticsSubscription = statistics.Connect();
        }

        private async void DisplayDialog(string v)
        {
            await new MessageDialog(v).ShowAsync();
        }

        public ReactiveCommand VersionsCommand { get; private set; }
        public ReactiveCommand InfoCommand { get; private set; }
        public ReactiveCommand ListCommand { get; private set; }
        public ReactiveCommand CreateCommand { get; private set; }

        public string Url
        {
            get { return this.url; }
            set { this.RaiseAndSetIfChanged(ref this.url, value); }
        }

        public string Message
        {
            get { return this.message; }
            private set { this.RaiseAndSetIfChanged(ref message, value); }
        }

        public ReactiveList<DownloadTaskViewModel> Content
        {
            get { return this.content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        private async Task<string> Versions()
        {
            return await this.SynologyService.GetVersionsAsync();
        }

        private async Task<string> Info()
        {
            return await this.SynologyService.GetInfoAsync();
        }

        private async Task<bool> Create(object param)
        {
            string url;
            if (param is Uri)
            {
                url = param.ToString();
            }
            else
            {
                url = this.GetAndClearUrl();
            }

            this.Message = "Starting download";
            var response = await this.SynologyService.CreateTaskAsync(url);
            if (response.Success)
            {
                return await this.List();
            }
            else
            {
                await new MessageDialog(string.Format("Start download failed: {0} ({1})", response.Error, response.ErrorCode)).ShowAsync();
                return false;
            }
        }

        private async Task<bool> List()
        {
            string dialogMessage = null;
            try
            {
                var response = await this.SynologyService.List();
                using (this.Content.SuppressChangeNotifications())
                {
                    this.Content.Clear();
                    this.Content.AddRange(response.Select(t => new DownloadTaskViewModel(this.SynologyService, t)));
                }
            }
            catch (Exception e)
            {
                dialogMessage = "Connection failed " + e.Message;
            }

            if (dialogMessage != null)
            {
                await new MessageDialog(dialogMessage).ShowAsync();
            }

            return dialogMessage != null;
        }

        private string GetAndClearUrl()
        {
            string result = this.Url;
            this.Url = null;
            return result;
        }

        public string UploadSpeed
        {
            get { return this.uploadSpeed.Value; }
        }

        public string DownloadSpeed
        {
            get { return this.downloadSpeed.Value; }
        }
    }
}

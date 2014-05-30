namespace SynoDL8.ViewModels
{
    using Microsoft.Practices.Prism.StoreApps.Interfaces;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using ReactiveUI.Mobile;
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
    using Windows.UI.Xaml.Navigation;

    public class MainViewModel : ReactiveObject, INavigationAware, IDisposable
    {
        private readonly ISynologyService SynologyService;
        private readonly ObservableAsPropertyHelper<string> uploadSpeed;
        private readonly ObservableAsPropertyHelper<string> downloadSpeed;
        private readonly Credentials Credentials;
        private readonly IConnectableObservable<Statistics> statisticsObservable;
        private readonly IConnectableObservable<bool> listObservable;
        private readonly ObservableAsPropertyHelper<bool> hasDownloads, hasFinished;

        private string message;
        private string url;

        private IDisposable listSubscription;
        private IDisposable statisticsSubscription;

        private ReactiveList<DownloadTaskViewModel> allTasks;

        public MainViewModel(ISynologyService synologyService, IConfigurationService configurationService)
        {
            this.SynologyService = synologyService.ThrowIfNull("synologyService");
            this.Credentials = configurationService.ThrowIfNull("configurationService").GetLastCredentials();
            this.HostInfo = string.Format("{0} @ {1}", this.Credentials.User, this.Credentials.Hostname);

            this.allTasks = new ReactiveList<DownloadTaskViewModel>();

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

            this.listObservable = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(4))
                                            .ObserveOnDispatcher()
                                            .Select(t => Observable.FromAsync(this.List))
                                            .Switch()
                                            .Publish();

            this.DownloadingList = this.allTasks.CreateDerivedCollection(t => t, filter: t => t.IsDownloading);
            this.hasDownloads = this.DownloadingList
                                    .CountChanged
                                    .Select(c => c > 0)
                                    .ToProperty(this, v => v.HasDowloads);
            this.FinishedList = this.allTasks.CreateDerivedCollection(t => t, filter: t => !t.IsDownloading);
            this.hasFinished = this.DownloadingList
                                   .CountChanged
                                   .Select(c => c > 0)
                                   .ToProperty(this, v => v.HasFinished);
            this.statisticsObservable = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(4))
                                                  .Select(_ => Observable.FromAsync(this.SynologyService.GetStatisticsAsync))
                                                  .Switch()                                       
                                                  .Publish();

            this.uploadSpeed = statisticsObservable.Select(s => (s == null) ? "" : string.Format("Upload: {0:0} KBps", s.UploadSpeed / 1000.0))
                                                   .ToProperty(this, v => v.UploadSpeed);
            this.downloadSpeed = statisticsObservable.Select(s => (s == null) ? "" : string.Format("Download: {0:0} KBps", s.DownloadSpeed / 1000.0))
                                                     .ToProperty(this, v => v.DownloadSpeed);

            this.Sections = new ReactiveList<string>() { "a", "b", "c", };
        }

        public string HostInfo { get; private set; }

        private async void DisplayDialog(string v)
        {
            await new MessageDialog(v).ShowAsync();
        }

        public ReactiveCommand VersionsCommand { get; private set; }
        public ReactiveCommand InfoCommand { get; private set; }
        public ReactiveCommand ListCommand { get; private set; }
        public ReactiveCommand CreateCommand { get; private set; }

        public IReactiveDerivedList<DownloadTaskViewModel> DownloadingList { get; private set; }
        public bool HasDowloads { get { return this.hasDownloads.Value; } }
        public IReactiveDerivedList<DownloadTaskViewModel> FinishedList { get; private set; }
        public bool HasFinished { get { return this.hasFinished.Value; } }

        public ReactiveList<string> Sections { get; set; }

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
                var newTasks = await this.SynologyService.List();
                using (this.allTasks.SuppressChangeNotifications())
                {
                    var removeIds = this.allTasks.Select(t => t.Task.Id).Except(newTasks.Select(t => t.Id));
                    foreach(var id in removeIds)
                    {
                        var task = this.allTasks.Single(t => t.Task.Id == id);
                        this.allTasks.Remove(task);
                    }

                    var updateIds = this.allTasks.Select(t => t.Task.Id).Intersect(newTasks.Select(t => t.Id));
                    foreach(var id in updateIds)
                    {
                        var task = this.allTasks.Single(t => t.Task.Id == id);
                        task.Task = newTasks.Single(t => t.Id == id);
                    }

                    var addIds = newTasks.Select(t => t.Id).Except(this.allTasks.Select(t => t.Task.Id));
                    foreach(var id in addIds)
                    {
                        var task = newTasks.Single(t => t.Id == id);
                        this.allTasks.Add(new DownloadTaskViewModel(this.SynologyService, task));
                    }
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

        public void Dispose()
        {
            this.statisticsSubscription.Dispose();
            this.listSubscription.Dispose();
        }

        public void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            this.Dispose();
        }

        public void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            string message = navigationParameter as string;
            if (message != null)
            {
                this.Message = message;
            }

            this.statisticsSubscription = this.statisticsObservable.Connect();
            this.listSubscription = this.listObservable.Connect();
        }
    }
}

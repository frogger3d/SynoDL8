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

    public class MainPageViewModel : ReactiveObject, INavigationAware, IDisposable, IEnableLogger
    {
        private readonly ISynologyService SynologyService;
        private readonly INavigationService NavigationService;

        private readonly ObservableAsPropertyHelper<string> uploadSpeed;
        private readonly ObservableAsPropertyHelper<string> downloadSpeed;
        private readonly Credentials Credentials;
        private readonly IConnectableObservable<Statistics> statisticsObservable;
        private readonly IConnectableObservable<IEnumerable<DownloadTask>> listObservable;
        private readonly ObservableAsPropertyHelper<bool> hasActive, hasFinished;

        private string message;
        private string url;

        private IDisposable listSubscription;
        private IDisposable statisticsSubscription;

        private ReactiveList<DownloadTaskViewViewModel> allTasks;

        public MainPageViewModel(INavigationService navigationService, ISynologyService synologyService, IConfigurationService configurationService)
        {
            this.NavigationService = navigationService.ThrowIfNull("navigationService");
            this.SynologyService = synologyService.ThrowIfNull("synologyService");
            this.Credentials = configurationService.ThrowIfNull("configurationService").GetLastCredentials();
            this.HostInfo = string.Format("{0} @ {1}", this.Credentials.User, this.Credentials.Hostname);

            this.allTasks = new ReactiveList<DownloadTaskViewViewModel>();
            this.allTasks.ChangeTrackingEnabled = true;

            var available = new BehaviorSubject<bool>(true);

            this.VersionsCommand = new ReactiveCommand(available);
            this.InfoCommand = new ReactiveCommand(available);
            this.ListCommand = new ReactiveCommand(available);
            this.CreateCommand = new ReactiveCommand(available);

            Observable.CombineLatest(VersionsCommand.IsExecuting, InfoCommand.IsExecuting, ListCommand.IsExecuting, CreateCommand.IsExecuting, (v, i, l, c) => !(v || i || l || c))
                      .Subscribe(n => available.OnNext(n));

            this.VersionsCommand.RegisterAsyncTask(_ => this.Versions()).Subscribe(v => DisplayDialog(v));
            this.InfoCommand.RegisterAsyncTask(_ => this.Info()).Subscribe(v => DisplayDialog(v));
            this.CreateCommand.RegisterAsyncTask(url => this.Create(url)).Subscribe();

            this.listObservable = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(4))
                .Select(t => Unit.Default)
                .Merge(this.ListCommand.Select(l => Unit.Default))
                .Where(_ => this.SynologyService.IsSignedIn)
                .Select(t => this.SynologyService.List().ToObservable())
                .Switch()
                .Publish();

            this.listObservable
                .ObserveOnDispatcher()
                .Subscribe(newTasks => UpdateList(newTasks.ToList(), this.allTasks));

            this.ActiveList = this.allTasks
                .CreateDerivedCollection(t => t, filter: t => t.IsActive, orderer: (t1, t2) => (int)(100 * t2.Progress - 100 * t1.Progress));
            this.hasActive = this.ActiveList.Changed
                .Select(_ => this.ActiveList.Count())
                .Select(c => c > 0)
                .ToProperty(this, v => v.HasActive);

            this.FinishedList = this.allTasks
                .CreateDerivedCollection(t => t, filter: t => !t.IsActive);
            this.hasFinished = this.FinishedList.Changed
                .Select(_ => this.FinishedList.Count())
                .Select(c => c > 0)
                .ToProperty(this, v => v.HasFinished);

            this.statisticsObservable = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(4))
                .Where(_ => this.SynologyService.IsSignedIn)
                .Select(_ => Observable.FromAsync(this.SynologyService.GetStatisticsAsync))
                .Switch()                                       
                .Publish();

            this.uploadSpeed = statisticsObservable
                .Select(s => (s == null) ? "" : string.Format("Upload: {0:0} KBps", s.UploadSpeed / 1000.0))
                .ToProperty(this, v => v.UploadSpeed);
            this.downloadSpeed = statisticsObservable
                .Select(s => (s == null) ? "" : string.Format("Download: {0:0} KBps", s.DownloadSpeed / 1000.0))
                .ToProperty(this, v => v.DownloadSpeed);

            this.allTasks.CollectionChanged += (s, e) =>
            {
                Debug.WriteLine("all: " + e.Action);
            };
            this.ActiveList.CollectionChanged += (s, e) =>
            {
                Debug.WriteLine("active: " + e.Action);
            };
            this.FinishedList.CollectionChanged += (s, e) =>
            {
                Debug.WriteLine("finish: " + e.Action);
            };
        }

        private void UpdateList(IList<DownloadTask> newTasks, ReactiveList<DownloadTaskViewViewModel> list)
        {
            var removeIds = list.Select(t => t.Task.Id)
                .Except(newTasks.Select(t => t.Id))
                .ToList();

            var updateIds = list.Select(t => t.Task.Id)
                .Intersect(newTasks.Select(t => t.Id))
                .ToList();

            var addIds = newTasks.Select(t => t.Id)
                .Except(list.Select(t => t.Task.Id))
                .ToList();

            foreach (var id in removeIds)
            {
                var task = list.Single(t => t.Task.Id == id);
                list.Remove(task);
            }
            Debug.WriteLine("removed " + removeIds.Count);

            foreach (var id in addIds)
            {
                var task = newTasks.Single(t => t.Id == id);
                list.Add(new DownloadTaskViewViewModel(this.SynologyService, task));
            }
            Debug.WriteLine("added " + addIds.Count);

            foreach (var id in updateIds)
            {
                var task = list.Single(t => t.Task.Id == id);
                task.Task = newTasks.Single(t => t.Id == id);
            }
            Debug.WriteLine("updated " + updateIds.Count);
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

        public IReactiveDerivedList<DownloadTaskViewViewModel> ActiveList { get; private set; }
        public bool HasActive
        {
            get { return this.hasActive.Value; }
        }

        public IReactiveDerivedList<DownloadTaskViewViewModel> FinishedList { get; private set; }
        public bool HasFinished
        {
            get { return this.hasFinished.Value; }
        }

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
                this.ListCommand.Execute(null);
                return true;
            }
            else
            {
                await new MessageDialog(string.Format("Start download failed: {0} ({1})", response.Error, response.ErrorCode)).ShowAsync();
                return false;
            }
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

        public async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            this.statisticsSubscription = this.statisticsObservable.Connect();
            this.listSubscription = this.listObservable.Connect();
            if( !this.SynologyService.IsSignedIn)
            {
                if (!await this.SynologyService.LoginAsync(this.Credentials))
                {
                    this.NavigationService.GoBack();
                    return;
                }
            }

            var request = navigationParameter as DownloadRequest;
            if (request != null)
            {
                var result = await this.SynologyService.CreateTaskAsync(request.Uri.ToString());
                if (result.Success)
                {
                    string message = "download started";
                    if (message != null)
                    {
                        this.Message = message;
                    }
                }
            }
        }
    }
}

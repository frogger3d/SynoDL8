namespace SynoDL8.ViewModel
{
    using ReactiveUI;
    using SynoDL8.DataModel;
    using SynoDL8.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;

    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        private readonly IDataModel DataModel;
        private readonly ObservableAsPropertyHelper<Visibility> busyV;

        private string message;
        private object content;
        private string url;
        private bool busy;

        public MainViewModel(IDataModel dataModel)
        {
            this.DataModel = dataModel;

            var busyObservable = this.ObservableForProperty(v => v.Busy, skipInitial: false);
            var readyObservable = busyObservable.Select(b => !b.Value);

            this.AuthCommand = new ReactiveCommand(readyObservable);
            this.LogoutCommand = new ReactiveCommand(readyObservable);
            this.VersionsCommand = new ReactiveCommand(readyObservable);
            this.InfoCommand = new ReactiveCommand(readyObservable);
            this.ListCommand = new ReactiveCommand(readyObservable);
            this.CreateCommand = new ReactiveCommand(readyObservable);
            
            this.AuthCommand.Subscribe(_ => this.Auth());
            this.LogoutCommand.Subscribe(_ => this.Logout());
            this.VersionsCommand.Subscribe(_ => this.Versions());
            this.InfoCommand.Subscribe(_ => this.Info());
            this.ListCommand.Subscribe(_ => this.List());
            this.CreateCommand.Subscribe(_ => this.Create());

            this.busyV = busyObservable.Select(b => b.Value ? Visibility.Visible : Visibility.Collapsed)
                                       .ToProperty(this, v => v.BusyV);

            this.List();
        }

        public ReactiveCommand AuthCommand { get; private set; }
        public ReactiveCommand LogoutCommand { get; private set; }
        public ReactiveCommand VersionsCommand { get; private set; }
        public ReactiveCommand InfoCommand { get; private set; }
        public ReactiveCommand ListCommand { get; private set; }
        public ReactiveCommand CreateCommand { get; private set; }

        public string Url
        {
            get { return this.url; }
            set { this.RaiseAndSetIfChanged(ref this.url, value); }
        }

        public bool Busy
        {
            get { return this.busy; }
            private set { this.RaiseAndSetIfChanged(ref this.busy, value); }
        }

        public Visibility BusyV
        {
            get { return this.busyV.Value; }
        }

        public string Message
        {
            get { return this.message; }
            private set { this.RaiseAndSetIfChanged(ref message, value); }
        }

        public object Content
        {
            get { return this.content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        private void Auth()
        {
            this.Message = "Authenticating";
            this.Busy = true;

            Observable.StartAsync(this.DataModel.Login)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Message = ev ? "Authenticated" : "Authentication failed";
                              this.Busy = false;
                          },
                          async ex =>
                          {
#if DEBUG
                              this.Content = ex.ToString();
#endif
                              this.Busy = false;
                              await new MessageDialog("Connection failed" + Environment.NewLine + ex.ToString()).ShowAsync();
                          });
        }

        private void Logout()
        {
            this.Message = "Logging out";
            this.Busy = true;

            Observable.StartAsync(this.DataModel.Logout)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Message = ev ? "Logged out" : "Logging out failed";
                              this.Busy = false;
                          },
                          async ex =>
                          {
#if DEBUG
                              this.Content = ex.ToString();
#endif
                              this.Busy = false;
                              await new MessageDialog("Connection failed").ShowAsync();
                          });
        }

        private async void Versions()
        {
            this.Content = await this.DataModel.GetVersions();
        }

        private async void Info()
        {
            this.Content = await this.DataModel.GetInfo();
        }

        private void Create()
        {
            this.Message = "Starting download";
            this.Busy = true;

            Observable.StartAsync(this.CreateDownload)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Content = ev;
                              this.Message = "Download started";
                              this.Busy = false;
                          },
                          async ex =>
                          {
#if DEBUG
                              this.Content = ex.ToString();
#endif
                              this.Busy = false;
                              await new MessageDialog("Start download failed").ShowAsync();
                          });
        }

        private async Task<string> CreateDownload()
        {
            Task<string> createTask = this.DataModel.Create(this.Url);
            this.Url = "";
            return await createTask;
        }

        private void List()
        {
            this.Message = "Getting download list";
            this.Busy = true;

            Observable.StartAsync(this.DataModel.List)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Content = ev.ToList();
                              this.Message = "Download list retrieved";
                              this.Busy = false;
                          },
                          async ex =>
                          {
#if DEBUG
                              this.Content = ex.ToString();
#endif
                              this.Busy = false;
                              await new MessageDialog("Connection failed").ShowAsync();
                          });
        }
    }
}

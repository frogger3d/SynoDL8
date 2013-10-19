namespace SynoDL8.ViewModel
{
    using GalaSoft.MvvmLight;
    using SynoDL8.DataModel;
    using SynoDL8.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;

    public class MainViewModel : ViewModelBase
    {
        private readonly DSQuerier DSQuerier;

        private string message;
        private object content;
        private string url;
        private bool busy;

        public MainViewModel()
        {
            var settings = new SettingsViewModel();
            this.DSQuerier = new DSQuerier(settings.Hostname, settings.User, settings.Password);

            this.AuthCommand = new RelayCommand(this.Auth);
            this.LogoutCommand = new RelayCommand(this.Logout);
            this.VersionsCommand = new RelayCommand(this.Versions);
            this.InfoCommand = new RelayCommand(this.Info);
            this.ListCommand = new RelayCommand(this.List);
            this.CreateCommand = new RelayCommand(this.Create);
        }

        public ICommand AuthCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand VersionsCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand ListCommand { get; private set; }
        public ICommand CreateCommand { get; private set; }

        public string Url
        {
            get { return this.url; }
            set { this.Set("Url", ref this.url, value); }
        }

        public bool Busy
        {
            get { return this.busy; }
            private set
            {
                this.Set("Busy", ref this.busy, value);
                this.RaisePropertyChanged(() => this.BusyV);
            }
        }

        public Visibility BusyV
        {
            get { return this.busy ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string Message
        {
            get { return this.message; }
            private set { this.Set("Message", ref this.message, value); }
        }

        public object Content
        {
            get { return this.content; }
            private set { this.Set("Content", ref this.content, value); }
        }

        private void Auth()
        {
            this.Message = "Authenticating";
            this.Busy = true;

            Observable.StartAsync(this.DSQuerier.Login)
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
                              await new MessageDialog("Connection failed").ShowAsync();
                          });
        }

        private void Logout()
        {
            this.Message = "Logging out";
            this.Busy = true;

            Observable.StartAsync(this.DSQuerier.Logout)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Message = ev ? "Logged out partial" : "Logging out failed";
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
            this.Content = await this.DSQuerier.GetVersions();
        }

        private async void Info()
        {
            this.Content = await this.DSQuerier.GetInfo();
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
            Task<string> createTask = this.DSQuerier.Create(this.Url);
            this.Url = "";
            return await createTask;
        }

        private void List()
        {
            this.Message = "Getting download list";
            this.Busy = true;

            Observable.StartAsync(this.DSQuerier.List)
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

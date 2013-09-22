namespace SynoDL8.ViewModels
{
    using GalaSoft.MvvmLight;
    using SynoDL8.DataModel;
    using SynoDL8.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class ItemsViewModel : ViewModelBase
    {
        private readonly DSQuerier DSQuerier;

        private string message;
        private object content;

        public ItemsViewModel()
        {
            var settings = new SettingsFlyoutViewModel();
            this.DSQuerier = new DSQuerier(settings.Hostname, settings.User, settings.Password);

            this.AuthCommand = new RelayCommand(this.Auth);
            this.LogoutCommand = new RelayCommand(this.Logout);
            this.VersionsCommand = new RelayCommand(this.Versions);
            this.InfoCommand = new RelayCommand(this.Info);
            this.ListCommand = new RelayCommand(this.List);
        }

        public ICommand AuthCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand VersionsCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand ListCommand { get; private set; }

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
            Observable.StartAsync(this.DSQuerier.Authenticate)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Message = ev ? "Authenticated" : "Authentication failed";
                          },
                          ex =>
                          {
                              this.Content = ex.ToString();
                              this.Message = "Connection failed";
                          });
        }

        private void Logout()
        {
            this.Content = "..";
            this.Message = "Logging out";

            Observable.StartAsync(this.DSQuerier.Logout)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Message = ev ? "Logged out partial" : "Logging out failed";
                          },
                          ex =>
                          {
                              this.Content = ex.ToString();
                              this.Message = "Connection failed";
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

        private void List()
        {
            this.Content = "..";
            this.Message = "Getting download list";

            Observable.StartAsync(this.DSQuerier.List)
                      .ObserveOnDispatcher()
                      .Subscribe(
                          ev =>
                          {
                              this.Content = ev.ToList();
                              this.Message = "Download list retrieved";
                          },
                          ex =>
                          {
                              this.Content = ex.ToString();
                              this.Message = "Connection failed";
                          });
        }
    }
}

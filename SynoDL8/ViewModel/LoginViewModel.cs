using ReactiveUI;
using SynoDL8.Model;
using SynoDL8.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynoDL8.ViewModel
{
    public class LoginViewModel : ReactiveObject, ILoginViewModel
    {
        private readonly IConfigurationService ConfigurationService;
        private readonly IDataModel DataModel;

        private readonly ObservableAsPropertyHelper<Visibility> busyV;
        private readonly ObservableAsPropertyHelper<bool> available;

        private Credentials credentials;
        private bool busy;
        private string signinError;

        public LoginViewModel(IConfigurationService configurationService, IDataModel dataModel)
        {
            if (configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }
            if (dataModel == null)
            {
                throw new ArgumentNullException("dataModel");
            }

            this.ConfigurationService = configurationService;
            this.DataModel = dataModel;

            this.Credentials = new Credentials();
            this.Credentials.IsValidationEnabled = true;

            var configuration = this.ConfigurationService.GetConfiguration();

            var hasErrorsObservable = Observable.FromEventPattern<DataErrorsChangedEventArgs>(h => this.Credentials.ErrorsChanged += h, h => this.Credentials.ErrorsChanged -= h)
                                                .Select(e => this.Credentials.GetAllErrors().Any());

            this.SigninCommand = new ReactiveCommand(hasErrorsObservable.Select(e => !e));
            this.SigninCommand.RegisterAsyncTask(_ => this.Signin())
                              .Subscribe(
                                n =>
                                {
                                    if (n)
                                    {
                                        ((Frame)Window.Current.Content).Navigate(typeof(MainPage));
                                    }
                                });

            var busyObservable = this.SigninCommand.IsExecuting;
            this.busyV = busyObservable.Select(b => b ? Visibility.Visible : Visibility.Collapsed)
                                       .ToProperty(this, v => v.BusyV);
            this.available= busyObservable.Select(b => !b)
                                          .ToProperty(this, v => v.Available);
        }        

        public Credentials Credentials
        {
            get { return this.credentials; }
            set { this.RaiseAndSetIfChanged(ref this.credentials, value); }
        }

        public bool Busy
        {
            get { return this.busy; }
            private set { this.RaiseAndSetIfChanged(ref this.busy, value); }
        }

        public bool Available
        {
            get { return this.available.Value; }
        }

        public Visibility BusyV
        {
            get { return this.busyV.Value; }
        }

        public string SigninError
        {
            get { return this.signinError; }
            private set { this.RaiseAndSetIfChanged(ref this.signinError, value); }
        }

        public ReactiveCommand SigninCommand { get; private set; }

        private async Task<bool> Signin()
        {
            if (!this.Credentials.ValidateProperties())
            {
                return false;
            }

            var configuration = new Configuration()
            {
                HostName = this.Credentials.Hostname,
                Password = this.Credentials.Password,
                UserName = this.Credentials.User
            };
            this.ConfigurationService.SaveConfiguration(configuration);
            
            // TODO this is an ugly construction..
            bool result;
            try
            {
                result = await this.DataModel.LoginAsync();
            }
            catch (HttpRequestException e)
            {
                var inner = e.InnerException as WebException;
                if(inner != null)
                {
                    this.SigninError = "Sign in error: " + inner.Status;
                }

                return false;
            }

            return result;
        }
    }
}

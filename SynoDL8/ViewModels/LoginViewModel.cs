using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ReactiveUI;
using SynoDL8.Model;
using SynoDL8.Views;
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
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynoDL8.ViewModels
{
    public class LoginViewModel : ReactiveObject, ILoginViewModel, INavigationAware
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

            var busyObservable = this.SigninCommand.IsExecuting.StartWith(false);
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

            this.SigninError = null;

            try
            {
                await this.DataModel.LoginAsync(credentials);
            }
            catch (HttpRequestException e)
            {
                var inner = e.InnerException as WebException;
                if (inner != null)
                {
                    this.SigninError = inner.Message;
                }
                else
                {
                    this.SigninError = e.Message;
                }

                return false;
            }
            catch(TimeoutException)
            {
                this.SigninError = "Timeout trying to connect";

                return false;
            }
            catch(VerificationException)
            {
                this.SigninError = "Wrong credentials";

                return false;
            }

            this.ConfigurationService.SaveConfiguration(this.Credentials);
            return true;
        }

        public void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            throw new NotImplementedException();
        }
    }
}

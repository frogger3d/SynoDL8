using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ReactiveUI;
using SynoDL8.Model;
using SynoDL8.Services;
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
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SynoDL8.ViewModels
{
    public class LoginPageViewModel : ReactiveObject, INavigationAware
    {
        private readonly IConfigurationService ConfigurationService;
        private readonly ISynologyService SynologyService;
        private readonly INavigationService NavigationService;

        private readonly ObservableAsPropertyHelper<Visibility> busyV;
        private readonly ObservableAsPropertyHelper<bool> available;

        private Credentials credentials;
        private bool busy;
        private string signinError;
        private ReactiveList<PreviousCredentialsViewModel> previous;

        public LoginPageViewModel(IConfigurationService configurationService, ISynologyService synologyService, INavigationService navigationService)
        {
            this.ConfigurationService = configurationService.ThrowIfNull("configurationService");
            this.SynologyService = synologyService.ThrowIfNull("synologyService");
            this.NavigationService = navigationService.ThrowIfNull("navigationService");

            this.Credentials = this.ConfigurationService.GetLastCredentials() ?? new Credentials();
            this.Credentials.IsValidationEnabled = true;

            var previousCredentialsViewModels = this.ConfigurationService.GetAllCredentials()
                .Select(c => new PreviousCredentialsViewModel(c))
                .ToList(); // make sure we subscribe to the actual list, otherwise the subscriptions would get lost on requery
            foreach (var vm in previousCredentialsViewModels)
            {
                var current = vm; // prevent all anonymous methods to reference the last instance
                current.Remove.Subscribe(_ =>
                {
                    this.Previous.Remove(current);
                    this.ConfigurationService.RemoveCredentials(current.Model);
                });
                current.Select.Subscribe(_ =>
                {
                    this.Credentials.Hostname = current.Model.Hostname;
                    this.Credentials.User = current.Model.User;
                    this.Credentials.Password = current.Model.Password;
                });
            }
            this.Previous = new ReactiveList<PreviousCredentialsViewModel>();
            this.Previous.AddRange(previousCredentialsViewModels);

            var canSignin = new BehaviorSubject<bool>(true);

            this.SigninCommand = new ReactiveCommand(canSignin);
            this.SigninCommand.RegisterAsyncTask(_ => this.Signin())
                .Subscribe(
                n =>
                {
                    if (n)
                    {
                        this.NavigationService.Navigate("Main", null);
                    }
                });

            var hasErrorsObservable = this.WhenAny(v => v.Credentials.IsValid, v => !v.Value);
            Observable.CombineLatest(SigninCommand.IsExecuting, hasErrorsObservable, (executing, hasErrors) => !(executing || hasErrors))
                .Subscribe(canSignin);

            var busyObservable = this.SigninCommand.IsExecuting.StartWith(false);
            this.busyV = busyObservable
                .Select(b => b ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, v => v.BusyV);

            this.available = busyObservable.Select(b => !b)
                .ToProperty(this, v => v.Available);
        }

        [RestorableState]
        public Credentials Credentials
        {
            get { return this.credentials; }
            set { this.RaiseAndSetIfChanged(ref this.credentials, value); }
        }

        public ReactiveList<PreviousCredentialsViewModel> Previous
        {
            get { return this.previous; }
            set { this.RaiseAndSetIfChanged(ref this.previous, value); }
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
                await this.SynologyService.LoginAsync(credentials);
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

            this.ConfigurationService.SaveCredentials(this.Credentials);
            return true;
        }

        public void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            this.FillStateDictionary(viewModelState);
        }

        public void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            this.RestoreViewModel(viewModelState);
        }
    }
}

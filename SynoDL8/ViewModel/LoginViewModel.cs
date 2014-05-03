using ReactiveUI;
using SynoDL8.DataModel;
using SynoDL8.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynoDL8.ViewModel
{
    public class LoginViewModel : ReactiveObject, ILoginViewModel, INotifyDataErrorInfo
    {
        private readonly IConfigurationService ConfigurationService;
        private readonly IDataModel DataModel;

        private readonly ObservableAsPropertyHelper<bool> hasErrors;
        private readonly ObservableAsPropertyHelper<string> errors;
        private readonly ObservableAsPropertyHelper<Visibility> busyV;

        private string hostname, user, password;
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

            var configuration = this.ConfigurationService.GetConfiguration();
            this.hostname = configuration.HostName;
            this.user = configuration.UserName;
            this.password = configuration.Password;

            var hostnameHasError = this.ObservableForProperty(v => v.Hostname, skipInitial: false)
                                       .Select(o => this.GetErrors(o.PropertyName).Any())
                                       .DistinctUntilChanged();
            var userHasError = this.ObservableForProperty(v => v.User, skipInitial: false)
                                   .Select(o => this.GetErrors(o.PropertyName).Any())
                                   .DistinctUntilChanged();

            hostnameHasError.Subscribe(e => { this.RaiseErrorsChanged("HostName"); });
            userHasError.Subscribe(e => { this.RaiseErrorsChanged("User"); });

            var hasErrorsObservable = hostnameHasError.CombineLatest(userHasError, (a, b) => a || b);
            this.hasErrors = hasErrorsObservable.ToProperty(this, vm => vm.HasErrors);

            this.errors = hasErrorsObservable.Select(h => string.Join(Environment.NewLine, this.GetErrors(null)))
                                             .ToProperty(this, vm => vm.Errors);

            this.SigninCommand = new ReactiveCommand(hasErrorsObservable.Select(e => !e));
            this.SigninCommand.RegisterAsyncTask(_ => this.Signin())
                              .Subscribe(
                                n =>
                                {
                                    if (n)
                                    {
                                        ((Frame)Window.Current.Content).Navigate(typeof(MainPage));
                                    }
                                    else
                                    {
                                        signinError = "Signin failed";
                                    }
                                },
                                e =>
                                {
                                    // TODO probably could not contact host
                                    signinError = "Signin failed: " + e.ToString();
                                });

            var busyObservable = this.SigninCommand.IsExecuting;
            this.busyV = busyObservable.Select(b => b ? Visibility.Visible : Visibility.Collapsed)
                                       .ToProperty(this, v => v.BusyV);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool Busy
        {
            get { return this.busy; }
            private set { this.RaiseAndSetIfChanged(ref this.busy, value); }
        }

        public Visibility BusyV
        {
            get { return this.busyV.Value; }
        }

        public string Hostname
        {
            get { return this.hostname; }
            set { this.RaiseAndSetIfChanged(ref this.hostname, value); }
        }

        public string User
        {
            get { return this.user; }
            set { this.RaiseAndSetIfChanged(ref this.user, value); }
        }

        public string Password
        {
            get { return this.password; }
            set { this.RaiseAndSetIfChanged(ref this.password, value); }
        }

        public string Errors
        {
            get { return this.errors.Value; }
        }

        public ReactiveCommand SigninCommand { get; private set; }

        private async Task<bool> Signin()
        {
            var configuration = new Configuration()
            {
                HostName = this.hostname,
                Password = this.password,
                UserName = this.user
            };
            this.ConfigurationService.SaveConfiguration(configuration);
            
            // TODO this is an ugly construction..
            bool result;
            try
            {
                result = await this.DataModel.Login();
            }
            catch(AggregateException e)
            {
                if (e.InnerException is WebException)
                {
                    return false;
                }
                else throw;
            }
            return result;
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return this.GetErrors(propertyName);
        }

        public IEnumerable<string> GetErrors(string propertyName)
        {
            bool checkAll = string.IsNullOrEmpty(propertyName);

            if (propertyName == "Hostname" || checkAll)
            {
                int separatorIndex = this.Hostname.IndexOf(':');
                string host = null;
                if (separatorIndex < 1)
                {
                    host = this.Hostname;
                }
                else
                {
                    host = this.Hostname.Substring(0, separatorIndex);
                    string portString = this.Hostname.Substring(separatorIndex + 1);
                    int port;
                    if(!int.TryParse(portString, out port))
                    {
                        yield return "Port part of host name not valid.";
                    }
                }

                if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
                {
                    yield return "Host name not valid.";
                }
            }

            if (propertyName == "User" || checkAll)
            {
                if (string.IsNullOrWhiteSpace(this.User))
                {
                    yield return "Username must be entered.";
                }
            }
        }

        public bool HasErrors
        {
            get { return this.hasErrors.Value; }
        }
        
        private void RaiseErrorsChanged(string propertyName)
        {
            var handle = this.ErrorsChanged;
            if (handle != null)
            {
                handle(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
    }
}

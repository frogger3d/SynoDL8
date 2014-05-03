using ReactiveUI;
using SynoDL8.DataModel;
using SynoDL8.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
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
        private readonly ObservableAsPropertyHelper<bool> available;

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

            TimeSpan delay = TimeSpan.FromMilliseconds(100);

            var hostnameHasError = this.ObservableForProperty(v => v.Hostname, skipInitial: false)
                                       .DistinctUntilChanged()
                                       .Throttle(delay)
                                       .Select(o => this.GetErrors(o.PropertyName).Any())
                                       .Publish();

            var userHasError = this.ObservableForProperty(v => v.User, skipInitial: false)
                                   .DistinctUntilChanged()
                                   .Throttle(delay)
                                   .Select(o => this.GetErrors(o.PropertyName).Any())
                                   .Publish();

            var passwordHasError = this.ObservableForProperty(v => v.Password, skipInitial: false)
                                       .DistinctUntilChanged()
                                       .Throttle(delay)
                                       .Select(o => this.GetErrors(o.PropertyName).Any())
                                       .Publish();

            hostnameHasError.Subscribe(e => { Debug.WriteLine("Hostname : " + e); this.RaiseErrorsChanged("HostName"); });
            userHasError.Subscribe(e => { Debug.WriteLine("user : " + e); this.RaiseErrorsChanged("User"); });
            passwordHasError.Subscribe(e => { Debug.WriteLine("Password: " + e); this.RaiseErrorsChanged("Password"); });

            var hasErrorsObservable = Observable.CombineLatest(hostnameHasError, userHasError, passwordHasError, (a, b, c) => a || b || c);
            this.hasErrors = hasErrorsObservable//.DistinctUntilChanged()
                                                .ToProperty(this, vm => vm.HasErrors);

            this.errors = hasErrorsObservable.Select(h => string.Join(Environment.NewLine, this.GetErrors(null)))
                                             .ToProperty(this, vm => vm.Errors);

            hostnameHasError.Connect();
            userHasError.Connect();
            passwordHasError.Connect();

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
                                        this.SigninError = "Signin failed";
                                    }
                                });

            var busyObservable = this.SigninCommand.IsExecuting;
            this.busyV = busyObservable.Select(b => b ? Visibility.Visible : Visibility.Collapsed)
                                       .ToProperty(this, v => v.BusyV);
            this.available= busyObservable.Select(b => !b)
                                          .ToProperty(this, v => v.Available);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        public string SigninError
        {
            get { return this.signinError; }
            private set { this.RaiseAndSetIfChanged(ref this.signinError, value); }
        }

        public string Errors
        {
            get { return this.errors.Value; }
        }

        public ReactiveCommand SigninCommand { get; private set; }

        private async Task<bool> Signin()
        {
            if (this.HasErrors)
            {
                return false;
            }

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
            catch (AggregateException)
            {
                return false;
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
                if (string.IsNullOrWhiteSpace(this.Hostname))
                {
                    yield return "Host name is required.";
                }
                else
                {
                    Uri uri = null;
                    string formatError = null;
                    try
                    {
                        uri = new Uri(this.Hostname);
                    }
                    catch(FormatException e)
                    {
                        formatError = e.Message;
                    }
                    if (formatError != null)
                    {
                        yield return formatError;
                    }

                    if (uri != null)
                    {
                        if (!(this.Hostname.StartsWith(@"http://") || this.Hostname.StartsWith(@"https://")))
                        {
                            yield return "Host name should start with either http:// or https://";
                        }
                        else if (uri.PathAndQuery != @"/")
                        {
                            yield return "Host name should not contain a path or qeury";
                        }
                    }
                }
            }

            if (propertyName == "User" || checkAll)
            {
                if (string.IsNullOrWhiteSpace(this.User))
                {
                    yield return "Username is required.";
                }
            }

            if (propertyName == "Password" || checkAll)
            {
                if (string.IsNullOrWhiteSpace(this.Password))
                {
                    yield return "Password is required.";
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

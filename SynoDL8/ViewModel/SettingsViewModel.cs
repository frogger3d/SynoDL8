﻿using ReactiveUI;
using SynoDL8.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace SynoDL8.ViewModel
{
    public class SettingsViewModel : ReactiveObject, INotifyDataErrorInfo
    {
        private readonly IConfigurationService ConfigurationService;

        private readonly ObservableAsPropertyHelper<bool> hasErrors;
        private readonly ObservableAsPropertyHelper<string> errors;

        private string hostname, user, password;

        public SettingsViewModel(IConfigurationService configurationService)
        {
            if (configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }

            this.ConfigurationService = configurationService;
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

            this.SaveCommand = new ReactiveCommand(hasErrorsObservable.Select(e => !e));
            this.SaveCommand.Subscribe(_ => this.Save());
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        public ReactiveCommand SaveCommand { get; private set; }

        private void Save()
        {
            var configuration = new Configuration()
            {
                HostName = this.hostname,
                Password = this.password,
                UserName = this.user
            };
            this.ConfigurationService.SaveConfiguration(configuration);
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
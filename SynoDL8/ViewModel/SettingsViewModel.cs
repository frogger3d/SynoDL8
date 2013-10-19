using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace SynoDL8.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public const string HostnameKey = "hostname";
        public const string UserKey = "user";
        public const string PasswordKey = "password";

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsViewModel()
        {
            this.Hostname = localSettings.Values[HostnameKey] as string ?? @"http://example.com";
            this.User = localSettings.Values[UserKey] as string ?? "user";
            this.Password = localSettings.Values[PasswordKey] as string ?? "password";
            this.SaveCommand = new RelayCommand(this.Save);
        }

        public string Hostname { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public ICommand SaveCommand { get; set; }

        private void Save()
        {
            localSettings.Values[HostnameKey] = this.Hostname;
            localSettings.Values[UserKey] = this.User;
            localSettings.Values[PasswordKey] = this.Password;
        }
    }
}

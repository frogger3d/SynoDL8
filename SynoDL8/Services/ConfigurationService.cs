using SynoDL8.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SynoDL8.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public const string HostnameKey = "hostname";
        public const string UserKey = "user";
        public const string PasswordKey = "password";

        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public event EventHandler Changed;

        public Credentials GetLastCredentials()
        {
            return new Credentials()
            {
                Hostname = localSettings.Values[HostnameKey] as string,
                User = localSettings.Values[UserKey] as string,
                Password = localSettings.Values[PasswordKey] as string,
            };
        }
        public void SaveCredentials(Credentials credentials)
        {
            localSettings.Values[HostnameKey] = credentials.Hostname;
            localSettings.Values[UserKey] = credentials.User;
            localSettings.Values[PasswordKey] = credentials.Password;
            
            var handle = this.Changed;
            if(handle != null)
            {
                handle(this, EventArgs.Empty);
            }
        }
    }
}

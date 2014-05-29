using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SynoDL8.Model
{
    public class ConfigurationService : IConfigurationService
    {
        public const string HostnameKey = "hostname";
        public const string UserKey = "user";
        public const string PasswordKey = "password";

        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public event EventHandler Changed;

        public Credentials GetConfiguration()
        {
            return new Credentials()
            {
                Hostname = localSettings.Values[HostnameKey] as string,
                User = localSettings.Values[UserKey] as string,
                Password = localSettings.Values[PasswordKey] as string,
            };
        }
        public void SaveConfiguration(Credentials configuration)
        {
            localSettings.Values[HostnameKey] = configuration.Hostname;
            localSettings.Values[UserKey] = configuration.User;
            localSettings.Values[PasswordKey] = configuration.Password;
            
            var handle = this.Changed;
            if(handle != null)
            {
                handle(this, EventArgs.Empty);
            }
        }
    }
}

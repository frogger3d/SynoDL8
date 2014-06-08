using Newtonsoft.Json.Linq;
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
        public const string AllKey = "all";
        public const string LastKey = "last";

        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public Credentials GetLastCredentials()
        {
            var credentialsJason = localSettings.Values[LastKey] as string;
            if(credentialsJason == null)
            {
                return null;
            }
            return Credentials.FromJason(credentialsJason);
        }

        public void SaveCredentials(Credentials credentials)
        {
            var allCredentials = this.GetAllCredentials();
            allCredentials.Add(credentials);

            localSettings.Values[LastKey] = credentials.ToJason();
            localSettings.Values[AllKey] = Credentials.ToJason(allCredentials);
        }


        public HashSet<Credentials> GetAllCredentials()
        {
            var allCredentialsJason = localSettings.Values[AllKey] as string;
            HashSet<Credentials> allCredentials = null;
            if (allCredentialsJason != null)
            {
                allCredentials = Credentials.EnumerableFromJason(allCredentialsJason);
            }

            return allCredentials ?? new HashSet<Credentials>();
        }

        public void RemoveCredentials(Credentials credentials)
        {
            var allCredentials = localSettings.Values[AllKey] as HashSet<Credentials>;
            if (allCredentials == null)
            {
                allCredentials = new HashSet<Credentials>();
            }

            allCredentials.Remove(credentials);

            localSettings.Values[AllKey] = Credentials.ToJason(allCredentials);
            if (GetLastCredentials().Equals(credentials))
            {
                localSettings.Values[LastKey] = null;
            }
        }
    }
}

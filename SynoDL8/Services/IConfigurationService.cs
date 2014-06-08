using SynoDL8.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SynoDL8.Services
{
    public interface IConfigurationService
    {
        Credentials GetLastCredentials();
        HashSet<Credentials> GetAllCredentials();
        void RemoveCredentials(Credentials credentials);
        void SaveCredentials(Credentials credentials);
    }
}

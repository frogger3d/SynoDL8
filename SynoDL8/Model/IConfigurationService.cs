using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SynoDL8.Model
{
    public interface IConfigurationService
    {
        Credentials GetConfiguration();
        void SaveConfiguration(Credentials credentials);
        event EventHandler Changed;
    }
}

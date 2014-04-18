using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SynoDL8.DataModel
{
    public interface IConfigurationService
    {
        Configuration GetConfiguration();
        void SaveConfiguration(Configuration configuration);
        event EventHandler Changed;
    }
}

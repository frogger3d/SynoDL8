using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynoDL8.DataModel
{
    public interface IDataModel
    {
        Task<bool> Login();
        Task<bool> Logout();
        Task<string> GetVersions();
        Task<string> GetInfo();
        Task<string> Create(string url);
        Task<IEnumerable<DownloadTask>> List();
    }
}

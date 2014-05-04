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
        Task<Statistics> GetStatistics();
        Task<SynologyResponse> Create(string url);

        Task<bool> Pause(string taskid);
        Task<bool> Resume(string taskid);
        Task<bool> Delete(string taskid);

        Task<IEnumerable<DownloadTask>> List();
    }
}

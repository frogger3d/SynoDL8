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
        Task<bool> LoginAsync();
        Task<bool> LogoutAsync();
        Task<string> GetVersionsAsync();
        Task<string> GetInfoAsync();
        Task<Statistics> GetStatisticsAsync();
        Task<SynologyResponse> CreateDownloadAsync(string url);

        Task<bool> Pause(string taskid);
        Task<bool> Resume(string taskid);
        Task<bool> Delete(string taskid);

        Task<IEnumerable<DownloadTask>> List();
    }
}

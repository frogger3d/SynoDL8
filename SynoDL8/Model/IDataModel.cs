using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynoDL8.Model
{
    public interface IDataModel
    {
        Task<bool> LoginAsync();
        Task<bool> LogoutAsync();
        Task<string> GetVersionsAsync();
        Task<string> GetInfoAsync();
        Task<Statistics> GetStatisticsAsync();
        Task<SynologyResponse> CreateTaskAsync(string url);

        Task<bool> PauseTaskAsync(string taskid);
        Task<bool> ResumeTaskAsync(string taskid);
        Task<bool> DeleteTaskAsync(string taskid);

        Task<IEnumerable<DownloadTask>> List();
    }
}

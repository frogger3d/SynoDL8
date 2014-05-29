using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.Model
{
    internal class DesignDataModel : IDataModel
    {
        public Task<bool> LoginAsync(Credentials credentials)
        {
            return default(Task<bool>);
        }

        public Task<bool> LogoutAsync()
        {
            return default(Task<bool>);
        }

        public Task<string> GetVersionsAsync()
        {
            return new Task<string>(() => "Lorem ipsum");
        }

        public Task<string> GetInfoAsync()
        {
            return new Task<string>(() => "Lorem ipsum");
        }

        public Task<Statistics> GetStatisticsAsync()
        {
            return new Task<Statistics>(() => new Statistics(1000,45000));
        }

        public Task<SynologyResponse> CreateTaskAsync(string url)
        {
            return default(Task<SynologyResponse>);
        }

        public Task<IEnumerable<DownloadTask>> List()
        {
            return new Task<IEnumerable<DownloadTask>>(() => CreateFakeList());
        }

        private IEnumerable<DownloadTask> CreateFakeList()
        {
            yield return new DownloadTask() { Title = "Lorem ipsum" };
            yield return new DownloadTask() { Title = "Lorem ipsum" };
            yield return new DownloadTask() { Title = "Lorem ipsum" };
        }

        public Task<bool> PauseTaskAsync(string taskid)
        {
            return new Task<bool>(() => true);
        }

        public Task<bool> ResumeTaskAsync(string taskid)
        {
            return new Task<bool>(() => true);
        }

        public Task<bool> DeleteTaskAsync(string taskid)
        {
            return new Task<bool>(() => true);
        }
    }
}

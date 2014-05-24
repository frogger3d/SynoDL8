using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.DataModel
{
    internal class DesignDataModel : IDataModel
    {
        public Task<bool> LoginAsync()
        {
            return new Task<bool>(() => true);
        }

        public Task<bool> LogoutAsync()
        {
            return new Task<bool>(() => true);
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

        public Task<SynologyResponse> CreateDownloadAsync(string url)
        {
            return new Task<SynologyResponse>(() => new SynologyResponse());
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

        public Task<bool> Pause(string taskid)
        {
            return new Task<bool>(() => true);
        }

        public Task<bool> Resume(string taskid)
        {
            return new Task<bool>(() => true);
        }

        public Task<bool> Delete(string taskid)
        {
            return new Task<bool>(() => true);
        }
    }
}

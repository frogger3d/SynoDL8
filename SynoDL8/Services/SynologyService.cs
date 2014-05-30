namespace SynoDL8.Services
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SynoDL8.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;

    public class SynologyService : ISynologyService
    {
        const string LoginQuery = @"{0}/webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account={1}&passwd={2}&session=DownloadStation&format=cookie";

        const string LogoutQuery = @"{0}/webapi/auth.cgi?api=SYNO.API.Auth&version=1&method=logout&session=DownloadStation";

        const string VersionQuery = @"{0}/webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task,SYNO.DownloadStation.Info,SYNO.DownloadStation.Schedule,SYNO.DownloadStation.Statistic";

        const string InfoQuery = @"{0}/webapi/DownloadStation/info.cgi?api=SYNO.DownloadStation.Info&version=1&method=getinfo";

        const string StatisticsQuery = @"{0}/webapi/DownloadStation/statistic.cgi?api=SYNO.DownloadStation.Statistic&version=1&method=getinfo";

        const string ListQuery = @"{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=list&additional=transfer,detail";

        const string CreateUri = @"{0}/webapi/DownloadStation/task.cgi";
        const string CreateRequest = "api=SYNO.DownloadStation.Task&version=1&method=create&uri={0}";

        const string PauseQuery = @"{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=pause&id={1}";
        const string ResumeQuery = @"{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=resume&id={1}";
        const string DeleteQuery = @"{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=delete&id={1}&force_complete=false";
        private readonly CookieContainer Cookies;

        private readonly HttpClient HttpClient;

        private Credentials credentials;

        public SynologyService(IConfigurationService configurationService)
        {
            this.HttpClient = new HttpClient();
            this.Cookies = new CookieContainer();
        }

        public async Task<bool> LoginAsync(Credentials credentials)
        {
            var re = await Observable.FromAsync(() => this.GetAsync(string.Format(LoginQuery, credentials.Hostname, credentials.User, credentials.Password)))
                                     .Select(r => SynologyResponse.FromJason(r, SynologyResponse.GetAuthError))
                                     .Timeout(TimeSpan.FromSeconds(5))
                                     .Retry(3);
            if (!re.Success)
            {
                throw new VerificationException(re.Error);
            }

            this.credentials = credentials;
            return true;
        }

        public Task<bool> LogoutAsync()
        {
            CheckSignedin();
            return this.GetAsync(string.Format(LogoutQuery, this.credentials.Hostname))
                       .IsSuccess(SynologyResponse.GetAuthError);
        }

        public async Task<string> GetVersionsAsync()
        {
            this.CheckSignedin();
            return Format(await this.GetAsync(string.Format(VersionQuery, this.credentials.Hostname)));
        }

        public async Task<string> GetInfoAsync()
        {
            this.CheckSignedin();
            return Format(await this.GetAsync(string.Format(InfoQuery, this.credentials.Hostname)));
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            this.CheckSignedin();
            var response = await this.GetAsync(string.Format(StatisticsQuery, this.credentials.Hostname)).ToResponse();
            return Statistics.FromResponse(response);
        }

        public async Task<IEnumerable<DownloadTask>> List()
        {
            this.CheckSignedin();
            var response = await this.GetAsync(string.Format(ListQuery, this.credentials.Hostname));
            return DownloadTask.FromJason(response, this);
        }

        public async Task<SynologyResponse> CreateTaskAsync(string url)
        {
            this.CheckSignedin();
            return await this.PostAsync(string.Format(CreateUri, this.credentials.Hostname), string.Format(CreateRequest, url)).ToResponse();
        }

        public async Task<bool> ResumeTaskAsync(string taskid)
        {
            this.CheckSignedin();
            return await this.GetAsync(string.Format(ResumeQuery, this.credentials.Hostname, taskid)).IsSuccess();
        }

        public async Task<bool> PauseTaskAsync(string taskid)
        {
            this.CheckSignedin();
            return await this.GetAsync(string.Format(PauseQuery, this.credentials.Hostname, taskid)).IsSuccess();
        }

        public async Task<bool> DeleteTaskAsync(string taskid)
        {
            this.CheckSignedin();
            return await this.GetAsync(string.Format(DeleteQuery, this.credentials.Hostname, taskid)).IsSuccess();
        }

        private async Task<string> PostAsync(string url, string content)
        {
            var response = await this.HttpClient.PostAsync(url, new StringContent(content));
            response.EnsureSuccessStatusCode();
            // workaround for malformed header in Synology response
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        private async Task<string> GetAsync(string url)
        {
            var response = await this.HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            // workaround for malformed header in Synology response
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        private static string Format(string json)
        {
            JToken jt = JToken.Parse(json);
            return jt.ToString(Formatting.Indented);
        }

        private void CheckSignedin()
        {
            if(this.credentials == null)
            {
                throw new InvalidOperationException("Not signed in");
            }
        }
    }
}

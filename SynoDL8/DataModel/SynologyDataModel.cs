namespace SynoDL8.DataModel
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SynologyDataModel : IDataModel, IDisposable
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

        private readonly IConfigurationService ConfigurationService;

        private string host;
        private string user;
        private string password;
        
        public SynologyDataModel(IConfigurationService configurationService)
        {
            this.HttpClient = new HttpClient();
            this.Cookies = new CookieContainer();

            this.ConfigurationService = configurationService.ThrowIfNull("configurationService");
            this.ConfigurationService.Changed += ConfigurationService_Changed;
            this.UpdateConfiguration();
        }

        public void Dispose()
        {
            this.ConfigurationService.Changed -= this.ConfigurationService_Changed;
        }

        public async Task<bool> LoginAsync()
        {
            return await Observable.FromAsync(() => this.GetAsync(string.Format(LoginQuery, this.host, this.user, this.password)))
                                   .Select(r => SynologyResponseMixins.IsSuccess(r, SynologyResponse.GetAuthError))
                                   .Timeout(TimeSpan.FromSeconds(3))
                                   .Retry(3);
                             //.Subscribe(next => next, error => false);
        }

        public Task<bool> LogoutAsync()
        {
            return this.GetAsync(string.Format(LogoutQuery, host))
                       .IsSuccess(SynologyResponse.GetAuthError);
        }

        public async Task<string> GetVersionsAsync()
        {
            return Format(await this.GetAsync(string.Format(VersionQuery, host)));
        }

        public async Task<string> GetInfoAsync()
        {
            return Format(await this.GetAsync(string.Format(InfoQuery, host)));
        }

        public async Task<Statistics> GetStatisticsAsync()
        {
            var response = await this.GetAsync(string.Format(StatisticsQuery, host)).ToResponse();
            return Statistics.FromResponse(response);
        }

        public Task<IEnumerable<DownloadTask>> List()
        {
            return this.GetAsync(string.Format(ListQuery, host))
                       .ContinueWith(t => DownloadTask.FromJason(t.Result, this));
        }

        public Task<SynologyResponse> CreateDownloadAsync(string url)
        {
            return this.PostAsync(string.Format(CreateUri, host), string.Format(CreateRequest, url)).ToResponse();
        }

        public Task<bool> Resume(string taskid)
        {
            return this.GetAsync(string.Format(ResumeQuery, host, taskid)).IsSuccess();
        }

        public Task<bool> Pause(string taskid)
        {
            return this.GetAsync(string.Format(PauseQuery, host, taskid)).IsSuccess();
        }

        public Task<bool> Delete(string taskid)
        {
            return this.GetAsync(string.Format(DeleteQuery, host, taskid)).IsSuccess();
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

        private void ConfigurationService_Changed(object sender, EventArgs e)
        {
            this.UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            var configuration = this.ConfigurationService.GetConfiguration();
            this.host = (configuration.HostName ?? "").TrimEnd('/');
            this.user = configuration.UserName;
            this.password = configuration.Password;
        }
    }
}

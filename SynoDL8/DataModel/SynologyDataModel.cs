namespace SynoDL8.DataModel
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SynoDL8.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class SynologyDataModel : IDataModel, IDisposable
    {
        const string LoginQuery = @"http://{0}/webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account={1}&passwd={2}&session=DownloadStation&format=cookie";

        const string LogoutQuery = @"http://{0}/webapi/auth.cgi?api=SYNO.API.Auth&version=1&method=logout&session=DownloadStation";

        const string VersionQuery = @"http://{0}/webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task,SYNO.DownloadStation.Info,SYNO.DownloadStation.Schedule,SYNO.DownloadStation.Statistic";

        const string InfoQuery = @"http://{0}/webapi/DownloadStation/info.cgi?api=SYNO.DownloadStation.Info&version=1&method=getinfo";

        const string ListQuery = @"http://{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=list&additional=transfer,detail";

        const string CreateUri = @"http://{0}/webapi/DownloadStation/task.cgi";
        const string CreateRequest = "api=SYNO.DownloadStation.Task&version=1&method=create&uri={0}";

        const string PauseQuery = @"http://{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=pause&id={1}";
        const string ResumeQuery = @"http://{0}/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=resume&id={1}";

        private static readonly CookieContainer Cookies = new CookieContainer();

        private readonly IConfigurationService ConfigurationService;

        private string host;
        private string user;
        private string password;
        
        public SynologyDataModel(IConfigurationService configurationService)
        {
            if(configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }

            this.ConfigurationService = configurationService;
            this.ConfigurationService.Changed += ConfigurationService_Changed;
            this.UpdateConfiguration();
        }

        public void Dispose()
        {
            this.ConfigurationService.Changed -= this.ConfigurationService_Changed;
        }

        public Task<bool> Login()
        {
            return SynologyDataModel.MakeAsyncRequest(string.Format(LoginQuery, this.host, this.user, this.password))
                            .ContinueWith(t =>
                            {
                                var o = JObject.Parse(t.Result);
                                return (bool)o["success"];
                            });
        }

        public Task<bool> Logout()
        {
            return SynologyDataModel.MakeAsyncRequest(string.Format(LogoutQuery, host))
                            .ContinueWith(t =>
                            {
                                var o = JObject.Parse(t.Result);
                                return (bool)o["success"];
                            });
        }

        public Task<string> GetVersions()
        {
            return MakeAsyncRequest(string.Format(VersionQuery, host));
        }

        public Task<string> GetInfo()
        {
            return MakeAsyncRequest(string.Format(InfoQuery, host));
        }

        public Task<IEnumerable<DownloadTask>> List()
        {
            return MakeAsyncRequest(string.Format(ListQuery, host))
                      .ContinueWith(t => DownloadTask.FromJason(t.Result, this));
        }

        public Task<string> Create(string url)
        {
            return MakeAsyncRequest(string.Format(CreateUri, host), method: "POST", req: string.Format(CreateRequest, url));
        }

        public Task<string> Resume(string taskid)
        {
            return MakeAsyncRequest(string.Format(ResumeQuery, host, taskid));
        }

        public Task<string> Pause(string taskid)
        {
            return MakeAsyncRequest(string.Format(PauseQuery, host, taskid));
        }

        private static async Task<string> MakeAsyncRequest(string url, string method = "GET", string req = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = Cookies;
            request.ContinueTimeout = 10;
            request.Method = method;
            if (req != null)
            {
                using (Stream requestStream = await request.GetRequestStreamAsync())
                using (StreamWriter sw = new StreamWriter(requestStream))
                {
                    await sw.WriteAsync(req);
                }
            }

            WebResponse response = await request.GetResponseAsync();
            return ReadStreamFromResponse(response);
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string strContent = sr.ReadToEnd();
                return Format(strContent);
            }
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
            this.host = configuration.HostName.TrimEnd('/');
            this.user = configuration.UserName;
            this.password = configuration.Password;
        }
    }
}

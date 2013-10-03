namespace SynoDL8.DataModel
{
    using Newtonsoft.Json.Linq;
    using SynoDL8.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class DSQuerier
    {
        /// <summary>
        /// auth query. string format {0} = account, string format {1} is passwd
        /// </summary>
        const string LoginQuery = @"/webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account={0}&passwd={1}&session=DownloadStation&format=cookie";

        const string LogoutQuery = @"/webapi/auth.cgi?api=SYNO.API.Auth&version=1&method=logout&session=DownloadStation";
        
        const string VersionQuery = @"/webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task,SYNO.DownloadStation.Info,SYNO.DownloadStation.Schedule,SYNO.DownloadStation.Statistic";

        const string InfoQuery = @"/webapi/DownloadStation/info.cgi?api=SYNO.DownloadStation.Info&version=1&method=getinfo";
        
        const string ListQuery = @"/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=list&additional=transfer,detail";

        const string CreateUri = @"/webapi/DownloadStation/task.cgi";
        const string CreateRequest = "api=SYNO.DownloadStation.Task&version=1&method=create&uri={0}";

        const string PauseQuery = @"/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=pause&id={0}";
        const string ResumeQuery = @"/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=resume&id={0}";

        private readonly string Host;
        private readonly string User;
        private readonly string Password;
        private static readonly CookieContainer Cookies = new CookieContainer();

        public DSQuerier(string host, string user, string password)
        {
            this.Host = host.TrimEnd('/');
            this.User = user;
            this.Password = password;
        }

        public Task<bool> Login()
        {
            return DSQuerier.MakeAsyncRequest(Host + string.Format(LoginQuery, this.User, this.Password))
                            .ContinueWith(t =>
                            {
                                var o = JObject.Parse(t.Result);
                                return (bool)o["success"];
                            });
        }

        public Task<bool> Logout()
        {
            return DSQuerier.MakeAsyncRequest(Host + LogoutQuery)
                            .ContinueWith(t =>
                            {
                                var o = JObject.Parse(t.Result);
                                return (bool)o["success"];
                            });
        }

        public Task<string> GetVersions()
        {
            return MakeAsyncRequest(Host + VersionQuery);
        }

        public Task<string> GetInfo()
        {
            return MakeAsyncRequest(Host + InfoQuery);
        }

        public Task<IEnumerable<DownloadTask>> List()
        {
            return MakeAsyncRequest(Host + ListQuery)
                      .ContinueWith(t => DownloadTask.FromJason(t.Result, this));
        }

        public Task<string> Create(string url)
        {
            return MakeAsyncRequest(Host + CreateUri, method: "POST", req: string.Format(CreateRequest, url));
        }

        public Task<string> Resume(string taskid)
        {
            return MakeAsyncRequest(Host + string.Format(ResumeQuery, taskid));
        }

        public Task<string> Pause(string taskid)
        {
            return MakeAsyncRequest(Host + string.Format(PauseQuery, taskid));
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
            return jt.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}

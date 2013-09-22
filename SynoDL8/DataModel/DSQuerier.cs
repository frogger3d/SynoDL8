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
        const string VersionQuery = @"/webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task,SYNO.DownloadStation.Info,SYNO.DownloadStation.Schedule,SYNO.DownloadStation.Statistic";

        const string DLInfoQuery = @"/webapi/DownloadStation/info.cgi?api=SYNO.DownloadStation.Info&version=1&method=getinfo";
        /// <summary>
        /// auth query. string format {0} = account, string format {1} is passwd
        /// </summary>
        const string AuthQuery = @"/webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account={0}&passwd={1}&session=DownloadStation&format=cookie";

        const string LogoutQuery = @"/webapi/auth.cgi?api=SYNO.API.Auth&version=1&method=logout&session=DownloadStation";

        const string ListQuery = @"/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=list&additional=transfer,detail";

        // webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task
        // webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account=admin&passwd=12345&session=DownloadStation&format=cookie
        // webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=list

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

        public Task<string> GetVersions()
        {
            return MakeAsyncRequest(Host + VersionQuery);
        }

        public Task<string> GetInfo()
        {
            return MakeAsyncRequest(Host + DLInfoQuery);
        }

        public Task<bool> Authenticate()
        {
            return DSQuerier.MakeAsyncRequest(Host + string.Format(AuthQuery, this.User, this.Password))
                            .ContinueWith(t => 
                                {
                                    var o = JObject.Parse(t.Result);
                                    return (bool)o["success"];
                                });
        }

        public Task<bool> Logout()
        {
            return DSQuerier.MakeAsyncRequest(Host + string.Format(LogoutQuery))
                            .ContinueWith(t =>
                                {
                                    var o = JObject.Parse(t.Result);
                                    return (bool)o["success"];
                                });
        }

        public Task<IEnumerable<DownloadTask>> List()
        {
            return MakeAsyncRequest(Host + ListQuery)
                      .ContinueWith(t => DownloadTask.FromJason(t.Result));
        }

        private static async Task<string> MakeAsyncRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = Cookies;
            request.ContinueTimeout = 10;

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

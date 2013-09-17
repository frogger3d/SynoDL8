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

namespace SynoDL8
{
    internal class DSQuerier
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

        /*
{
  "data": {
    "sid": "oaRmWLvQP8CG6"
  },
  "success": true
}
         */
        public Task<bool> Authenticate()
        {
            return MakeAsyncRequest(Host + string.Format(AuthQuery, this.User, this.Password))
                      .ContinueWith(t => 
                          {
                              var o = JObject.Parse(t.Result);
                              return (bool)o["success"];
                          });
        }

        public Task<string> Logout()
        {
            return MakeAsyncRequest(Host + string.Format(LogoutQuery));
        }

        /*
{
  "data": {
    "offeset": 0,
    "tasks": [
      {
        "additional": {
          "detail": {
            "connected_leechers": 1,
            "connected_seeders": 5,
            "create_time": "1368478229",
            "destination": "Video",
            "priority": "auto",
            "total_peers": 0,
            "uri": "http://fenopy.eu/torrent/Iron-Man-3-2013-DVDRIP-x264-Xvid/MTAxNTY1ODk=/download.torrent"
          },
          "transfer": {
            "size_downloaded": "366086778",
            "size_uploaded": "14661613",
            "speed_download": 717737,
            "speed_upload": 49191
          }
        },
        "id": "dbid_85",
        "size": "732957306",
        "status": "downloading",
        "status_extra": null,
        "title": "Iron.Man.3.2013.DVDRIP..x264.Xvid",
        "type": "bt",
        "username": "sax"
      }
    ],
    "total": 1
  },
         */
        public Task<string> List()
        {
            var requestTask  = MakeAsyncRequest(Host + ListQuery);

            var o = JObject.Parse(requestTask.Result);
            //var firstTitle = o["data"]["tasks"][0]["title"];

            return requestTask;
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

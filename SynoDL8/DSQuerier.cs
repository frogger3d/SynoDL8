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
        const string infoquery = @"webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task";
        /// <summary>
        /// auth query. string format {0} = account, string format {1} is passwd
        /// </summary>
        const string authquery = @"webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account={0}&passwd={1}&session=DownloadStation&format=cookie";

        // http://myds.com/webapi/query.cgi?api=SYNO.API.Info&version=1&method=query&query=SYNO.API.Auth,SYNO.DownloadStation.Task
        // http://myds.com/webapi/auth.cgi?api=SYNO.API.Auth&version=2&method=login&account=admin&passwd=12345&session=DownloadStation&format=cookie
        // http://myds.com/webapi/DownloadStation/task.cgi?api=SYNO.DownloadStation.Task&version=1&method=list

        private readonly string Host;

        public DSQuerier(string host)
        {
            this.Host = host;
        }

        public Task<string> GetDSVersions()
        {
            return MakeAsyncRequest(Host + infoquery);
        }

        // Define other methods and classes here
        private static Task<string> MakeAsyncRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContinueTimeout = 10;

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }
    }
}

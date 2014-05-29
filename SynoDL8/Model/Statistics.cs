using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.Model
{
    public class Statistics
    {
        public readonly int DownloadSpeed;
        public readonly int UploadSpeed;

        public Statistics(int download, int upload)
        {
            this.DownloadSpeed = download;
            this.UploadSpeed = upload;
        }

        internal static Statistics FromResponse(SynologyResponse response)
        {
            if (response.Success)
            {
                var jobject = JObject.Parse(response.Content);
                return new Statistics(
                    (int)(jobject["data"]["speed_download"]),
                    (int)(jobject["data"]["speed_upload"]));
            }
            else
            {
                return null;
            }
        }
    }
}

namespace SynoDL8.DataModel
{
    using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    /*
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
}
     */
    public class DownloadTask
    {
        public string Id { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Todo: find out all statuses
        /// </summary>
        public string Status { get; set; }

        public long Size { get; set; }
        public long SizeUploaded { get; set; }
        public long SizeDownloaded { get; set; }
        public long SpeedUpload { get; set; }
        public long SpeedDownload { get; set; }

        public double Progress
        {
            get { return (double)this.SizeDownloaded / this.Size; }
        }

        public override string ToString()
        {
            return this.Title + " " + this.Id + " " + this.Status + " " + string.Format("{0:0.00%}", this.Progress);
        }

        public static IEnumerable<DownloadTask> FromJason(string jasonString)
        {
            var root = JObject.Parse(jasonString);
            if ((bool)root["success"])
            {
                foreach (var task in root["data"]["tasks"])
                {
                    var additional = task["additional"];
                    var transfer = additional["transfer"];
                    yield return new DownloadTask()
                    {
                        Id = (string)task["id"],
                        Title = (string)task["title"],
                        Size = (long)task["size"],
                        Status = (string)task["status"],
                        SizeUploaded = (long)transfer["size_uploaded"],
                        SizeDownloaded = (long)transfer["size_downloaded"],
                        SpeedUpload = (long)transfer["speed_upload"],
                        SpeedDownload = (long)transfer["speed_download"],
                    };
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to get download list");
            }
        }
    }
}

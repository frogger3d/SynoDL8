namespace SynoDL8.DataModel
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

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

        public ICommand PlayCommand { get; set; }
        public ICommand PauseCommand { get; set; }

        public override string ToString()
        {
            return this.Title + " " + this.Id + " " + this.Status + " " + string.Format("{0:0.00%}", this.Progress);
        }

        public static IEnumerable<DownloadTask> FromJason(string jasonString, DSQuerier dsquerier)
        {
            var root = JObject.Parse(jasonString);
            if ((bool)root["success"])
            {
                foreach (var task in root["data"]["tasks"])
                {
                    var additional = task["additional"];
                    var transfer = additional["transfer"];

                    string taskid = (string)task["id"];
                    yield return new DownloadTask()
                    {
                        Id = taskid,
                        Title = (string)task["title"],
                        Size = (long)task["size"],
                        Status = (string)task["status"],
                        SizeUploaded = (long)transfer["size_uploaded"],
                        SizeDownloaded = (long)transfer["size_downloaded"],
                        SpeedUpload = (long)transfer["speed_upload"],
                        SpeedDownload = (long)transfer["speed_download"],
                        PlayCommand = new RelayCommand(() => dsquerier.Resume(taskid)),
                        PauseCommand = new RelayCommand(() => dsquerier.Pause(taskid)),
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

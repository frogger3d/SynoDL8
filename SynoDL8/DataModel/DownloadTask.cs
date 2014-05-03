namespace SynoDL8.DataModel
{
    using Newtonsoft.Json.Linq;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class DownloadTask
    {
        public enum status
        {
            unknown, // NOT IN OFFICIAL SDK
            waiting,
            downloading,
            paused,
            finishing,
            finished,
            hash_checking,
            seeding,
            filehosting_waiting,
            extracting,
            error,
        };

        public enum err_detail
        {
            none, // NOT IN OFFICIAL SDK
            broken_link,
            destination_not_exist,
            destination_denied,
            disk_full,
            quota_reached,
            timeout,
            exceed_max_file_system_size,
            exceed_max_destination_size,
            exceed_max_temp_size,
            encrypted_name_too_long,
            name_too_long,
            torrent_duplicate,
            file_not_exist,
            required_premium_account,
            not_supported_type,
            ftp_encryption_not_supported_type,
            extract_failed,
            extract_failed_wrong_password,
            extract_failed_invalid_archive,
            extract_failed_quota_reached,
            extract_failed_disk_full,
            unknown,
        };

        public string Id { get; set; }

        public string Title { get; set; }

        public status Status { get; set; }

        public err_detail ErrorInfo { get; set; }

        public long Size { get; set; }
        public long SizeUploaded { get; set; }
        public long SizeDownloaded { get; set; }
        public long SpeedUpload { get; set; }
        public long SpeedDownload { get; set; }

        public static IEnumerable<DownloadTask> FromJason(string jasonString, SynologyDataModel dsquerier)
        {
            var root = JObject.Parse(jasonString);
            if (SynologyResponse.FromJason(jasonString).Success)
            {
                foreach (var task in root["data"]["tasks"])
                {
                    var additional = task["additional"];
                    var transfer = additional["transfer"];

                    string taskid = (string)task["id"];
                    DownloadTask downloadTask = new DownloadTask()
                    {
                        Id = taskid,
                        Title = (string)task["title"],
                        Size = (long)task["size"],
                        Status = (status)Enum.Parse(typeof(status), (string)task["status"]),
                        SizeUploaded = (long)transfer["size_uploaded"],
                        SizeDownloaded = (long)transfer["size_downloaded"],
                        SpeedUpload = (long)transfer["speed_upload"],
                        SpeedDownload = (long)transfer["speed_download"],
                    };

                    switch (downloadTask.Status)
                    {
                        case status.error:
                            var status_extra = task["status_extra"];
                            downloadTask.ErrorInfo = (err_detail)Enum.Parse(typeof(err_detail), (string)status_extra["error_detail"]);
                            break;
                    }


                    yield return downloadTask;
                }
            }
        }
    }
}

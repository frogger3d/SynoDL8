using ReactiveUI;
using SynoDL8.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SynoDL8.ViewModel
{
    class DesignMainViewModel: IMainViewModel
    {
        private readonly IDataModel DataModel;
        private ReactiveList<DownloadTaskViewModel> downloadTasks;

        public DesignMainViewModel(IDataModel datamodel)
        {
            this.DataModel = datamodel;
            this.downloadTasks = new ReactiveList<DownloadTaskViewModel>();
            this.FillContentAsync();
        }

        private async void FillContentAsync()
        {
            var tasks = await this.DataModel.List();
            var vms = tasks.Select(t => new DownloadTaskViewModel(this.DataModel, t));
            this.downloadTasks.AddRange(vms);
        }

        private static DownloadTask GetFakeDownloadTask()
        {
            return new DownloadTask() { Size = 10000, Title = "Foo", Status = DownloadTask.status.downloading, };
        }

        public ReactiveCommand VersionsCommand { get { return null; } }
        public ReactiveCommand InfoCommand { get { return null; } }
        public ReactiveCommand ListCommand { get { return null; } }
        public ReactiveCommand CreateCommand { get { return null; } }
        
        public string Url { get; set; }
        public string Message { get { return null; } }

        public ReactiveList<DownloadTaskViewModel> Content
        {
            get { return this.downloadTasks; }
        }

        public string UploadSpeed
        {
            get { return "45"; }
        }

        public string DownloadSpeed
        {
            get { return "560"; }
        }
    }
}

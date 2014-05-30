namespace SynoDL8.ViewModels
{
    using ReactiveUI;
    using SynoDL8.Model;
    using SynoDL8.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;

    public interface IMainViewModel
    {
        ReactiveCommand VersionsCommand { get; }
        ReactiveCommand InfoCommand { get; }
        ReactiveCommand ListCommand { get; }
        ReactiveCommand CreateCommand { get; }

        string HostInfo { get; }
        string Url { get; set; }
        string Message { get; }
        ReactiveList<DownloadTaskViewModel> Content { get; }

        string UploadSpeed { get; }
        string DownloadSpeed { get; }
    }
}

using ReactiveUI;
using ReactiveUI.Xaml;
using SynoDL8.Model;
using SynoDL8.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace SynoDL8.DesignViewModels
{
    public class DownloadTaskDesignViewModel
    {
        public string Title { get { return "Download stuffs.zip"; } }
        public string Status { get { return "paused"; } }
        public string VisualState { get { return "Paused"; } }
        public bool Busy { get { return false; } }
        public double Progress { get { return 0.4; } }
        public bool IsActive { get { return true; } }
    }
}

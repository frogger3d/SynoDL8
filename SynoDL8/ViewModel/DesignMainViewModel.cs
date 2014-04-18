using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Windows.UI.Xaml;

namespace SynoDL8.ViewModel
{
    class DesignMainViewModel: IMainViewModel
    {
        public ReactiveCommand AuthCommand { get { return null; } }
        public ReactiveCommand LogoutCommand { get { return null; } }
        public ReactiveCommand VersionsCommand { get { return null; } }
        public ReactiveCommand InfoCommand { get { return null; } }
        public ReactiveCommand ListCommand { get { return null; } }
        public ReactiveCommand CreateCommand { get { return null; } }
        public string Url { get; set; }

        public bool Busy { get { return true; } }

        public Visibility BusyV { get { return Visibility.Visible; } }
        public string Message { get { return null; } }
        public object Content { get { return null; } }
    }
}

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SynoDL8.ViewModel
{
    public interface ILoginViewModel
    {
        ReactiveCommand SigninCommand { get; }
        bool Busy { get; }
        Visibility BusyV { get; }
        string User { get; set; }
        string Hostname { get; set; }
        string Password { get; set; }
        string Errors { get; }
        string SigninError { get; }
    }
}

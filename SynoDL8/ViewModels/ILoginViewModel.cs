using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SynoDL8.ViewModels
{
    public interface ILoginViewModel
    {
        ReactiveCommand SigninCommand { get; }
        bool Busy { get; }
        Visibility BusyV { get; }
        string SigninError { get; }
    }
}

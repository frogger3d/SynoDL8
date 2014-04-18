using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.ViewModel
{
    public interface ILoginViewModel
    {
        ReactiveCommand SigninCommand { get; }
        string User { get; set; }
        string Hostname { get; set; }
        string Password { get; set; }
    }
}

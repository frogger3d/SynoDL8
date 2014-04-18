﻿namespace SynoDL8.ViewModel
{
    using ReactiveUI;
    using SynoDL8.DataModel;
    using SynoDL8.ViewModel;
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
        ReactiveCommand AuthCommand { get; }
        ReactiveCommand LogoutCommand { get; }
        ReactiveCommand VersionsCommand { get; }
        ReactiveCommand InfoCommand { get; }
        ReactiveCommand ListCommand { get; }
        ReactiveCommand CreateCommand { get; }

        string Url { get;set;}
        bool Busy { get; }
        Visibility BusyV { get; }
        string Message { get; }
        object Content { get; }
    }
}

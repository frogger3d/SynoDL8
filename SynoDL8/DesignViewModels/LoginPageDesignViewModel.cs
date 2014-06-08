using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ReactiveUI;
using SynoDL8.Model;
using SynoDL8.Services;
using SynoDL8.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SynoDL8.DesignViewModels
{
    public class LoginPageDesignViewModel
    {
        public LoginPageDesignViewModel()
        {
            this.Previous = new List<Credentials>();
            foreach(var i in Enumerable.Range(0,10))
            {
                this.Previous.Add(new Credentials() { User = "User" + i, Hostname = @"http://host" + i });
            }

            this.Credentials = this.Previous.Last();
        }

        public Credentials Credentials { get; set; }
        public List<Credentials> Previous { get; set; }
    }
}

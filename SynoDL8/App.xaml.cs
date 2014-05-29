using SynoDL8.Common;
using SynoDL8.Views;
using SynoDL8.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Microsoft.Practices.Prism.StoreApps;
using SynoDL8.Model;
using System.Threading.Tasks;

// The Split App template is documented at http://go.microsoft.com/fwlink/?LinkId=234228

namespace SynoDL8
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : MvvmAppBase
    {
        private IContainer container;
            
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialize(IActivatedEventArgs args)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new SynoDownloadModule());
            this.container = builder.Build();

            ViewModelLocator.SetDefaultViewTypeToViewModelTypeResolver(viewtype =>
            {
                if (viewtype == typeof(MainPage))
                    return typeof(IMainViewModel);
                if (viewtype == typeof(LoginPage))
                    return typeof(ILoginViewModel);
                return null;
            });

            ViewModelLocator.SetDefaultViewModelFactory(vmtype =>
            {
                return this.container.Resolve(vmtype);
            });
        }

        protected override Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Login", null);
            return Task.FromResult<object>(null);
        }
    }
}

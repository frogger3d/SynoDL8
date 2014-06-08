using Autofac;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SynoDL8.Common;
using SynoDL8.Model;
using SynoDL8.Services;
using SynoDL8.ViewModels;
using SynoDL8.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

namespace SynoDL8
{
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

            builder.RegisterInstance(this.NavigationService).As<INavigationService>();
            builder.RegisterInstance(this.SessionStateService).As<ISessionStateService>();

            // Services
            builder.RegisterType<SynologyService>().As<ISynologyService>().SingleInstance();
            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();

            // ViewModels
            builder.RegisterType<MainPageViewModel>().AsSelf();
            builder.RegisterType<DownloadTaskViewViewModel>().AsSelf();
            builder.RegisterType<LoginPageViewModel>().AsSelf();

            this.container = builder.Build();

            ViewModelLocator.SetDefaultViewModelFactory(vmtype =>
            {
                return this.container.Resolve(vmtype);
            });
        }

        /// <summary>
        /// Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs protocolArgs = (ProtocolActivatedEventArgs)args;
                    NavigationService.Navigate("Main", new DownloadRequest(protocolArgs.Uri));
                    break;
            }

        }

        /// <summary>
        /// Invoked when the application is launched. Override this method to perform application initialization and to display initial content in the associated Window.
        /// </summary>
        protected override Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Login", null);
            return Task.FromResult<object>(null);
        }
    }
}

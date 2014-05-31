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

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs protocolArgs = (ProtocolActivatedEventArgs)args;
                    var synologyService = this.container.Resolve<ISynologyService>();
                    var configurationService = this.container.Resolve<IConfigurationService>();
                    var credentials = configurationService.GetLastCredentials();
                    if (credentials != null)
                    {
                        await synologyService.LoginAsync(credentials);
                        if (synologyService.IsSignedIn)
                        {
                            await synologyService.CreateTaskAsync(protocolArgs.Uri.ToString());
                            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                            {
                                NavigationService.Navigate("Main", "Download started");
                            }

                            return;
                        }
                    }

                    if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                    {
                        NavigationService.Navigate("Login", null);
                    }

                    return;
            }

        }

        protected override async Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    var synologyService = this.container.Resolve<ISynologyService>();
                    var configurationService = this.container.Resolve<IConfigurationService>();
                    var credentials = configurationService.GetLastCredentials();
                    if (credentials != null)
                    {
                        await synologyService.LoginAsync(credentials);
                        if (synologyService.IsSignedIn)
                        {
                            await synologyService.CreateTaskAsync(args.Arguments);
                            NavigationService.Navigate("Main", null);
                            return;
                        }
                    }

                    NavigationService.Navigate("Login", null);
                    return;

                case ActivationKind.Launch:
                    NavigationService.Navigate("Login", null);
                    return;

                default:
                    throw new ArgumentException("Unexpected activation kind");
            }
        }
    }
}

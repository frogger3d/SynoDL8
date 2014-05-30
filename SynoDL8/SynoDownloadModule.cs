using Autofac;
using SynoDL8.Model;
using SynoDL8.Services;
using SynoDL8.ViewModels;
using SynoDL8.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8
{
    class SynoDownloadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            bool isInDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;

            // Services
            builder.RegisterType<SynologyService>().As<ISynologyService>().SingleInstance();
            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();

            // ViewModels
            builder.RegisterType<MainViewModel>().As<IMainViewModel>();
            builder.RegisterType<DownloadTaskViewModel>().As<DownloadTaskViewModel>();
            builder.RegisterType<LoginViewModel>().As<ILoginViewModel>();
        }
    }
}

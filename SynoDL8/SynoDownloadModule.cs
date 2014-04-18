using Autofac;
using SynoDL8.DataModel;
using SynoDL8.ViewModel;
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
            if (isInDesignMode)
            {
                builder.RegisterType<DesignDataModel>().As<IDataModel>();
                builder.RegisterType<DesignMainViewModel>().As<IMainViewModel>();
            }
            else
            {
                builder.RegisterType<SynologyDataModel>().As<IDataModel>().SingleInstance();
                builder.RegisterType<MainViewModel>().As<IMainViewModel>();
            }
            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();
            builder.RegisterType<LoginViewModel>().As<ILoginViewModel>();
            builder.RegisterType<SettingsViewModel>().AsSelf();
        }
    }
}

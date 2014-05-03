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
                builder.RegisterType<DownloadTaskViewModel>().WithParameter("task", new DownloadTask() { Title = "Fake title", Size = 300, Status = DownloadTask.status.downloading });
            }
            else
            {
                builder.RegisterType<SynologyDataModel>().As<IDataModel>().SingleInstance();
                builder.RegisterType<MainViewModel>().As<IMainViewModel>();
                builder.RegisterType<DownloadTaskViewModel>().As<DownloadTaskViewModel>();
            }
            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().SingleInstance();
            builder.RegisterType<LoginViewModel>().As<ILoginViewModel>();
        }
    }
}

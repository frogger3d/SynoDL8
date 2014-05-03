/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:MvvmLightReference"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Autofac;
using SynoDL8.DataModel;

namespace SynoDL8.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private readonly IContainer Container;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new SynoDownloadModule());
            Container = builder.Build();
        }

        public IMainViewModel Main
        {
            get
            {
                return this.Container.Resolve<IMainViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return this.Container.Resolve<SettingsViewModel>();
            }
        }

        public ILoginViewModel Login
        {
            get
            {
                return this.Container.Resolve<ILoginViewModel>();
            }
        }

        public DownloadTaskViewModel Task
        {
            get
            {
                return this.Container.Resolve<DownloadTaskViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
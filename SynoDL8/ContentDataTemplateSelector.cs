
namespace SynoDL8
{
    using SynoDL8.Model;
    using SynoDL8.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class ContentDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TaskTemplate { get; set; }
        public DataTemplate ExceptionTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is string)
            {
                return this.StringTemplate;
            }
            else if (item is Exception)
            {
                return this.ExceptionTemplate;
            }
            else if (item is DownloadTaskViewViewModel)
            {
                return this.TaskTemplate;
            }

            return null;
        }
    }
}

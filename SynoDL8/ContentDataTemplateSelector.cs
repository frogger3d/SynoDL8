using SynoDL8.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynoDL8
{
    public class ContentDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TaskListTemplate { get; set; }
        public DataTemplate ExceptionTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return this.TaskListTemplate;

            if (item is string)
            {
                return this.StringTemplate;
            }
            else if (item is Exception)
            {
                return this.ExceptionTemplate;
            }
            else if (item is List<DownloadTask>)
            {
                return this.TaskListTemplate;
            }

            return null;
        }
    }
}

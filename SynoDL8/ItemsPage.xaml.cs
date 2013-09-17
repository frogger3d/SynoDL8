using SynoDL8.Data;
using SynoDL8.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace SynoDL8
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : SynoDL8.Common.LayoutAwarePage
    {
        const string host = @"http://diskstation:5000/";

        private readonly DSQuerier DSQuerier;

        public ItemsPage()
        {
            this.InitializeComponent();

            var settings = new SettingsFlyoutViewModel();
            this.DSQuerier = new DSQuerier(settings.Hostname, settings.User, settings.Password);

        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);
            this.DefaultViewModel["Items"] = sampleDataGroups;
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(SplitPage), groupId);
        }

        private void Auth(object sender, RoutedEventArgs e)
        {
            this.DSResults.Text = "";
            this.message.Text = "Authenticating";

            Observable.StartAsync(this.DSQuerier.Authenticate)
                      .ObserveOnDispatcher()
                      .Subscribe(
                      ev => 
                          {
                              this.message.Text = ev ? "Authenticated" : "Authentication failed";
                          },
                      ex =>
                          {
                              this.DSResults.Text = ex.ToString();
                              this.message.Text = "Connection failed";
                          },
                      () => this.message.Text = "Authenticated");
        }

        private async void Logout(object sender, RoutedEventArgs e)
        {
            this.DSResults.Text = await this.DSQuerier.Logout();
        }

        private async void Versions(object sender, RoutedEventArgs e)
        {
            this.DSResults.Text = await this.DSQuerier.GetVersions();
        }

        private async void Info(object sender, RoutedEventArgs e)
        {
            this.DSResults.Text = await this.DSQuerier.GetInfo();
        }

        private async void List(object sender, RoutedEventArgs e)
        {
            this.DSResults.Text = await this.DSQuerier.List();
        }
    }
}

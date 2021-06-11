using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Axemasta.SuperWebView.Sample.Pages
{
    public partial class BrowserPage : ContentPage
    {
        public BrowserPage()
        {
            InitializeComponent();

            //superWebView.Source = "https://www.google.co.uk";

            superWebView.Navigating += OnNavigating;
            superWebView.Navigated += OnNavigated;
            superWebView.NavigationCancelled += OnNavigationCancelled;

            superWebView.ProgressChanged += OnProgress;

            //normalWebView.Source = "https://www.google.co.uk";

            //normalWebView.Navigating += OnNavigating;
            //normalWebView.Navigated += OnNavigated;
        }

        private void OnNavigationCancelled(object sender, NavigationCancelledEventArgs e)
        {
            Debug.WriteLine($"OnNavigationCancelled - Navigation to site cancelled: {e.Url}");
        }

        private void OnProgress(object sender, ProgressEventArgs e)
        {
            Debug.WriteLine($"OnProgress: {e.Percentage}%");
            Debug.WriteLine($"OnProgress - Raw: {e.Progress}");
        }

        private void OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            Debug.WriteLine($"OnNavigating Fired - {e.Url}");
        }

        private void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            Debug.WriteLine($"OnNavigated Fired - {e.Url}");
        }

        private async void OnNavigating(object sender, SuperWebNavigatingEventArgs e)
        {
            Debug.WriteLine($"OnNavigating Fired - {e.Url}");

            var token = e.GetDeferral();

            bool canBrowse = await CanBrowse(e.Url);

            if (!canBrowse)
                e.Cancel();

            token.Complete();
        }

        private void OnNavigated(object sender, SuperWebNavigatedEventArgs e)
        {
            Debug.WriteLine($"OnNavigated Fired - {e.Url}");
        }

        private async Task<bool> CanBrowse(string url)
        {
            await Task.Delay(1000);

            return !url.Contains("bbc.co.uk");
        }
    }
}

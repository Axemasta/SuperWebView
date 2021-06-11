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

            superWebView.Navigating += OnNavigating;
            superWebView.Navigated += OnNavigated;
            superWebView.NavigationCancelled += OnNavigationCancelled;
            superWebView.ProgressChanged += OnProgress;
            superWebView.BrowserInvocation += OnBrowserInvocation;

            var assemblyName = this.GetType().Assembly.FullName;

            var scripts = new List<JavaScript>()
            {
                new EmbeddedJavaScript("JQuery", "Axemasta.SuperWebView.Sample.Scripts.jquery-3.5.1.min.js", assemblyName),
                new EmbeddedJavaScript("Spy", "Axemasta.SuperWebView.Sample.Scripts.spy.js", assemblyName)
            };

            superWebView.InjectJavascript(scripts);
        }

        private void OnBrowserInvocation(object sender, BrowserInvocationEventArgs e)
        {
            Debug.WriteLine($"OnBrowserInvocation - Invoked with data: {e.Message}");
        }

        private void OnNavigationCancelled(object sender, NavigationCancelledEventArgs e)
        {
            Debug.WriteLine($"OnNavigationCancelled - Navigation to site cancelled: {e.Url}");
        }

        private void OnProgress(object sender, ProgressEventArgs e)
        {
            Debug.WriteLine($"OnProgress: {e.PercentageComplete}%");
            Debug.WriteLine($"OnProgress - Raw: {e.RawProgress}");
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Axemasta.SuperWebView.Sample.Pages
{
    public partial class BrowserPage : ContentPage
    {
        public BrowserPage()
        {
            InitializeComponent();

            On<Xamarin.Forms.PlatformConfiguration.iOS>()
                .SetUseSafeArea(true);

            backButton.Clicked += OnBackRequested;
            forwardButton.Clicked += OnForwardRequested;
            reloadButton.Clicked += OnReloadRequested;

            superWebView.Navigating += OnNavigating;
            superWebView.Navigated += OnNavigated;
            superWebView.NavigationCancelled += OnNavigationCancelled;
            superWebView.ProgressChanged += OnProgress;
            superWebView.BrowserInvocation += OnBrowserInvocation;
            superWebView.CanGoBackChanged += OnCanGoBackChanged;
            superWebView.CanGoForwardChanged += OnCanGoForwardChanged;

            var assemblyName = this.GetType().Assembly.FullName;

            var scripts = new List<JavaScript>()
            {
                new EmbeddedJavaScript("JQuery", "Axemasta.SuperWebView.Sample.Scripts.jquery-3.5.1.min.js", assemblyName),
                new EmbeddedJavaScript("Spy", "Axemasta.SuperWebView.Sample.Scripts.spy.js", assemblyName)
            };

            superWebView.InjectJavascript(scripts);
        }

        private void OnCanGoForwardChanged(object sender, EventArgs e)
        {
            backButton.IsEnabled = superWebView.CanGoBack;
        }

        private void OnCanGoBackChanged(object sender, EventArgs e)
        {
            forwardButton.IsEnabled = superWebView.CanGoForward;
        }

        private void OnReloadRequested(object sender, EventArgs e)
        {
            superWebView.Reload();
        }

        private void OnForwardRequested(object sender, EventArgs e)
        {
            superWebView.GoForward();
        }

        private void OnBackRequested(object sender, EventArgs e)
        {
            superWebView.GoBack();
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
            Debug.WriteLine($"OnProgress - Raw: {e.NormalisedProgress}");

            var progress = e.PercentageComplete / 100; // XF ProgressBar accepts 0-1

            progressBar.ProgressTo(progress, 250, Easing.SinIn);
        }

        private async void OnNavigating(object sender, SuperWebNavigatingEventArgs e)
        {
            Debug.WriteLine($"OnNavigating Fired - {e.Url}");

            if (e.CanCancel)
            {
                var token = e.GetDeferral();

                bool canBrowse = await CanBrowse(e.Url);

                if (!canBrowse)
                    e.Cancel();

                token.Complete();
            }
        }

        private void OnNavigated(object sender, SuperWebNavigatedEventArgs e)
        {
            Debug.WriteLine($"OnNavigated Fired - {e.Url}");

            addressLabel.Text = e.Url;
        }

        private async Task<bool> CanBrowse(string url)
        {
            await Task.Delay(1000);

            return !url.Contains("bbc.co.uk");
        }
    }
}

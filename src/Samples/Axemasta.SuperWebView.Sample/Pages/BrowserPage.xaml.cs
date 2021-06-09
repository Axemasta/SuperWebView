using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace Axemasta.SuperWebView.Sample.Pages
{
    public partial class BrowserPage : ContentPage
    {
        public BrowserPage()
        {
            InitializeComponent();

            superWebView.Source = "https://www.google.co.uk";

            superWebView.Navigating += OnNavigating;
            superWebView.Navigated += OnNavigated;
        }

        private void OnNavigating(object sender, SuperWebNavigatingEventArgs e)
        {
            Debug.WriteLine("OnNavigating Fired");
        }

        private void OnNavigated(object sender, SuperWebNavigatedEventArgs e)
        {
            Debug.WriteLine("OnNavigated Fired");
        }
    }
}

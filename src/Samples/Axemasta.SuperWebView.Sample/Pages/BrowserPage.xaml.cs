using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Axemasta.SuperWebView.Sample.Pages
{
    public partial class BrowserPage : ContentPage
    {
        public BrowserPage()
        {
            InitializeComponent();

            superWebView.Source = "https://www.google.co.uk";
        }
    }
}

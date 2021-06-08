using System;
using Axemasta.SuperWebView.Sample.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Axemasta.SuperWebView.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new BrowserPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

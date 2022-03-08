using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AP.MobileToolkit.Fonts;
using Axemasta.SuperWebView.Sample.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Axemasta.SuperWebView.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Log.Listeners.Add(new DelegateLogListener(OnLogListener));
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            FontRegistry.RegisterFonts(FontAwesomeBrands.Font,
                                       FontAwesomeRegular.Font,
                                       FontAwesomeSolid.Font);

            MainPage = new BrowserPage();
        }

        private void OnLogListener(string arg1, string arg2)
        {
            Console.WriteLine($"App - OnLogListener - {arg1}: {arg2}");
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (var exception in e.Exception.InnerExceptions)
            {
                Console.WriteLine($"App - OnUnobservedTaskException - An exception occured: {exception.ToString()}");
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null)
            {
                Console.WriteLine("App - OnUnhandledException - Could not cast UnhandledExceptionEventArgs ExceptionObject to exception...");
                return;
            }

            Console.WriteLine($"App - OnUnhandledException - An exception occured: {exception}");
        }
    }
}

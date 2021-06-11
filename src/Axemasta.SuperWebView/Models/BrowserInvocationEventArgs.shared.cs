using System;
namespace Axemasta.SuperWebView
{
    public class BrowserInvocationEventArgs : EventArgs
    {
        public string Message { get; }

        public BrowserInvocationEventArgs(string message)
        {
            this.Message = message;
        }
    }
}

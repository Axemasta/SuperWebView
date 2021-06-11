using System;
namespace Axemasta.SuperWebView
{
    public class ProgressEventArgs : EventArgs
    {
        public double RawProgress { get; }

        public double PercentageComplete { get; }

        public ProgressEventArgs(double progress, int maximum)
        {
            RawProgress = progress;

            var rawPercentge =  progress / maximum * 100;

            PercentageComplete = Math.Round(rawPercentge, 2);
        }
    }
}

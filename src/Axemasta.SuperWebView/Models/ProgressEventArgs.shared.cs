using System;
namespace Axemasta.SuperWebView
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; }

        public double Percentage { get; }

        public ProgressEventArgs(double progress, int maximum)
        {
            Progress = progress;

            var rawPercentge =  progress / maximum * 100;

            Percentage = Math.Round(rawPercentge, 2);
        }
    }
}

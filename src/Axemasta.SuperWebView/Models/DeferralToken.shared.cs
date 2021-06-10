using System;
using System.Threading;

namespace Axemasta.SuperWebView
{
    public class DeferralToken
	{
		Action _completed;

		internal DeferralToken(Action completed)
		{
			_completed = completed;
		}

		public void Complete()
		{
			var taskToComplete = Interlocked.Exchange(ref _completed, null);

			if (taskToComplete != null)
				taskToComplete?.Invoke();
		}

		internal bool IsCompleted => _completed == null;
	}
}

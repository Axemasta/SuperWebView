using System;
using System.Threading;

namespace Axemasta.SuperWebView
{
    public class DeferralToken : IDeferralToken
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

	/// <summary>
    /// Blank Deferral Token For When Cancellation Is Not Enabled
    /// </summary>
    public class EmptyDeferralToken : IDeferralToken
    {
		/* If CanCancel == false & the user has not checked for this, GetDeferralToken() would otherwise
		 * return null causing the Complete(); method to throw a NullReferenceException. This class prevents
		 * this condition from occuring
		 */

        public void Complete()
        {
        }
    }
}

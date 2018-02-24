using System;
using System.Windows.Forms;

namespace KeePassHIBP
{
	public class DelayedEventHandler
	{
		private readonly Timer delayTimer = new Timer();

		private object delegatedSender;
		private EventArgs delegatedEventArgs;

		public EventHandler OnDelay;

		public DelayedEventHandler(TimeSpan delay, EventHandler eventDelegate)
		{
			delayTimer.Interval = (int)delay.TotalMilliseconds;
			delayTimer.Tick += delegate
			{
				delayTimer.Stop();

				if (eventDelegate != null)
				{
					eventDelegate.Invoke(delegatedSender, delegatedEventArgs);
				}
			};

			OnDelay = delegate (object sender, EventArgs e)
			{
				delegatedSender = sender;
				delegatedEventArgs = e;

				delayTimer.Stop();

				delayTimer.Start();
			};
		}
	}
}

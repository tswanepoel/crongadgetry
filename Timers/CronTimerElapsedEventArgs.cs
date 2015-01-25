namespace CronGadgetry.Timers
{
    using System;
    using System.Threading;

    public class CronTimerElapsedEventArgs : EventArgs
    {
        private readonly DateTimeOffset _time;

        public DateTimeOffset Time
        {
            get { return _time; }
        }

        public CronTimerElapsedEventArgs(DateTimeOffset time)
        {
            _time = time;
        }

        public void Wait()
        {
            TimeSpan wait = _time - DateTimeOffset.Now - TimeSpan.FromMilliseconds(1d);

            if (wait > TimeSpan.Zero)
            {
                Thread.Sleep(wait);
            }

            // Burn the remaining nanoseconds (in theory).
            while (_time > DateTimeOffset.Now)
            {
            }
        }
    }
}

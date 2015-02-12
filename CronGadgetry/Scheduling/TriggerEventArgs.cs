namespace CronGadgetry.Scheduling
{
    using System;

    public class TriggerEventArgs : EventArgs
    {
        private readonly ITrigger _trigger;

        public TriggerEventArgs(ITrigger trigger)
        {
            _trigger = trigger;
        }

        public ITrigger Trigger
        {
            get { return _trigger; }
        }
    }
}

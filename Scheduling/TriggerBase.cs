namespace CronGadgetry.Scheduling
{
    using System;

    public abstract class TriggerBase : ITrigger
    {
        private readonly TimeSpan _fireOffset;

        protected TriggerBase(TimeSpan fireOffset)
        {
            _fireOffset = fireOffset;
        }

        public TimeSpan FireOffset
        {
            get { return _fireOffset; }
        }

        public abstract DateTimeOffset? GetTimeAfter(DateTimeOffset value);
    }
}

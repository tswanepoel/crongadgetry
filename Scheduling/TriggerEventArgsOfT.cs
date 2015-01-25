namespace CronGadgetry.Scheduling
{
    public class TriggerEventArgs<T> : TriggerEventArgs
        where T : ITrigger
    {
        public TriggerEventArgs(T trigger)
            : base(trigger)
        {
        }

        public new T Trigger
        {
            get { return (T)base.Trigger; }
        }
    }
}

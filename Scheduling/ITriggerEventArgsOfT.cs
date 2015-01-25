namespace CronGadgetry.Scheduling
{
    public interface ITriggerEventArg<out T> : ITriggerEventArgs
        where T : ITrigger
    {
        new T Trigger { get; }
    }
}

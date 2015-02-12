namespace CronGadgetry.Scheduling
{
    public delegate void TriggerEventHandler<T>(object sender, TriggerEventArgs<T> e)
        where T : ITrigger;
}

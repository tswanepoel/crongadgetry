namespace CronGadgetry.Scheduling
{
    public delegate void JobEventHandler<T>(object sender, JobEventArgs<T> e)
        where T : IJob;
}

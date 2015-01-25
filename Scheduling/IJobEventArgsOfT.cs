namespace CronGadgetry.Scheduling
{
    public interface IJobEventArgs<out T> : IJobEventArgs
        where T : IJob
    {
        new T Job { get; }
    }
}

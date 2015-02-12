namespace CronGadgetry.Scheduling
{
    public interface IJobExecutionContext<out T> : IJobExecutionContext
        where T : IJob
    {
        new T Job { get; }
    }
}

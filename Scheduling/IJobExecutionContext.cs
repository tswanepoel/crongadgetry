namespace CronGadgetry.Scheduling
{
    using System;
    using System.Threading;

    public interface IJobExecutionContext
    {
        IScheduler Scheduler { get; }
        IJob Job { get; }
        ITrigger Trigger { get; }
        DateTimeOffset Time { get; }
        CancellationToken CancellationToken { get; }
    }
}

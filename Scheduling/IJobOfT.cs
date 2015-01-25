namespace CronGadgetry.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface IJob<T> : IJob
        where T : ITrigger
    {
        new event TriggerEventHandler<T> TriggerAdded;
        new event TriggerEventHandler<T> TriggerRemoving;

        new ICollection<T> Triggers { get; }
        void Execute(IScheduler<IJob<T>> scheduler, T trigger, DateTimeOffset time, CancellationToken cancellationToken);
    }
}

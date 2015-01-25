namespace CronGadgetry.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface IJob
    {
        event TriggerEventHandler TriggerAdded;
        event TriggerEventHandler TriggerRemoving;

        ICollection<ITrigger> Triggers { get; }
        void Execute(IScheduler scheduler, ITrigger trigger, DateTimeOffset time, CancellationToken cancellationToken);
    }
}

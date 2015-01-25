namespace CronGadgetry.Scheduling
{
    using System.Collections.Generic;

    public interface IScheduler<T> : IScheduler
        where T : IJob
    {
        new event JobEventHandler<T> JobAdded;
        new event JobEventHandler<T> JobRemoving;

        new ICollection<T> Jobs { get; }
    }
}

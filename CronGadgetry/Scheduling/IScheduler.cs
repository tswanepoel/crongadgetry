namespace CronGadgetry.Scheduling
{
    using System.Collections.Generic;

    public interface IScheduler
    {
        event JobEventHandler JobAdded;
        event JobEventHandler JobRemoving;

        ICollection<IJob> Jobs { get; }

        void Start();
        void Stop();
    }
}

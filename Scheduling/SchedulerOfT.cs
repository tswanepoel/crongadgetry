namespace CronGadgetry.Scheduling
{
    using System.Collections.Generic;

    public class Scheduler<T> : Scheduler, IScheduler<T>
        where T : class, IJob
    {
        public new event JobEventHandler<T> JobAdded;
        public new event JobEventHandler<T> JobRemoving;

        public Scheduler()
        {
        }

        public Scheduler(IEnumerable<T> jobs)
            : base(jobs)
        {
        }

        public new ICollection<T> Jobs
        {
            get { return (ICollection<T>)base.Jobs; }
        }

        protected override void OnJobAdded(IJob job)
        {
            base.OnJobAdded(job);

            if (JobAdded != null)
            {
                JobAdded(this, new JobEventArgs<T>((T)job));
            }
        }

        protected override void OnJobRemoving(IJob job)
        {
            base.OnJobRemoving(job);

            if (JobRemoving != null)
            {
                JobRemoving(this, new JobEventArgs<T>((T)job));
            }
        }
    }
}

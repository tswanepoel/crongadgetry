namespace CronGadgetry.Scheduling
{
    using System;
    using System.Threading;

    public class JobExecutionContext<TJob> : JobExecutionContext, IJobExecutionContext<TJob>
        where TJob : class, IJob
    {
        internal JobExecutionContext(IScheduler<TJob> scheduler, TJob job, ITrigger trigger, DateTimeOffset time, CancellationToken cancellationToken)
            : base(scheduler, job, trigger, time, cancellationToken)
        {
        }

        public new TJob Job
        {
            get { return (TJob)base.Job; }
        }
    }
}

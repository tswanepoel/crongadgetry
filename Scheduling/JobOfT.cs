namespace CronGadgetry.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class Job<T> : Job, IJob<T>
        where T : class, ITrigger
    {
        public new event TriggerEventHandler<T> TriggerAdded;
        public new event TriggerEventHandler<T> TriggerRemoving;

        public Job(Action<JobExecutionContext<Job<T>>> action)
            : base(context => action((JobExecutionContext<Job<T>>)context))
        {
        }

        public Job(Action<JobExecutionContext<Job<T>>> action, IEnumerable<T> triggers)
            : base(context => action((JobExecutionContext<Job<T>>)context), triggers)
        {
        }

        public Job(Action<JobExecutionContext<Job<T>>> action, params T[] triggers)
            : base(context => action((JobExecutionContext<Job<T>>)context), triggers)
        {
        }

        public new ICollection<T> Triggers
        {
            get { return (ICollection<T>)base.Triggers; }
        }

        public override void Execute(IScheduler scheduler, ITrigger trigger, DateTimeOffset time, CancellationToken cancellationToken)
        {
            Action(new JobExecutionContext<IJob<T>>((IScheduler<IJob<T>>)scheduler, this, trigger, time, cancellationToken));
        }

        public void Execute(IScheduler<IJob<T>> scheduler, T trigger, DateTimeOffset time, CancellationToken cancellationToken)
        {
            Action(new JobExecutionContext<IJob<T>>(scheduler, this, trigger, time, cancellationToken));
        }

        protected override void OnTriggerAdded(ITrigger trigger)
        {
            base.OnTriggerAdded(trigger);

            if (TriggerAdded != null)
            {
                TriggerAdded(this, new TriggerEventArgs<T>((T)trigger));
            }
        }

        protected override void OnTriggerRemoving(ITrigger trigger)
        {
            base.OnTriggerRemoving(trigger);

            if (TriggerRemoving != null)
            {
                TriggerRemoving(this, new TriggerEventArgs<T>((T)trigger));
            }
        }
    }
}

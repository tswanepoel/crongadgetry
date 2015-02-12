namespace CronGadgetry.Scheduling
{
    using System;
    using System.Threading;

    public class JobExecutionContext : IJobExecutionContext
    {
        private readonly IScheduler _scheduler;
        private readonly IJob _job;
        private readonly ITrigger _trigger;
        private readonly DateTimeOffset _time;
        private readonly CancellationToken _cancellationToken;

        internal JobExecutionContext(IScheduler scheduler, IJob job, ITrigger trigger, DateTimeOffset time, CancellationToken cancellationToken)
        {
            _scheduler = scheduler;
            _job = job;
            _trigger = trigger;
            _time = time;
            _cancellationToken = cancellationToken;
        }

        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        public IJob Job
        {
            get { return _job; }
        }

        public ITrigger Trigger
        {
            get { return _trigger; }
        }

        public DateTimeOffset Time
        {
            get { return _time; }
        }

        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
        }
    }
}

namespace CronGadgetry.Scheduling
{
    using System;

    public class JobEventArgs : EventArgs, IJobEventArgs
    {
        private readonly IJob _job;

        public JobEventArgs(IJob job)
        {
            _job = job;
        }

        public IJob Job
        {
            get { return _job; }
        }
    }
}

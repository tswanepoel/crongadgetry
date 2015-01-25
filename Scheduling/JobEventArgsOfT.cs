namespace CronGadgetry.Scheduling
{
    public class JobEventArgs<T> : JobEventArgs, IJobEventArgs<T>
        where T : IJob
    {
        public JobEventArgs(T job)
            : base(job)
        {
        }

        public new T Job
        {
            get { return (T)base.Job; }
        }
    }
}

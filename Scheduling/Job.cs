namespace CronGadgetry.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    public class Job : IJob
    {
        public event TriggerEventHandler TriggerAdded;
        public event TriggerEventHandler TriggerRemoving;

        private readonly Action<JobExecutionContext> _action;
        private readonly TriggerCollection _triggers;

        public Job(Action<JobExecutionContext> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;
            _triggers = new TriggerCollection(this);
        }

        public Job(Action<JobExecutionContext> action, params ITrigger[] triggers)
            : this(action, (IEnumerable<ITrigger>)triggers)
        {
        }

        public Job(Action<JobExecutionContext> action, IEnumerable<ITrigger> triggers)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;
            _triggers = new TriggerCollection(this, triggers);
        }

        public virtual void Execute(IScheduler scheduler, ITrigger trigger, DateTimeOffset time , CancellationToken cancellationToken)
        {
            _action(new JobExecutionContext(scheduler, this, trigger, time, cancellationToken));
        }

        protected Action<JobExecutionContext> Action
        {
            get { return _action; }   
        }

        public TriggerCollection Triggers
        {
            get { return _triggers; }
        }

        ICollection<ITrigger> IJob.Triggers
        {
            get { return _triggers; }
        }

        protected virtual void OnTriggerAdded(ITrigger trigger)
        {
            if (TriggerAdded != null)
            {
                TriggerAdded(this, new TriggerEventArgs(trigger));
            }
        }

        protected virtual void OnTriggerRemoving(ITrigger trigger)
        {
            if (TriggerRemoving != null)
            {
                TriggerRemoving(this, new TriggerEventArgs(trigger));
            }
        }

        public class TriggerCollection : ICollection<ITrigger>
        {
            private readonly Job _job;
            private readonly ICollection<ITrigger> _items;

            public TriggerCollection(Job job)
            {
                _job = job;
                _items = new List<ITrigger>();
            }

            public TriggerCollection(Job job, IEnumerable<ITrigger> items)
            {
                _job = job;
                _items = new List<ITrigger>(items);
            }

            public void Add(ITrigger item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException();
                }

                _items.Add(item);
                _job.OnTriggerAdded(item);
            }

            public void Clear()
            {
                foreach (var item in _items)
                {
                    _job.OnTriggerRemoving(item);
                }

                _items.Clear();
            }

            public bool Contains(ITrigger item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(ITrigger[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _items.Count; }
            }

            bool ICollection<ITrigger>.IsReadOnly
            {
                get { return _items.IsReadOnly; }
            }

            public bool Remove(ITrigger item)
            {
                _job.OnTriggerRemoving(item);
                return _items.Remove(item);
            }

            public IEnumerator<ITrigger> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }
    }
}

namespace CronGadgetry.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Scheduler : IScheduler, IDisposable
    {
        public event JobEventHandler JobAdded;
        public event JobEventHandler JobRemoving;

        private readonly object _syncRoot = new object();
        private readonly Dictionary<ITrigger, CancellationTokenSource> _cancellationTokenSources = new Dictionary<ITrigger, CancellationTokenSource>();
        private readonly JobCollection _jobs;
        private bool _running = true;
        private bool _disposed;

        public Scheduler()
        {
            _jobs = new JobCollection(this);
        }

        public Scheduler(IEnumerable<IJob> jobs)
        {
            _jobs = new JobCollection(this, jobs);
        }

        public JobCollection Jobs
        {
            get { return _jobs; }
        }

        ICollection<IJob> IScheduler.Jobs
        {
            get { return _jobs; }
        }

        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Scheduler");
            }

            lock (_syncRoot)
            {
                if (_running) { return; }

                foreach (var cancellationTokenSource in _cancellationTokenSources.Values)
                {
                    cancellationTokenSource.Dispose();
                }

                _cancellationTokenSources.Clear();

                foreach (var job in _jobs)
                {
                    foreach (var trigger in job.Triggers)
                    {
                        JobTriggerAdded(job, new TriggerEventArgs(trigger));
                    }
                }

                _running = true;
            }
        }

        public void Stop()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Scheduler");
            }

            lock (_syncRoot)
            {
                if (!_running) { return; }

                foreach (var cancellationTokenSource in _cancellationTokenSources.Values)
                {
                    cancellationTokenSource.Cancel();
                }

                _running = false;
            }
        }

        protected virtual void OnJobAdded(IJob job)
        {
            job.TriggerAdded += JobTriggerAdded;
            job.TriggerRemoving += JobTriggerRemoving;

            if (JobAdded != null)
            {
                JobAdded(this, new JobEventArgs(job));
            }

            lock (_syncRoot)
            {
                if (!_running) { return; }

                foreach (var trigger in job.Triggers)
                {
                    JobTriggerAdded(job, new TriggerEventArgs(trigger));
                }
            }
        }

        protected virtual void OnJobRemoving(IJob job)
        {
            if (JobRemoving != null)
            {
                JobRemoving(this, new JobEventArgs(job));
            }

            job.TriggerAdded -= JobTriggerAdded;
            job.TriggerRemoving -= JobTriggerRemoving;

            lock (_syncRoot)
            {
                if (!_running) { return; }

                foreach (var trigger in job.Triggers)
                {
                    JobTriggerRemoving(job, new TriggerEventArgs(trigger));
                }
            }
        }

        private void JobTriggerAdded(object sender, TriggerEventArgs e)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSources.Add(e.Trigger, cancellationTokenSource);

            Task.Factory.StartNew(() => Run((IJob)sender, e.Trigger, cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        private void JobTriggerRemoving(object sender, TriggerEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Run(IJob job, ITrigger trigger, CancellationToken cancellationToken)
        {
            DateTimeOffset? next = DateTimeOffset.Now;

            while ((next = trigger.GetTimeAfter((DateTimeOffset)next)) != null)
            {
                TimeSpan wait = (DateTimeOffset)next - DateTimeOffset.Now + trigger.FireOffset;

                if (wait > TimeSpan.Zero)
                {
                    Thread.Sleep(wait);
                }

                // Burn the remaining nanoseconds (in theory).
                while (next > DateTimeOffset.Now - trigger.FireOffset)
                {
                }

                if (cancellationToken.IsCancellationRequested) { return; }

                var time = (DateTimeOffset)next;
                Task.Factory.StartNew(() => OnTriggerFired(job, trigger, time, cancellationToken), cancellationToken);
            }
        }

        protected virtual void OnTriggerFired(IJob job, ITrigger trigger, DateTimeOffset time, CancellationToken cancellationToken)
        {
            job.Execute(this, trigger, time, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_running)
                {
                    Stop();

                    _disposed = true;
                    Thread.MemoryBarrier();

                    foreach (var cancellationTokenSource in _cancellationTokenSources.Values)
                    {
                        cancellationTokenSource.Dispose();
                    }
                }
            }
        }

        public class JobCollection : ICollection<IJob>
        {
            private readonly Scheduler _scheduler;
            private readonly ICollection<IJob> _items;

            public JobCollection(Scheduler scheduler)
            {
                _scheduler = scheduler;
                _items = new List<IJob>();
            }

            public JobCollection(Scheduler scheduler, IEnumerable<IJob> jobs)
            {
                _scheduler = scheduler;
                _items = new List<IJob>(jobs);
            }

            public void Add(IJob item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException();
                }

                _scheduler.OnJobAdded(item);
                _items.Add(item);
            }

            public void Clear()
            {
                foreach (var item in _items)
                {
                    _scheduler.OnJobRemoving(item);
                }

                _items.Clear();
            }

            public bool Contains(IJob item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(IJob[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _items.Count; }
            }

            bool ICollection<IJob>.IsReadOnly
            {
                get { return _items.IsReadOnly; }
            }

            public bool Remove(IJob item)
            {
                _scheduler.OnJobRemoving(item);
                return _items.Remove(item);
            }

            public IEnumerator<IJob> GetEnumerator()
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

namespace CronGadgetry.Timers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CronTimer : IDisposable
    {
        private readonly CronExpression _expression;
        private readonly TimeSpan _fireOffset;

        private readonly object _syncRoot = new object();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _running;
        private bool _disposed;

        public event EventHandler ExpressionChanged;
        public event EventHandler<CronTimerElapsedEventArgs> Elapsed;

        public CronTimer(CronExpression expression)
            : this(expression, TimeSpan.Zero)
        {   
        }

        public CronTimer(CronExpression expression, TimeSpan fireOffset)
        {
            _expression = expression;
            _fireOffset = fireOffset;
        }

        public CronExpression Expression
        {
            get { return _expression; }
        }

        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("CronTimer");
            }

            if (_expression == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (_running) { return; }

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                }

                _cancellationTokenSource = new CancellationTokenSource();

                Task.Factory.StartNew(() => Run(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
                _running = true;
            }
        }

        public void Stop()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("CronTimer");
            }
            
            lock (_syncRoot)
            {
                if (!_running) { return; }

                _cancellationTokenSource.Cancel();
                _running = false;
            }
        }

        private void Run(CancellationToken cancellationToken)
        {
            foreach (var next in _expression.GetAllTimesAfter(DateTimeOffset.Now))
            {
                TimeSpan wait = next - DateTimeOffset.Now + _fireOffset;

                if (wait > TimeSpan.Zero)
                {
                    Thread.Sleep(wait);
                }

                // Burn the remaining nanoseconds (in theory).
                while (next > DateTimeOffset.Now - _fireOffset)
                {
                }

                if (cancellationToken.IsCancellationRequested) { return; }

                DateTimeOffset time = next;
                Task.Run(() => OnElapsed(time), cancellationToken);
            }
        }

        protected virtual void OnExpressionChanged(EventArgs e)
        {
            if (ExpressionChanged != null)
            {
                ExpressionChanged(this, e);
            }
        }

        protected virtual void OnElapsed(DateTimeOffset time)
        {
            if (Elapsed != null)
            {
                Elapsed(this, new CronTimerElapsedEventArgs(time));
            }
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

                    _cancellationTokenSource.Dispose();
                }
            }
        }
    }
}

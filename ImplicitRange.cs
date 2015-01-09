namespace CronGadgetry
{
    using System;
    using System.Collections.Generic;

    public struct ImplicitRange : IRange
    {
        private readonly int _resolution;
        private readonly int _start;
        private readonly int _end;

        public ImplicitRange(int resolution, int start, int end)
        {
            _resolution = resolution;
            _start = Math.Min(start, end);
            _end = Math.Max(start, end);
        }

        public IEnumerable<int> Values
        {
            get
            {
                for (int value = _start; value <= _end; value += _resolution)
                {
                    yield return value;
                }
            }
        }

        public bool Contains(int value)
        {
            return _start <= value && value <= _end && value % _resolution == 0;
        }

        public IRange GetViewFrom(int value)
        {
            return new ImplicitRange(_resolution, Math.Max(_start, value), _end);
        }

        public IRange GetViewBetween(int min, int max)
        {
            return new ImplicitRange(_resolution, Math.Max(_start, min), Math.Min(_end, max));
        }
    }
}

namespace CronGadgetry.Collections
{
    using System;
    using System.Collections.Generic;

    public class ExplicitRange : IRange
    {
        private readonly SortedSet<int> _values;

        public ExplicitRange(SortedSet<int> values)
        {
            _values = values;
        }

        public ExplicitRange(IEnumerable<int> values)
        {
            _values = new SortedSet<int>(values);
        }

        public ExplicitRange(IEnumerable<int> values, IComparer<int> comparer)
        {
            _values = new SortedSet<int>(values, comparer);
        }

        public IEnumerable<int> Values
        {
            get { return _values; }
        }

        public IRange GetViewFrom(int value)
        {
            if (_values.Count != 0)
            {
                return new ExplicitRange(_values.GetViewBetween(value, Math.Max(value, _values.Max)));
            }
            
            return new ExplicitRange(new SortedSet<int>());
        }

        public IRange GetViewBetween(int min, int max)
        {
            return new ExplicitRange(_values.GetViewBetween(min, max));
        }

        public bool Contains(int value)
        {
            return _values.Contains(value);
        }
    }
}

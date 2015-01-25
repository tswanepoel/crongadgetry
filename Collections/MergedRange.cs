namespace CronGadgetry.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class MergedRange : IRange
    {
        public class Enumerator : IEnumerator<int>
        {
            private bool _started = false;
            private bool _continueLeft = true;
            private bool _continueRight = true;
            private readonly IEnumerator<int> _left;
            private readonly IEnumerator<int> _right;

            public Enumerator(IEnumerator<int> left, IEnumerator<int> right)
            {
                _left = left;
                _right = right;
            }

            public int Current
            {
                get
                {
                    if (_continueLeft || _continueRight)
                    {
                        if (_continueLeft && _continueRight)
                        {
                            return _left.Current < _right.Current ? _left.Current : _right.Current;    
                        }

                        if (_continueLeft)
                        {
                            return _left.Current;
                        }

                        return _right.Current;
                    }

                    return default(int);
                }
            }

            public bool MoveNext()
            {
                if (!_started)
                {
                    _continueLeft = _left.MoveNext();
                    _continueRight = _right.MoveNext();

                    while (_continueLeft && _continueRight && _left.Current == _right.Current)
                    {
                        _continueLeft = _left.MoveNext();
                    }

                    _started = true;
                    return _continueLeft || _continueRight;
                }

                if (_continueLeft || _continueRight)
                {
                    if (_continueLeft && _continueRight)
                    {
                        if (_left.Current < _right.Current)
                        {
                            do
                            {
                                _continueLeft = _left.MoveNext();
                            }
                            while (_continueLeft && _left.Current == _right.Current);

                            return true;
                        }

                        do
                        {
                            _continueRight = _right.MoveNext();
                        }
                        while (_continueRight && _right.Current == _left.Current);
                        
                        return true;
                    }

                    if (_continueLeft)
                    {
                        return _continueLeft = _left.MoveNext();
                    }

                    return _continueRight = _right.MoveNext();
                }

                return false;
            }

            public void Reset()
            {
                _left.Reset();
                _right.Reset();

                _continueLeft =
                    _continueRight = true;
            }

            public void Dispose()
            {
                _left.Dispose();
                _right.Dispose();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        private readonly IRange _left;
        private readonly IRange _right;

        public MergedRange(IRange left, IRange right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            _left = left;
            _right = right;
        }

        public IEnumerable<int> Values
        {
            get
            {
                var enumerator = new Enumerator(_left.Values.GetEnumerator(), _right.Values.GetEnumerator());

                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        public IRange GetViewFrom(int value)
        {
            return new MergedRange(_left.GetViewFrom(value), _right.GetViewFrom(value));
        }

        public IRange GetViewBetween(int min, int max)
        {
            return new MergedRange(_left.GetViewBetween(min, max), _right.GetViewBetween(min, max));
        }

        public bool Contains(int value)
        {
            return _left.Contains(value) || _right.Contains(value);
        }
    }
}

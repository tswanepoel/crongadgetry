namespace CronGadgetry
{
    using System;
    using System.Collections.Generic;

    public class DayOfWeekRange : IDayRange
    {
        public struct Occurence
        {
            private readonly int _occurrence;

            public Occurence(DayOfWeek dayOfWeek, int instance)
            {
                if (instance > 5)
                {
                    throw new ArgumentOutOfRangeException("instance");
                }

                _occurrence = (instance << 3) + (int)dayOfWeek;
            }

            public override int GetHashCode()
            {
                return _occurrence;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Occurence))
                {
                    return false;
                }

                return Equals((Occurence)obj);
            }

            public bool Equals(Occurence occurence)
            {
                return occurence._occurrence.Equals(_occurrence);
            }

            public DayOfWeek DayOfWeek
            {
                get { return (DayOfWeek)(_occurrence & 0x7); }
            }

            public int Instance
            {
                get { return _occurrence >> 3; }
            }
        }

        private readonly SortedSet<DayOfWeek> _daysOfWeek;
        private readonly SortedSet<Occurence> _occurrences;
        private readonly SortedSet<Occurence> _occurrencesFromLast;

        public DayOfWeekRange(DayOfWeek start, DayOfWeek end, SortedSet<Occurence> occurrences, SortedSet<Occurence> occurrencesFromLast)
        {
            _daysOfWeek = new SortedSet<DayOfWeek>();

            for (int i = Math.Min((int)start, (int)end); i <= Math.Max((int)start, (int)end); i++)
            {
                _daysOfWeek.Add((DayOfWeek)i);
            }

            _occurrences = occurrences;
            _occurrencesFromLast = occurrencesFromLast;
        }

        public DayOfWeekRange(IEnumerable<DayOfWeek> values, SortedSet<Occurence> occurrences, SortedSet<Occurence> occurrencesFromLast)
        {
            if (values != null)
            {
                _daysOfWeek = new SortedSet<DayOfWeek>(values);
            }

            _occurrences = occurrences;
            _occurrencesFromLast = occurrencesFromLast;
        }

        public IEnumerable<int> GetValues(int year, int month)
        {
            return CreateRange(year, month).Values;
        }

        public bool Contains(int year, int month, int day)
        {
            IRange range = CreateRange(year, month);
            return range.Contains(day);
        }

        public IRange GetViewFrom(int year, int month, int day)
        {
            return CreateRange(year, month).GetViewFrom(day);
        }

        public IRange GetViewBetween(int year, int month, int min, int max)
        {
            return CreateRange(year, month).GetViewBetween(min, max);
        }

        private IRange CreateRange(int year, int month)
        {
            var daysOfMonth = new SortedSet<int>();
            
            DayOfWeek firstDayOfMonth = new DateTime(year, month, 1).DayOfWeek;
            int lastDayOfMonth = DateTime.DaysInMonth(year, month);

            if (_daysOfWeek != null)
            {
                foreach (DayOfWeek dayOfWeek in _daysOfWeek)
                {
                    int instance = 1;
                    int dayOfMonth;

                    while ((dayOfMonth = GetDayOfMonth(dayOfWeek, instance, firstDayOfMonth)) <= lastDayOfMonth)
                    {
                        daysOfMonth.Add(dayOfMonth);
                        instance++;
                    }
                }
            }

            if (_occurrences != null)
            {
                foreach (Occurence occurrence in _occurrences)
                {
                    daysOfMonth.Add(GetDayOfMonth(occurrence.DayOfWeek, occurrence.Instance, firstDayOfMonth));
                }
            }

            if (_occurrencesFromLast != null)
            {
                DayOfWeek lastDayOfMonthDow = new DateTime(year, month, lastDayOfMonth).DayOfWeek;

                foreach (Occurence occurrence in _occurrencesFromLast)
                {
                    daysOfMonth.Add(GetDayOfMonthFromLast(occurrence.DayOfWeek, occurrence.Instance, lastDayOfMonth, lastDayOfMonthDow));
                }
            }

            return new ExplicitRange(daysOfMonth);
        }

        private static int GetDayOfMonth(DayOfWeek dayOfWeek, int instance, DayOfWeek firstDayOfMonth)
        {
            int diff = (int)dayOfWeek - (int)firstDayOfMonth;

            if (diff < 0)
            {
                return 7 * instance + diff + 1;
            }

            return 7 * (instance - 1) + diff + 1;
        }

        private static int GetDayOfMonthFromLast(DayOfWeek dayOfWeek, int instance, int lastDayOfMonth, DayOfWeek lastDayOfMonthDow)
        {
            int diff = (int)lastDayOfMonthDow - (int)dayOfWeek;

            if (diff < 0)
            {
                return lastDayOfMonth - 7 * instance - diff;
            }

            return lastDayOfMonth - 7 * (instance - 1) - diff;
        }
    }
}

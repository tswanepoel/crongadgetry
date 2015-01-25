namespace CronGadgetry.Collections
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DayOfMonthRange : IDayRange
    {
        private readonly IRange _dayRange;
        private readonly SortedSet<int> _daysFromLast;
        private readonly SortedSet<int> _nearestWeekdays;

        public DayOfMonthRange(int start, int end, IEnumerable<int> daysFromLast, IEnumerable<int> nearestWeekdays)
        {
            _dayRange = new ImplicitRange(1, start, end);
            
            if (daysFromLast != null)
            {
                _daysFromLast = new SortedSet<int>(daysFromLast);
            }

            if (nearestWeekdays != null)
            {
                _nearestWeekdays = new SortedSet<int>(nearestWeekdays);   
            }
        }

        public DayOfMonthRange(IEnumerable<int> values, IEnumerable<int> daysFromLast, IEnumerable<int> nearestWeekdays)
        {
            if (values != null)
            {
                _dayRange = new ExplicitRange(values);
            }

            if (daysFromLast != null)
            {
                _daysFromLast = new SortedSet<int>(daysFromLast);    
            }

            if (nearestWeekdays != null)
            {
                _nearestWeekdays = new SortedSet<int>(nearestWeekdays);   
            }
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
            IRange range = null;
            int lastDayOfMonth = DateTime.DaysInMonth(year, month);

            if (_dayRange != null)
            {
                range = _dayRange.GetViewBetween(1, lastDayOfMonth);
            }

            if (_daysFromLast != null && _daysFromLast.Count != 0)
            {
                IRange dayFromLastRange = new ExplicitRange(_daysFromLast.Select(d => lastDayOfMonth - d));
                range = range != null ? range.Merge(dayFromLastRange) : dayFromLastRange;
            }

            if (_nearestWeekdays != null && _nearestWeekdays.Count != 0)
            {
                IRange nearestWeekdayRange = new ExplicitRange(_nearestWeekdays.Select(d => GetNearestWeekday(year, month, Math.Min(d, lastDayOfMonth), lastDayOfMonth)));
                range = range != null ? range.Merge(nearestWeekdayRange) : nearestWeekdayRange;
            }

            return range;
        }
        
        private static int GetNearestWeekday(int year, int month, int day, int lastDayOfMonth)
        {
            var dow = new DateTime(year, month, day).DayOfWeek;

            switch (dow)
            {
                case DayOfWeek.Saturday:
                    if (day != 1)
                    {
                        return day - 1;
                    }

                    return 3;

                case DayOfWeek.Sunday:
                    if (day != lastDayOfMonth)
                    {
                        return day + 1;
                    }

                    return day - 2;
            }

            return day;
        }
    }
}

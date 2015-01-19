namespace CronGadgetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CronExpression
    {
        private const char ExpressionSeperator = ' ';
        private const string AllToken = "*";
        private const string AnyToken = "?";
        private const string IncrementsSeperator = "/";
        private const string RangeChar = "-";
        private const string LastChar = "L";
        private const string WeekdayChar = "W";
        private const string LastWeekdayToken = "LW";
        private const string OccurrenceSeperator = "#";

        private readonly IRange _nanoseconds;
        private readonly IRange _microseconds;
        private readonly IRange _milliseconds;
        private readonly IRange _seconds;
        private readonly IRange _minutes;
        private readonly IRange _hours;
        private readonly IDayRange _days;
        private readonly IRange _months;
        private readonly IRange _years;
        private readonly TimeSpan _offset;

        public CronExpression(IRange nanosecons, IRange microseconds, IRange milliseconds, IRange seconds, IRange minutes, IRange hours, IDayRange daysOfMonth, IRange months, IDayRange daysOfWeek)
            : this(nanosecons, microseconds, milliseconds, seconds, minutes, hours, daysOfMonth, months, daysOfWeek, null, TimeSpan.Zero)
        {
        }

        public CronExpression(IRange nanoseconds, IRange microseconds, IRange milliseconds, IRange seconds, IRange minutes, IRange hours, IDayRange daysOfMonth, IRange months, IDayRange daysOfWeek, TimeSpan offset)
            : this(nanoseconds, microseconds, milliseconds, seconds, minutes, hours, daysOfMonth, months, daysOfWeek, null, offset)
        {
        }

        public CronExpression(IRange nanoseconds, IRange microseconds, IRange milliseconds, IRange seconds, IRange minutes, IRange hours, IDayRange daysOfMonth, IRange months, IDayRange daysOfWeek, IRange years)
            : this(nanoseconds, microseconds, milliseconds, seconds, minutes, hours, daysOfMonth, months, daysOfWeek, years, TimeSpan.Zero)
        {
        }

        public CronExpression(IRange nanoseconds, IRange microseconds, IRange milliseconds, IRange seconds, IRange minutes, IRange hours, IDayRange daysOfMonth, IRange months, IDayRange daysOfWeek, IRange years, TimeSpan offset)
        {
            if (daysOfMonth != null && daysOfWeek != null)
            {
                throw new NotSupportedException();
            }

            _nanoseconds = nanoseconds;
            _microseconds = microseconds;
            _milliseconds = milliseconds;
            _seconds = seconds;
            _minutes = minutes;
            _hours = hours;
            _days = daysOfMonth ?? daysOfWeek;
            _months = months;
            _years = years;
            _offset = offset;
        }
        
        public bool HasTime(DateTimeOffset value)
        {
            DateTimeOffset temp = value.ToOffset(_offset);

            return
                (_years == null || _years.Contains(temp.Year)) &&
                _months.Contains(temp.Month) &&
                _days.Contains(temp.Day, temp.Month, temp.Year) &&
                _hours.Contains(temp.Hour) &&
                _minutes.Contains(temp.Minute) &&
                _seconds.Contains(temp.Second) &&
                _milliseconds.Contains(temp.Millisecond) &&
                _microseconds.Contains(temp.GetMicroseconds()) &&
                _nanoseconds.Contains(temp.GetNanoseconds());
        }

        public DateTimeOffset? GetTimeAfter(DateTimeOffset value)
        {
            IEnumerable<DateTimeOffset> times = GetAllTimesAfter(value);

            foreach (var time in times)
            {
                return time;
            }

            return null;
        }

        public IEnumerable<DateTimeOffset> GetAllTimesAfter(DateTimeOffset value)
        {
            DateTimeOffset temp = value.ToOffset(_offset);

            bool firstYear = true;
            bool firstMonth = true;
            bool firstDay = true;
            bool firstHour = true;
            bool firstMinute = true;
            bool firstSecond = true;
            bool firstMillisecond = true;
            bool firstMicrosecond = true;

            var years = _years != null ? _years.GetViewFrom(temp.Year).Values : Enumerable.Range(temp.Year, DateTimeOffset.MaxValue.Year - temp.Year + 1);

            foreach (var year in years)
            {
                firstYear &= year == value.Year;

                foreach (var month in firstYear ? _months.GetViewFrom(temp.Month).Values : _months.Values)
                {
                    firstMonth &= firstYear && month == value.Month;

                    foreach (var day in firstMonth ? _days.GetViewFrom(temp.Year, temp.Month, temp.Day).Values : _days.GetValues(year, month))
                    {
                        firstDay &= firstMonth && day == value.Day;

                        foreach (var hour in firstDay ? _hours.GetViewFrom(temp.Hour).Values : _hours.Values)
                        {
                            firstHour &= firstDay && hour == value.Hour;

                            foreach (var minute in firstHour ? _minutes.GetViewFrom(temp.Minute).Values : _minutes.Values)
                            {
                                firstMinute &= firstHour && minute == value.Minute;

                                foreach (var second in firstMinute ? _seconds.GetViewFrom(temp.Second).Values : _seconds.Values)
                                {
                                    firstSecond &= firstMinute && second == value.Second;

                                    foreach (var millisecond in firstSecond ? _milliseconds.GetViewFrom(temp.Millisecond).Values : _milliseconds.Values)
                                    {
                                        firstMillisecond &= firstSecond && millisecond == value.Millisecond;

                                        foreach (var microsecond in firstMillisecond ? _microseconds.GetViewFrom(temp.GetMicroseconds()).Values : _microseconds.Values)
                                        {
                                            firstMicrosecond &= firstMillisecond && microsecond == value.GetMicroseconds();

                                            foreach (var nanosecond in firstMicrosecond ? _nanoseconds.GetViewFrom(temp.GetNanoseconds()).Values : _nanoseconds.Values)
                                            {
                                                var next = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, _offset).AddNanoseconds(microsecond * 1000 + nanosecond);
                                                if (next <= temp) { continue; }

                                                yield return next.ToOffset(value.Offset);
                                            }

                                            firstMicrosecond = false;
                                        }

                                        firstMillisecond = false;
                                    }

                                    firstSecond = false;
                                }

                                firstMinute = false;
                            }

                            firstHour = false;
                        }

                        firstDay = false;
                    }

                    firstMonth = false;
                }

                firstYear = false;
            }
        }

        public static CronExpression Parse(string expression)
        {
            return Parse(expression, DateTimeOffset.Now.Offset, CronFormat.Naive);
        }

        public static CronExpression Parse(string expression, TimeSpan offset)
        {
            return Parse(expression, offset, CronFormat.Naive);
        }

        public static CronExpression Parse(string expression, CronFormat format)
        {
            return Parse(expression, DateTimeOffset.Now.Offset, format);
        }

        public static CronExpression Parse(string expression, TimeSpan offset, CronFormat format)
        {
            string[] expressions = expression.Split(new[] { ExpressionSeperator }, StringSplitOptions.RemoveEmptyEntries);

            IRange nanoseconds;
            IRange microseconds;
            IRange milliseconds;
            IRange seconds;
            IRange minutes;
            IRange hours;
            IDayRange daysOfMonth;
            IRange months;
            IDayRange daysOfWeek;
            IRange years;

            if (format == CronFormat.Extended)
            {
                if (expressions.Length != 9 && expressions.Length != 10)
                {
                    throw new ArgumentException();
                }

                nanoseconds = ParseRange(expressions[0], 100, 0, 900);
                microseconds = ParseRange(expressions[1], 1, 0, 999);
                milliseconds = ParseRange(expressions[2], 1, 0, 999);
                seconds = ParseRange(expressions[3], 1, 0, 59);
                minutes = ParseRange(expressions[4], 1, 0, 59);
                hours = ParseRange(expressions[5], 1, 0, 23);
                daysOfMonth = ParseRangeDayOfMonth(expressions[6]);
                months = ParseRangeMonth(expressions[7]);
                daysOfWeek = ParseRangeDayOfWeek(expressions[8]);
                years = expressions.Length == 10 ? ParseRange(expressions[9], 1, DateTimeOffset.MinValue.Year, DateTimeOffset.MaxValue.Year) : null;
                return new CronExpression(nanoseconds, microseconds, milliseconds, seconds, minutes, hours, daysOfMonth, months, daysOfWeek, years, offset);
            }

            if (expressions.Length != 6 && expressions.Length != 7)
            {
                throw new ArgumentException();
            }

            nanoseconds = new ImplicitRange(100, 0, 0);
            microseconds = new ImplicitRange(1, 0, 0);
            milliseconds = new ImplicitRange(1, 0, 0);
            seconds = ParseRange(expressions[0], 1, 0, 59);
            minutes = ParseRange(expressions[1], 1, 0, 59);
            hours = ParseRange(expressions[2], 1, 0, 23);
            daysOfMonth = ParseRangeDayOfMonth(expressions[3]);
            months = ParseRangeMonth(expressions[4]);
            daysOfWeek = ParseRangeDayOfWeek(expressions[5]);
            years = expressions.Length == 7 ? ParseRange(expressions[6], 1, DateTimeOffset.MinValue.Year, DateTimeOffset.MaxValue.Year) : null;
            return new CronExpression(nanoseconds, microseconds, milliseconds, seconds, minutes, hours, daysOfMonth, months, daysOfWeek, years, offset);
        }

        private static IRange ParseRange(string expression, int resolution, int min, int max)
        {
            string[] tokens = expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var values = new SortedSet<int>();

            foreach (string token in tokens)
            {
                if (string.Equals(token, AllToken, StringComparison.Ordinal))
                {
                    return new ImplicitRange(resolution, min, max);
                }

                if (string.Equals(token, AnyToken, StringComparison.Ordinal))
                {
                    return new ImplicitRange(resolution, min, max);
                }

                int charIndex;

                if ((charIndex = token.IndexOf(RangeChar, StringComparison.Ordinal)) != -1)
                {
                    int start = int.Parse(token.Substring(0, charIndex));

                    if (start % resolution != 0)
                    {
                        throw new NotSupportedException();
                    }

                    int end = int.Parse(token.Substring(charIndex + 1));

                    for (int value = start; value <= end; value += resolution)
                    {
                        values.Add(value);
                    }

                    continue;
                }

                if ((charIndex = token.IndexOf(IncrementsSeperator, StringComparison.Ordinal)) != -1)
                {
                    string temp = token.Substring(0, charIndex);
                    int start = string.Equals(temp, AllToken, StringComparison.Ordinal) ? min : int.Parse(token.Substring(0, charIndex));

                    int interval = int.Parse(token.Substring(charIndex + 1));

                    for (int value = start; value <= max; value += interval)
                    {
                        if (value % resolution != 0)
                        {
                            throw new NotSupportedException();
                        }

                        values.Add(value);
                    }

                    continue;
                }

                values.Add(int.Parse(token));
            }

            return new ExplicitRange(values);
        }

        private static IDayRange ParseRangeDayOfMonth(string expression)
        {
            string[] tokens = expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var values = new SortedSet<int>();
            var valuesFromLast = new SortedSet<int>();
            var nearestWeekdays = new SortedSet<int>();

            foreach (string token in tokens)
            {
                if (string.Equals(token, AllToken, StringComparison.Ordinal))
                {
                    return new DayOfMonthRange(1, 31, null, null);
                }

                if (string.Equals(token, AnyToken, StringComparison.Ordinal))
                {
                    return null;
                }

                if (token.IndexOf(LastWeekdayToken, StringComparison.Ordinal) != -1)
                {
                    nearestWeekdays.Add(31);
                    continue;
                }

                int charIndex;

                if ((charIndex = token.IndexOf(LastChar, StringComparison.Ordinal)) != -1)
                {
                    if (token.Length != 1)
                    {
                        int value = int.Parse(token.Substring(charIndex + 2));
                        valuesFromLast.Add(value);

                        continue;
                    }

                    valuesFromLast.Add(0);
                    continue;
                }

                if ((charIndex = token.IndexOf(WeekdayChar, StringComparison.Ordinal)) != -1)
                {
                    nearestWeekdays.Add(int.Parse(token.Substring(0, charIndex)));
                    continue;
                }

                if ((charIndex = token.IndexOf(RangeChar, StringComparison.Ordinal)) != -1)
                {
                    int start = int.Parse(token.Substring(0, charIndex));
                    int end = int.Parse(token.Substring(charIndex + 1));

                    for (int value = start; value <= end; value++)
                    {
                        values.Add(value);
                    }

                    continue;
                }

                if ((charIndex = token.IndexOf(IncrementsSeperator, StringComparison.Ordinal)) != -1)
                {
                    string temp = token.Substring(0, charIndex);
                    int start = string.Equals(temp, AllToken, StringComparison.Ordinal) ? 1 : int.Parse(token.Substring(0, charIndex));

                    int interval = int.Parse(token.Substring(charIndex + 1));

                    for (int value = start; value <= 31; value += interval)
                    {
                        values.Add(value);
                    }

                    continue;
                }

                values.Add(int.Parse(token));
            }

            return new DayOfMonthRange(values, valuesFromLast, nearestWeekdays);
        }

        private static IRange ParseRangeMonth(string expression)
        {
            string[] tokens = expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var values = new SortedSet<int>();

            foreach (string token in tokens)
            {
                if (string.Equals(token, AllToken, StringComparison.Ordinal))
                {
                    return new ImplicitRange(1, 1, 12);
                }

                if (string.Equals(token, AnyToken, StringComparison.Ordinal))
                {
                    return new ImplicitRange(1, 1, 12);
                }

                int charIndex;

                if ((charIndex = token.IndexOf(RangeChar, StringComparison.Ordinal)) != -1)
                {
                    int start = ParseMonth(token.Substring(0, charIndex));
                    int end = ParseMonth(token.Substring(charIndex + 1));

                    for (int value = start; value <= end; value += 1)
                    {
                        values.Add(value);
                    }

                    continue;
                }

                if ((charIndex = token.IndexOf(IncrementsSeperator, StringComparison.Ordinal)) != -1)
                {
                    string temp = token.Substring(0, charIndex);
                    int start = string.Equals(temp, AllToken, StringComparison.Ordinal) ? 1 : ParseMonth(token.Substring(0, charIndex));

                    int interval = ParseMonth(token.Substring(charIndex + 1));

                    for (int value = start; value <= 12; value += interval)
                    {
                        values.Add(value);
                    }

                    continue;
                }

                values.Add(ParseMonth(token));
            }

            return new ExplicitRange(values);
        }

        private static IDayRange ParseRangeDayOfWeek(string expression)
        {
            string[] tokens = expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var values = new SortedSet<DayOfWeek>();
            var occurrences = new SortedSet<DayOfWeekRange.Occurence>();
            var occurrencesFromLast = new SortedSet<DayOfWeekRange.Occurence>();

            foreach (string token in tokens)
            {
                if (string.Equals(token, AllToken, StringComparison.Ordinal))
                {
                    return new DayOfWeekRange(DayOfWeek.Sunday, DayOfWeek.Saturday, null, null);
                }

                if (string.Equals(token, AnyToken, StringComparison.Ordinal))
                {
                    return null;
                }

                int charIndex;

                if ((charIndex = token.IndexOf(RangeChar, StringComparison.Ordinal)) != -1)
                {
                    var start = (int)ParseDayOfWeek(token.Substring(0, charIndex));
                    var end = (int)ParseDayOfWeek(token.Substring(charIndex + 1));

                    for (int value = start; value <= end; value++)
                    {
                        values.Add((DayOfWeek)value);
                    }

                    continue;
                }

                if ((charIndex = token.IndexOf(IncrementsSeperator, StringComparison.Ordinal)) != -1)
                {
                    string temp = token.Substring(0, charIndex);
                    int start = string.Equals(temp, AllToken, StringComparison.Ordinal) ? (int)DayOfWeek.Sunday : (int)ParseDayOfWeek(token.Substring(0, charIndex));

                    int interval = int.Parse(token.Substring(charIndex + 1));

                    for (int value = start; value <= (int)DayOfWeek.Saturday; value += interval)
                    {
                        values.Add((DayOfWeek)value);
                    }

                    continue;
                }

                if ((charIndex = token.IndexOf(OccurrenceSeperator, StringComparison.Ordinal)) != -1)
                {
                    DayOfWeek value = ParseDayOfWeek(token.Substring(0, charIndex));
                    int instance = int.Parse(token.Substring(charIndex + 1));

                    occurrences.Add(new DayOfWeekRange.Occurence(value, instance));
                    continue;
                }

                if (token.IndexOf(LastChar, StringComparison.Ordinal) != -1)
                {
                    if (token.Length == 1)
                    {
                        values.Add(DayOfWeek.Saturday);
                        continue;    
                    }

                    DayOfWeek value = ParseDayOfWeek(token.Substring(0, token.Length - 1));
                    occurrencesFromLast.Add(new DayOfWeekRange.Occurence(value, 1));

                    continue;
                }

                values.Add(ParseDayOfWeek(token));
            }

            return new DayOfWeekRange(values, occurrences, occurrencesFromLast);
        }
        
        private static DayOfWeek ParseDayOfWeek(string value)
        {
            switch (value)
            {
                case "SUN":
                    return DayOfWeek.Sunday;

                case "MON":
                    return DayOfWeek.Monday;

                case "TUE":
                    return DayOfWeek.Tuesday;

                case "WED":
                    return DayOfWeek.Wednesday;

                case "THU":
                    return DayOfWeek.Thursday;

                case "FRI":
                    return DayOfWeek.Friday;

                case "SAT":
                    return DayOfWeek.Saturday;
            }

            int number;

            if (int.TryParse(value, out number))
            {
                return (DayOfWeek)(number - 1);
            }

            throw new NotSupportedException();
        }

        private static int ParseMonth(string value)
        {
            switch (value)
            {
                case "JAN":
                    return 1;

                case "FEB":
                    return 2;

                case "MAR":
                    return 3;

                case "APR":
                    return 4;

                case "MAY":
                    return 5;

                case "JUN":
                    return 6;

                case "JUL":
                    return 7;

                case "AUG":
                    return 8;

                case "SEP":
                    return 9;

                case "OCT":
                    return 10;

                case "NOV":
                    return 11;

                case "DEC":
                    return 12;
            }

            int number;

            if (int.TryParse(value, out number))
            {
                return number;
            }

            throw new NotSupportedException();
        }
    }
}

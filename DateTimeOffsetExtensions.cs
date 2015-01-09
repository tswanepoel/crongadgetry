namespace CronGadgetry
{
    using System;

    public static class DateTimeOffsetExtensions
    {
        public static int GetMicroseconds(this DateTimeOffset value)
        {
            const decimal ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000m;
            return (int)(value.Ticks % ticksPerMicrosecond);
        }

        public static int GetNanoseconds(this DateTimeOffset value)
        {
            const decimal ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000000m;
            return (int)(value.Ticks % ticksPerNanosecond);
        }

        public static DateTimeOffset AddMicroseconds(this DateTimeOffset value, int microseconds)
        {
            const decimal ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000m;
            return value.AddTicks((int)(microseconds * ticksPerMicrosecond));
        }

        public static DateTimeOffset AddNanoseconds(this DateTimeOffset value, int nanoseconds)
        {
            const decimal ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000000m;
            return value.AddTicks((int)(nanoseconds * ticksPerNanosecond));
        }
    }
}

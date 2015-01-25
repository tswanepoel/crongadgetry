namespace CronGadgetry.Scheduling
{
    using System;

    public interface ITrigger
    {
        TimeSpan FireOffset { get; }
        DateTimeOffset? GetTimeAfter(DateTimeOffset value);
    }
}

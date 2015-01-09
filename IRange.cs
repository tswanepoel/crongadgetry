namespace CronGadgetry
{
    using System.Collections.Generic;

    public interface IRange
    {
        IEnumerable<int> Values { get; }
        IRange GetViewFrom(int value);
        IRange GetViewBetween(int min, int max);
        bool Contains(int value);
    }
}

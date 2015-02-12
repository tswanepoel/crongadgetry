namespace CronGadgetry.Collections
{
    using System.Collections.Generic;

    public interface IDayRange
    {
        bool Contains(int year, int month, int day);
        IEnumerable<int> GetValues(int year, int month);
        IRange GetViewFrom(int year, int month, int day);
        IRange GetViewBetween(int year, int month, int min, int max);
    }
}

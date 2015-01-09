namespace CronGadgetry
{
    public static class RangeExtensions
    {
        public static IRange Merge(this IRange self, IRange range)
        {
            return new MergedRange(self, range);
        }
    }
}

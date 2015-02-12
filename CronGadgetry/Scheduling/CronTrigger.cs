namespace CronGadgetry.Scheduling
{
    using System;

    public class CronTrigger : TriggerBase
    {
        private readonly CronExpression _expression;

        public CronTrigger(string expression)
            : this(CronExpression.Parse(expression), TimeSpan.Zero)
        {
        }

        public CronTrigger(string expression, CronFormat format)
            : this(CronExpression.Parse(expression, format), TimeSpan.Zero)
        {
        }

        public CronTrigger(string expression, CronFormat format, TimeSpan fireOffset)
            : this(CronExpression.Parse(expression, format), fireOffset)
        {
        }

        public CronTrigger(CronExpression expression)
            : this(expression, TimeSpan.Zero)
        {
        }

        public CronTrigger(CronExpression expression, TimeSpan fireOffset)
            : base(fireOffset)
        {
            _expression = expression;
        }

        public override DateTimeOffset? GetTimeAfter(DateTimeOffset value)
        {
            return _expression.GetTimeAfter(value);
        }
    }
}

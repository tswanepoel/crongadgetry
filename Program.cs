namespace CronGadgetry
{
    using Scheduling;
    using Timers;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Simple expression...");

            foreach (var value in CronExpression.Parse("0 0 0 ? JAN/2 6#2,6L")
                                                .GetAllTimesAfter(DateTimeOffset.Now)
                                                .Take(5))
            {
                Console.WriteLine(value);
            }

            Console.WriteLine();
            Console.WriteLine("Precise expression...");

            foreach (var value in CronExpression.Parse("* 0 0 0 0 0 1 JAN ?", CronFormat.Extended)
                                                .GetAllTimesAfter(DateTimeOffset.Now)
                                                .Take(999999999))
            {
                Console.WriteLine(value.ToString("O"/*ISO8601*/));
            }

            Console.WriteLine();
            Console.WriteLine("Simple enumeration...");

            var sw = new Stopwatch();
            sw.Start();

            foreach (var _ in CronExpression.Parse("0 0 0 * * * * * ?", CronFormat.Extended)
                                                .GetAllTimesAfter(DateTimeOffset.Now)
                                                .Take(100000))
            {
            }

            Console.WriteLine("100,000 times in {0} milliseconds.", sw.ElapsedMilliseconds);

            Console.WriteLine();
            Console.WriteLine("Timer...");

            using (var timer = new CronTimer(CronExpression.Parse("0 0 0/100 * * * * * ?", CronFormat.Extended)))
            {
                timer.Elapsed += (sender, e) => Console.WriteLine(e.Time.ToString("O"/*ISO8601*/));
                timer.Start();

                Thread.Sleep(TimeSpan.FromSeconds(1d));
            }

            Console.WriteLine();
            Console.WriteLine("Scheduler...");

            using (var scheduler = new Scheduler())
            {
                scheduler.Jobs.Add(new Job(
                    context => Console.WriteLine(context.Time.ToString("O"/*ISO8601*/)),
                    new CronTrigger("0 0 0/200 * * * * * ? *", CronFormat.Extended),
                    new CronTrigger("0 0 100/200 * * * * * ? *", CronFormat.Extended)));

                Thread.Sleep(TimeSpan.FromSeconds(1d));
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to quit.");

            Console.ReadKey(true);
        }
    }
}

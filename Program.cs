namespace CronGadgetry
{
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
                                                .Take(5))
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
                timer.Stop();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to quit.");

            Console.ReadKey(true);
        }
    }
}

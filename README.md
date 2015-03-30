# CRON Gadgetry
The CRON Gadgetry project aims to demonstrate how a CRON string can be parsed and enumerated for occurrences. This project further extends expressions with 3 additional tokens; _Millisecond_, _Microsecond_ and _Nanosecond_. Ultimately, they allow representing any System.DataTimeOffset, retaining its precision of up to 7 decimal places. That is, from 1/1/0001 12:00:00.0000000 AM to 12/31/9999 11:59:59.9999999 PM.

**Simple Format**

*Second* | *Minute* | *Hour* | *Day of Month* | *Month* | *Day of Week* | *Year (Optional)*

**Extended Format**

**_Nanosecond_** | **_Microsecond_** | **_Millisecond_** | *Second* | *Minute* | *Hour* | *Day of Month* | *Month* | *Day of Week* | *Year (Optional)*

> System.DateTimeOffset only specifies up to hundredth's of nanoseconds (at the 7th decimal place). Therefore, _Nanosecond_ may only be specified in hundredth's, accordingly. For example, 0, 100, 200, 300, .. 900.

### Timer

This demonstration also includes a CronTimer that raises events for occurrences. Additionally, the timer may be created with a System.TimeSpan specified to offset the raising of these events. This is useful when handlers require time to prepare. For example, when specifying TimeSpan.FromSeconds(-1d) as the offset, events will all be raised 1 second early, allowing handlers to connect to a database, read configuration data or perform logging. Within this event handler, a call to the CronTimerEventArgs.Wait blocks the calling thread until the offset elapses to resume execution at the actual time intended for that occurrence.

### Performance
Results were determined by timing how long it takes, in seconds, to work out a number of System.DateTimeOffset values.  

Expression: * * * * * ? (every second)

|               | Quartz  | Gadgetry |
| -------------:| -------:| --------:|
|     **1,000** |   0.042 |    0.007 |
|    **10,000** |   0.092 |    0.013 |
|   **100,000** |   0.591 |    0.071 |
| **1,000,000** |   5.597 |    0.626 |

---
Expression: 0 * 8-16 ? * MON-FRI (every minute of every business hour on weekdays)

|               | Quartz  | Gadgetry |
| -------------:| -------:| --------:|
|     **1,000** |   0.041 |    0.015 |
|    **10,000** |   0.094 |    0.023 |
|   **100,000** |   0.603 |    0.095 |
| **1,000,000** |   5.715 |    0.785 |

---
Expression: * * * * * * * * ? (every 100 nanoseconds)

|               | Quartz  | Gadgetry |
| -------------:| -------:| --------:|
|     **1,000** |      NA |    0.007 |
|    **10,000** |      NA |    0.011 |
|   **100,000** |      NA |    0.052 |
| **1,000,000** |      NA |    0.441 |

> These results can obviously vary depending on different hardware or circumstances. By no means are they intended to be complete or used for anything other than perhaps noticing that, in this implementation, attention is particularly given to performance. I think Quartz is awesome and does many more other things that we can't even do here.

---
Here are some code snippets that illustrate how the results were determined.

**Quartz .NET v2.3 (2.3.2.0)**
```C#
var quartz = new CronExpression(expr);

var sw = new Stopwatch();
sw.Start();
            
DateTimeOffset? next = start;

for (int i = 0; i < count; i++)
{
    next = quartz.GetNextValidTimeAfter((DateTimeOffset)next);

    if (next == null)
        break;
}

Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
```

**Gadgetry**
```C#
var gadgetry = CronExpression.Parse(expr);

var sw = new Stopwatch();
sw.Start();

foreach (var _ in gadgetry.GetAllTimesAfter(start).Take(count))
{
}

Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
```

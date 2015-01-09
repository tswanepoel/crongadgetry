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
These results were determined by timing how long it takes to work out a number of System.DateTimeOffet values.  

Expression: * * * * * ? (every second)

|               | Quartz  | Gadgetry |
| -------------:| -------:| --------:|
|     **1,000** |   0.099 |    0.001 |
|    **10,000** |   0.983 |    0.009 |
|   **100,000** |   9.902 |    0.094 |
| **1,000,000** |  97.376 |    0.932 |

---
Expression: 0 0 8-16 ? * MON-FRI (every business hour on weekdays)

|               | Quartz  | Gadgetry |
| -------------:| -------:| --------:|
|     **1,000** |   0.047 |    0.016 |
|    **10,000** |   0.082 |    0.001 |
|   **100,000** |   0.644 |    0.013 |
| **1,000,000** |   5.335 |    0.131 |

---
Expression: * * * * * * * * ? (every 100 nanoseconds)

|               | Quartz  | Gadgetry |
| -------------:| -------:| --------:|
|     **1,000** |      NA |    0.003 |
|    **10,000** |      NA |    0.008 |
|   **100,000** |      NA |    0.055 |
| **1,000,000** |      NA |    0.531 |

---
The code used to determine these results looks something like this.

**Quartz**
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

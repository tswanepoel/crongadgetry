# CRON Gadgetry
The CRON Gadgetry project aims to demonstrate how a CRON string can be parsed and enumerated for occurrences. This project further extends expressions with 3 additional tokens; Milliseconds, Microseconds and Nanoseconds. Ultimately, they allow representing any System.DataTimeOffset, retaining its precision of up to 7 decimal places. That is, from 1/1/0001 12:00:00.0000000 AM to 12/31/9999 11:59:59.9999999 PM.

### Simple Format
*Second* | *Minute* | *Hour* | *Day of Month* | *Month* | *Day of Week* | *Year (Optional)*

### Extended Format
**_Nanosecond_** | **_Microsecond_** | **_Millisecond_** | *Second* | *Minute* | *Hour* | *Day of Month* | *Month* | *Day of Week* | *Year (Optional)*

> System.DateTimeOffset only specifies up to hundredth's of nanoseconds (at the 7th decimal place). Therefore, nanoseconds may only be specified in hundredth's, accordingly. For example, 0, 100, 200, 300, .. 900.

### CRON Timer

This demonstration also includes a CronTimer that raises events for occurrences. Additionally, the timer may be created with a System.TimeSpan specified to offset the raising of these events. This is useful when handlers require time to prepare. For example, when specifying TimeSpan.FromSeconds(-1d) as the offset, events will all be raised 1 second early, allowing handlers to connect to a database, read configuration data or perform logging. Within this event handler, a call to the CronTimerEventArgs.Wait blocks the calling thread until the offset elapses to resume execution at the actual time intended for that occurrence.

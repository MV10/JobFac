using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

//
//  IMPORTANT: 
//  ALL SCHEDULING/TIMING USES THE NODA TIME LIBRARY.
//  NODA TIME "INSTANT" IS ONLY CONVERTED TO UTC DATETIMEOFFSET FOR SQL STORAGE.
//

// Date-based information about a schedule target; used to determine
// whether the job should be scheduled for the date in question.

namespace JobFac.Library
{
    public class DateTimeAnalysis
    {
        public DateTimeAnalysis(LocalDate forZonedTargetDate)
        {
            // the scheduler doesn't use the time of day, which is 
            // discarded before DateTimeAnalysis is created
            Time = forZonedTargetDate.AtMidnight().TimeOfDay;
            Constructor(forZonedTargetDate);
        }

        public DateTimeAnalysis(string nowForTimeZone)
        {
            var tz = DateTimeZoneProviders.Tzdb[nowForTimeZone];
            var now = SystemClock.Instance.GetCurrentInstant().InZone(tz);
            Time = now.TimeOfDay;
            Constructor(now.Date);
        }

        public LocalDate Date { get; private set; }  // adjusted to ScheduleTimeZone from the JobDefinition
        public LocalTime Time { get; private set; }  // only used by sequence steps, not used by scheduler

        public bool DateIsToday { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }
        public int LastDayOfMonth { get; private set; }
        public string DayOfWeek { get; private set; }
        public string DayOfMonth { get; private set; }
        public string MonthAndDay { get; private set; } // mm/dd
        public bool IsFirstDayOfMonth { get; private set; }
        public bool IsLastDayOfMonth { get; private set; }
        public bool IsWeekday { get; private set; }
        public bool IsFirstWeekdayOfMonth { get; private set; }
        public bool IsLastWeekdayOfMonth { get; private set; }

        public string HourMinute { get; private set; }  // HHmm
        public string Hour { get; private set; }        // HH
        public string Minute { get; private set; }      // mm

        private void Constructor(LocalDate forZonedTargetDate)
        {
            Date = forZonedTargetDate;
            LastDayOfMonth = Date.With(DateAdjusters.EndOfMonth).Day;
            Month = Date.Month;
            Day = Date.Day;
            DayOfWeek = ((int)Date.DayOfWeek).ToString();
            DayOfMonth = Day.ToString();
            MonthAndDay = $"{Date:MM/dd}";
            IsFirstDayOfMonth = Day == 1;
            IsLastDayOfMonth = Day == LastDayOfMonth;

            IsWeekday = Date.DayOfWeek < IsoDayOfWeek.Saturday;
            IsFirstWeekdayOfMonth = Day == FirstWeekdayOfMonth();
            IsLastWeekdayOfMonth = Day == LastWeekdayOfMonth();

            var fmt = CultureInfo.InvariantCulture.DateTimeFormat;
            HourMinute = Time.ToString("HHmm", fmt);
            Hour = Time.ToString("HH", fmt);
            Minute = Time.ToString("mm", fmt);
        }

        public bool InDaysOfWeek(List<string> dates)
            => dates.Any(d => d.Equals(DayOfWeek));

        public bool InDaysOfMonth(List<string> dates)
            => dates.Any(d => d.Equals(DayOfMonth))
            || (IsFirstDayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
            || (IsLastDayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase)));

        public bool InSpecificDates(List<string> dates)
            => dates.Any(d => d.Equals(MonthAndDay));

        public bool InDateRanges(List<string> dates)
            => dates.Any(d => InDateRange(d));

        public bool InWeekdays(List<string> dates)
            => (IsFirstWeekdayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
            || (IsLastWeekdayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase)))
            || (IsWeekday && dates.Any(d => d.Equals("weekday", StringComparison.OrdinalIgnoreCase)))
            || (!IsWeekday && dates.Any(d => d.Equals("weekend", StringComparison.OrdinalIgnoreCase)));

        public bool InHours(List<string> times)
            => times.Any(t => t.Equals(Hour));

        public bool InMinutes(List<string> times)
            => times.Any(t => t.Equals(Minute));

        public bool InSpecificTimes(List<string> times)
            => times.Any(t => t.Equals(HourMinute));

        public bool InTimeRanges(List<string> times)
            => times.Any(t => InTimeRange(t));

        public bool InDateRange(string range)
            => InRange(range, Month, Day);

        public bool InTimeRange(string range)
            => InRange(range, Time.Hour, Time.Minute);

        private bool InRange(string range, int major, int minor)
        {
            // Ranges can be dates like "12/10-12/20" or "12-10-12-20" or times
            // such as "0945-1030" or "09:45-10:30" but this strips them down to
            // eight characters ... 12101220 or 09451030.
            var r = range.Replace(" ", "").Replace("/", "").Replace(":", "").Replace("-", "");

            var A1 = int.Parse(range.Substring(0, 2));
            var B1 = int.Parse(range.Substring(2, 2));
            var A2 = int.Parse(range.Substring(4, 2));
            var B2 = int.Parse(range.Substring(6, 2));

            var c1 = major > A1 || (major == A1 && minor >= B1);
            var c2 = major < A2 || (major == A2 && minor <= B2);

            // Date-range crosses the year-boundary (ex. 12/01-02/01)
            // or time-range crosses the day-boundary (ex. 2300-0200)
            if (A1 > A2 || (A1 == A2 && B1 > B2)) return c1 || c2;

            // Range is within the same year or day:
            return c1 && c2;
        }

        // Works for HHmm or HH:mm. 
        public static (int, int) GetHourMinute(string HHmm)
        {
            int n = HHmm.Length - 2;
            int hour = int.Parse(HHmm.Substring(0, n));
            int minute = int.Parse(HHmm.Substring(n, 2));
            return (hour, minute);
        }

        private int FirstWeekdayOfMonth()
        {
            var result = Date.With(DateAdjusters.StartOfMonth);
            while (result.DayOfWeek == IsoDayOfWeek.Saturday || result.DayOfWeek == IsoDayOfWeek.Sunday)
                result.PlusDays(1);
            return result.Day;
        }

        private int LastWeekdayOfMonth()
        {
            var result = Date.With(DateAdjusters.EndOfMonth);
            while (result.DayOfWeek == IsoDayOfWeek.Saturday || result.DayOfWeek == IsoDayOfWeek.Sunday)
                result.PlusDays(-1);
            return result.Day;
        }
    }
}

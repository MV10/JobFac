using NodaTime;
using System;
using System.Collections.Generic;
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
            => Constructor(forZonedTargetDate);

        public DateTimeAnalysis(string nowForTimeZone)
        {
            var tz = DateTimeZoneProviders.Tzdb[nowForTimeZone];
            var now = SystemClock.Instance.GetCurrentInstant().InZone(tz).Date;
            Constructor(now);
        }

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
        }

        public LocalDate Date { get; private set; }  // adjusted to ScheduleTimeZone from the JobDefinition

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

        public bool InDaysOfWeek(List<string> dates)
            => dates.Any(d => d.Equals(DayOfWeek));

        public bool InDaysOfMonth(List<string> dates)
            => dates.Any(d => d.Equals(DayOfMonth))
            || (IsFirstDayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
            || (IsLastDayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase)));

        public bool InSpecificDates(List<string> dates)
            => dates.Any(d => d.Equals(MonthAndDay));

        public bool InDateRange(List<string> dates)
            => dates.Any(d => InDateRange(d));

        public bool InWeekdays(List<string> dates)
            => (IsFirstWeekdayOfMonth && dates.Any(d => d.Equals("first", StringComparison.OrdinalIgnoreCase)))
            || (IsLastWeekdayOfMonth && dates.Any(d => d.Equals("last", StringComparison.OrdinalIgnoreCase)))
            || (IsWeekday && dates.Any(d => d.Equals("weekday", StringComparison.OrdinalIgnoreCase)))
            || (!IsWeekday && dates.Any(d => d.Equals("weekend", StringComparison.OrdinalIgnoreCase)));

        // The date range values should be in the exact format mm/dd-mm/dd. This method determines
        // if target.Date is in that range, inclusive of the start/end dates for the range.
        public bool InDateRange(string range)
        {
            var m1 = int.Parse(range.Substring(0, 2));
            var d1 = int.Parse(range.Substring(3, 2));
            var m2 = int.Parse(range.Substring(6, 2));
            var d2 = int.Parse(range.Substring(9, 2));

            var c1 = Month > m1 || (Month == m1 && Day >= d1);
            var c2 = Month < m2 || (Month == m2 && Day <= d2);

            // Range crosses the year-boundary (ex. 12/01-02/01):
            if (m1 > m2 || (m1 == m2 && d1 > d2)) return c1 || c2;

            // Range is within the year:
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

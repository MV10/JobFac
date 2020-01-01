using NodaTime;

//
//  IMPORTANT: 
//  ALL SCHEDULING/TIMING USES THE NODA TIME LIBRARY.
//  NODA TIME "INSTANT" IS ONLY CONVERTED TO UTC DATETIMEOFFSET FOR SQL STORAGE.
//

// Date-based information about a schedule target; used to determine
// whether the job should be scheduled for the date in question.

namespace JobFac.Services.Scheduling
{
    public class TargetDateAnalysis
    {
        public TargetDateAnalysis(LocalDate forZonedTargetDate)
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
        public bool IsFirstWeekdayOfMonth { get; private set; }
        public bool IsLastWeekdayOfMonth { get; private set; }

        // For ScheduleDateMode.DateRanges the ScheduleDates values should be date ranges in the
        // format mm/dd-mm/dd (exactly that format); this determines if target.Date is in that
        // range (inclusive of the start/end dates for the range).
        public bool InDateRange(string range)
        {
            var m1 = int.Parse(range.Substring(0, 2));
            var d1 = int.Parse(range.Substring(3, 2));
            var m2 = int.Parse(range.Substring(6, 2));
            var d2 = int.Parse(range.Substring(9, 2));
            return ((Month > m1 || (Month == m1 && Day >= d1)) && (Month < m2 || (Month == m2 && Day <= d2)));
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

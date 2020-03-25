using System;
using System.Collections.Generic;

namespace Schedulebot.Schedule
{
    public class ScheduleWeek
    {
        public ScheduleDay[] days;

        public int DaysAmount { get; } = 6;
        
        public ScheduleWeek()
        {
            days = new ScheduleDay[DaysAmount];
            for (int i = 0; i < DaysAmount; ++i)
                days[i] = new ScheduleDay();
        }

        public ScheduleWeek(int daysAmount)
        {
            DaysAmount = daysAmount;
            days = new ScheduleDay[DaysAmount];
            for (int i = 0; i < DaysAmount; ++i)
                days[i] = new ScheduleDay();
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleWeek week
                && EqualityComparer<ScheduleDay[]>.Default.Equals(days, week.days);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(days);
        }

        public static bool operator ==(ScheduleWeek week1, ScheduleWeek week2)
        {
            if (week1.DaysAmount != week2.DaysAmount)
                return false;
            for (int i = 0; i < week1.DaysAmount; ++i)
            {
                if (week1.days[i] != week2.days[i])
                    return false;
            }
            return true;
        }
        public static bool operator !=(ScheduleWeek week1, ScheduleWeek week2)
        {
            return !(week1 == week2);
        }
    }
}
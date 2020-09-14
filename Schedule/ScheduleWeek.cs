using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleWeek : ICloneable
    {
        public ScheduleDay[] days;

        public int DaysCount { get; }

        public ScheduleWeek(int daysCount = 6)
        {
            DaysCount = daysCount;
            days = new ScheduleDay[DaysCount];
            for (int i = 0; i < DaysCount; ++i)
                days[i] = new ScheduleDay();
        }

        public object Clone()
        {
            ScheduleWeek clone = new ScheduleWeek(DaysCount);
            for (int i = 0; i < DaysCount; i++)
                clone.days[i] = new ScheduleDay(days[i]);
            return clone;
        }

        public void SortLectures()
        {
            for (int i = 0; i < DaysCount; i++)
            {
                days[i].SortLectures();
            }
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
            if (week1.DaysCount != week2.DaysCount)
                return false;
            for (int i = 0; i < week1.DaysCount; ++i)
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
using System;
using System.Collections.Generic;

namespace Schedulebot.Schedule
{
    public class ScheduleWeek
    {
        public ScheduleDay[] days = new ScheduleDay[6];
        
        public ScheduleWeek()
        {
            for (int i = 0; i < 6; ++i)
                days[i] = new ScheduleDay();
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleWeek week &&
                   EqualityComparer<ScheduleDay[]>.Default.Equals(days, week.days);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(days);
        }

        public static bool operator ==(ScheduleWeek week1, ScheduleWeek week2)
        {
            for (int i = 0; i < 6; ++i)
            {
                if (week1.days[i] != week2.days[i])
                    return false;
            }
            return true;
        }
        public static bool operator !=(ScheduleWeek week1, ScheduleWeek week2)
        {
            for (int i = 0; i < 2; ++i)
            {
                if (week1.days[i] != week2.days[i])
                    return true;
            }
            return false;
        }
    }
}
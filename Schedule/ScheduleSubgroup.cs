using System;
using System.Collections.Generic;

namespace Schedulebot.Schedule
{
    public class ScheduleSubgroup
    {
        public ScheduleWeek[] weeks;
        public long PhotoId { get; set; } = 0; // вынести

        public int WeeksCount { get; }

        public ScheduleSubgroup(int weeksCount = 2)
        {
            WeeksCount = weeksCount;
            weeks = new ScheduleWeek[WeeksCount];
            for (int i = 0; i < WeeksCount; ++i)
                weeks[i] = new ScheduleWeek();
        }

        public void SortLectures()
        {
            for (int i = 0; i < WeeksCount; i++)
            {
                weeks[i].SortLectures();
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleSubgroup subgroup
                && EqualityComparer<ScheduleWeek[]>.Default.Equals(weeks, subgroup.weeks);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(weeks);
        }

        public static bool operator ==(ScheduleSubgroup schedule1, ScheduleSubgroup schedule2)
        {
            if (schedule1.WeeksCount != schedule2.WeeksCount)
                return false;
            for (int i = 0; i < schedule1.WeeksCount; ++i)
            {
                if (schedule1.weeks[i] != schedule2.weeks[i])
                    return false;
            }
            return true;
        }
        
        public static bool operator !=(ScheduleSubgroup schedule1, ScheduleSubgroup schedule2)
        {
            return !(schedule1 == schedule2);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Schedulebot.Schedule
{
    public class ScheduleSubgroup
    {
        public ScheduleWeek[] weeks;
        public long PhotoId { get; set; } = 0; // вынести

        public int SubgroupsAmount { get; } = 2;

        public ScheduleSubgroup()
        {
            weeks = new ScheduleWeek[SubgroupsAmount];
            for (int i = 0; i < SubgroupsAmount; ++i)
                weeks[i] = new ScheduleWeek();
        }

        public ScheduleSubgroup(int subgroupsAmount)
        {
            SubgroupsAmount = subgroupsAmount;
            weeks = new ScheduleWeek[SubgroupsAmount];
            for (int i = 0; i < SubgroupsAmount; ++i)
                weeks[i] = new ScheduleWeek();
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
            if (schedule1.SubgroupsAmount != schedule2.SubgroupsAmount)
                return false;
            for (int i = 0; i < schedule1.SubgroupsAmount; ++i)
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
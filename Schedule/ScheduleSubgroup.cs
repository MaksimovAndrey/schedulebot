namespace Schedulebot.Schedule
{
    public class ScheduleSubgroup
    {
        public ScheduleWeek[] weeks = new ScheduleWeek[2];
        public ScheduleSubgroup()
        {
            for (int i = 0; i < 2; ++i)
                weeks[i] = new ScheduleWeek();
        }
        public static bool operator ==(ScheduleSubgroup schedule1, ScheduleSubgroup schedule2)
        {
            for (int i = 0; i < 2; ++i)
            {
                if (schedule1.weeks[i] != schedule2.weeks[i])
                    return false;
            }
            return true;
        }
        public static bool operator !=(ScheduleSubgroup schedule1, ScheduleSubgroup schedule2)
        {
            for (int i = 0; i < 2; ++i)
            {
                if (schedule1.weeks[i] != schedule2.weeks[i])
                    return true;
            }
            return false;
        }
    }
}
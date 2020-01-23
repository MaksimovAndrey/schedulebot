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
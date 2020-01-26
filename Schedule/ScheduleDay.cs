namespace Schedulebot.Schedule
{
    public class ScheduleDay
    {
        public ScheduleLecture[] lectures = new ScheduleLecture[8];
        public ulong photoId = 0;
        public ScheduleDay()
        {
            for (int i = 0; i < 8; ++i)
                lectures[i] = new ScheduleLecture();
        }
        public static bool operator ==(ScheduleDay day1, ScheduleDay day2)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (day1.lectures[i] != day2.lectures[i])
                    return false;
            }
            return true;
        }
        public static bool operator !=(ScheduleDay day1, ScheduleDay day2)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (day1.lectures[i] != day2.lectures[i])
                    return true;
            }
            return false;
        }
        public bool IsEmpty()
        {
            for (int i = 0; i < 8; ++i)
                if (!lectures[i].IsEmpty())
                    return false;
            return true;
        }
        public int CountOfLectures()
        {
            int count = 0;
            for (int i = 0; i < 8; ++i)
            {
                if (!lectures[i].IsEmpty())
                    ++count;
            }
            return count;
        }
    }
}
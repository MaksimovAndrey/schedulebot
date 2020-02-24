using System;
using System.Collections.Generic;

namespace Schedulebot.Schedule
{
    public class ScheduleDay
    {
        public ScheduleLecture[] lectures;
        public bool isStudying = false;
        public long PhotoId { get; set; } = 0; // вынести 

        public int LecturesAmount { get; } = 8;
        
        public ScheduleDay()
        {
            lectures = new ScheduleLecture[LecturesAmount];
            for (int i = 0; i < LecturesAmount; ++i)
                lectures[i] = new ScheduleLecture();
        }

        public ScheduleDay(int lecturesAmount)
        {
            if (lecturesAmount < 1)
                throw new ArgumentOutOfRangeException("lecturesAmount", lecturesAmount, "Количество пар в день не может быть меньше 1");
            LecturesAmount = lecturesAmount;
            lectures = new ScheduleLecture[LecturesAmount];
            for (int i = 0; i < LecturesAmount; ++i)
                lectures[i] = new ScheduleLecture();
        }
        
        public static bool operator ==(ScheduleDay day1, ScheduleDay day2)
        {
            if (day1.LecturesAmount != day2.LecturesAmount)
                return false;
            for (int i = 0; i < day1.LecturesAmount; ++i)
            {
                if (day1.lectures[i] != day2.lectures[i])
                    return false;
            }
            return true;
        }
        
        public static bool operator !=(ScheduleDay day1, ScheduleDay day2)
        {
            return !(day1 == day2);
        }
        
        public bool IsEmpty()
        {
            for (int i = 0; i < LecturesAmount; ++i)
                if (!lectures[i].IsEmpty())
                    return false;
            return true;
        }
        
        public int CountOfLectures()
        {
            int count = 0;
            for (int i = 0; i < LecturesAmount; ++i)
            {
                if (!lectures[i].IsEmpty())
                    ++count;
            }
            return count;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lectures, isStudying);
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleDay day &&
                   EqualityComparer<ScheduleLecture[]>.Default.Equals(lectures, day.lectures) &&
                   isStudying == day.isStudying;
        }
    }
}
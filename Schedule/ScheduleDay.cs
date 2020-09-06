using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleDay
    {
        public List<ScheduleLecture> lectures;
        public long PhotoId { get; set; } = 0; // вынести 
        public bool IsStudying => lectures.Count == 0 ? false : true;
        
        public ScheduleDay()
        {
            lectures = new List<ScheduleLecture>();
        }

        public ScheduleDay(ScheduleDay day)
        {
            lectures = new List<ScheduleLecture>(day.lectures);
            PhotoId = day.PhotoId;
        }

        public void SortLectures()
        {
            if (lectures.Count == 0)
                return;

            List<ScheduleLecture> result = new List<ScheduleLecture>();
            List<int> startTimes = new List<int>();
            for (int currentLecture = 0; currentLecture < lectures.Count; currentLecture++)
            {
                startTimes.Add(lectures[currentLecture].TimeStartToInt());
            }
            while (startTimes.Count > 1)
            {
                int minIndex = 0;
                int minTime = startTimes[0];
                for (int currentTime = 1; currentTime < startTimes.Count; currentTime++)
                {
                    if (startTimes[currentTime] < minTime)
                    {
                        minTime = startTimes[currentTime];
                        minIndex = currentTime;
                    }
                }
                result.Add(lectures[minIndex]);
                lectures.RemoveAt(minIndex);
                startTimes.RemoveAt(minIndex);
            }
            result.Add(lectures[0]);
            lectures.RemoveAt(0); // lectures.Clear();
            lectures = result;
        }

        public bool IsEmpty()
        {
            return lectures.Count == 0 ? true : false;
        }

        public string GetChanges(List<ScheduleLecture> newLectures)
        {
            int currentLecture;
            int minLectures = Math.Min(lectures.Count, newLectures.Count);
            StringBuilder changesBuilder = new StringBuilder();

            for (currentLecture = 0; currentLecture < minLectures; currentLecture++)
            {
                if (lectures[currentLecture] != newLectures[currentLecture])
                {
                    changesBuilder.Append('-');
                    changesBuilder.Append(lectures[currentLecture].ToString());
                    changesBuilder.Append("\n+");
                    changesBuilder.Append(newLectures[currentLecture].ToString());
                    changesBuilder.Append('\n');
                }
            }
            for (int lectureIndex = currentLecture + 1; lectureIndex < lectures.Count; lectureIndex++)
            {
                changesBuilder.Append('-');
                changesBuilder.Append(lectures[lectureIndex].ToString());
                changesBuilder.Append('\n');
            }
            for (int lectureIndex = currentLecture + 1; lectureIndex < newLectures.Count; lectureIndex++)
            {
                changesBuilder.Append('+');
                changesBuilder.Append(newLectures[lectureIndex].ToString());
                changesBuilder.Append('\n');
            }

            return changesBuilder.ToString();
        }
        
        public static bool operator ==(ScheduleDay day1, ScheduleDay day2)
        {
            if (day1.lectures.Count != day2.lectures.Count)
                return false;
            for (int i = 0; i < day1.lectures.Count; ++i)
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

        public override int GetHashCode()
        {
            return HashCode.Combine(lectures);
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleDay day && EqualityComparer<List<ScheduleLecture>>.Default.Equals(lectures, day.lectures);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleDay
    {
        public List<ScheduleLecture> lectures;
        public long PhotoId { get; set; } = 0; // вынести 
        public bool IsStudying => lectures.Count != 0;

        public ScheduleDay()
        {
            lectures = new List<ScheduleLecture>();
        }

        public ScheduleDay(ScheduleDay day)
        {
            lectures = new List<ScheduleLecture>(day.lectures);
            PhotoId = day.PhotoId;
        }

        /// <summary>
        /// Сортирует пары
        /// </summary>
        public void SortLectures()
        {
            if (lectures.Count == 0)
                return;

            List<ScheduleLecture> result = new List<ScheduleLecture>();
            List<int> startTimes = new List<int>();
            for (int currentLecture = 0; currentLecture < lectures.Count; currentLecture++)
            {
                startTimes.Add(Utils.Converter.TimeToInt(lectures[currentLecture].TimeStart));
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

        /// <summary>
        /// Ищет изменения в расписании в этот день
        /// <br>Обращаемся к новому дню, передаем устаревшие пары</br>
        /// </summary>
        /// <param name="oldLectures">Устаревшие пары</param>
        /// <returns>Строка с изменениями</returns>
        public string GetChanges(List<ScheduleLecture> oldLectures)
        {
            int minOfOldNewLectures = Math.Min(lectures.Count, oldLectures.Count);
            StringBuilder changesBuilder = new StringBuilder();

            for (int currentLecture = 0; currentLecture < minOfOldNewLectures; currentLecture++)
            {
                if (lectures[currentLecture] != oldLectures[currentLecture])
                {
                    changesBuilder.Append('-');
                    changesBuilder.Append(oldLectures[currentLecture].ToString());
                    changesBuilder.Append("\n+");
                    changesBuilder.Append(lectures[currentLecture].ToString());
                    changesBuilder.Append('\n');
                }
            }
            for (int lectureIndex = minOfOldNewLectures; lectureIndex < oldLectures.Count; lectureIndex++)
            {
                changesBuilder.Append('-');
                changesBuilder.Append(oldLectures[lectureIndex].ToString());
                changesBuilder.Append('\n');
            }
            for (int lectureIndex = minOfOldNewLectures; lectureIndex < lectures.Count; lectureIndex++)
            {
                changesBuilder.Append('+');
                changesBuilder.Append(lectures[lectureIndex].ToString());
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
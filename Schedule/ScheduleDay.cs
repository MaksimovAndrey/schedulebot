using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleDay : IEquatable<ScheduleDay>
    {
        public List<ScheduleLecture> lectures;
        public DateTime Date { get; }
        public long PhotoId { get; set; } = 0; // вынести 
        public bool IsStudying => lectures.Count != 0;

        public ScheduleDay(DateTime date)
        {
            lectures = new List<ScheduleLecture>();
            Date = date;
        }

        public ScheduleDay(ScheduleDay day)
        {
            lectures = new List<ScheduleLecture>(day.lectures);
            PhotoId = day.PhotoId;
            Date = day.Date;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append("📅");
            str.Append(Date.ToString("dd'.'MM'.'yyyy"));
            str.Append(Constants.delimiter);
            str.Append(CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(Date.DayOfWeek));
            for (int i = 0; i < lectures.Count; i++)
            {
                str.Append("\n\n");
                str.Append(lectures[i].ToString());
            }

            return str.ToString();
        }

        /// <summary>
        /// !!!НЕ ИСПОЛЬЗОВАТЬ, НЕ РАБОТАЕТ!!!
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

        public override bool Equals(object obj)
        {
            return Equals(obj as ScheduleDay);
        }

        public bool Equals(ScheduleDay other)
        {
            return other != null &&
                   EqualityComparer<List<ScheduleLecture>>.Default.Equals(lectures, other.lectures) &&
                   Date == other.Date;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lectures, Date);
        }

        public static bool operator ==(ScheduleDay day1, ScheduleDay day2)
        {
            if (day1.Date != day2.Date 
                || day1.lectures.Count != day2.lectures.Count)
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
    }
}
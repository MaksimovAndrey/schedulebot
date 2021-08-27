using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleDay : IEquatable<ScheduleDay>
    {
        public List<ScheduleLecture> lectures;
        public DateTime Date { get; }
        public long PhotoId { get; set; } = 0;
        public bool IsPhotoUploading = false;

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

        public string GetDateString()
        {
            return Date.ToString("dd'.'MM'.'yyyy");
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append("📅");
            str.Append(Date.ToString("dd'.'MM'.'yyyy"));
            str.Append(Constants.delimiter);
            str.Append(Utils.Converter.DayOfWeekToString(Date.DayOfWeek));
            for (int i = 0; i < lectures.Count; i++)
            {
                str.Append("\n\n");
                str.Append(lectures[i].ToString());
            }

            return str.ToString();
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

        public bool EqualDay(ScheduleDay other)
        {
            if (this.lectures.Count != other.lectures.Count
                || this.Date.DayOfWeek != other.Date.DayOfWeek)
                return false;
            for (int i = 0; i < this.lectures.Count; ++i)
            {
                if (this.lectures[i] != other.lectures[i])
                    return false;
            }
            return true;
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

using System;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleLecture : IEquatable<ScheduleLecture>
    {
        public string TimeStart { get; }
        public string TimeEnd { get; }

        public string Subject { get; }
        public string LectureHall { get; }
        public string Lecturer { get; }

        public string Type { get; }

        public ScheduleLecture(
            string timeStart = "",
            string timeEnd = "",

            string subject = null,
            string lectureHall = null,
            string lecturer = null,

            string type = ""
        )
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;

            Subject = subject;
            LectureHall = lectureHall;
            Lecturer = lecturer;

            Type = type;
        }

        public ScheduleLecture(
            Parsing.Utils.ParsedLecture parsedLecture
        )
        {
            TimeStart = parsedLecture.BeginLesson;
            TimeEnd = parsedLecture.EndLesson;

            Subject = parsedLecture.Discipline;
            LectureHall = parsedLecture.Auditorium + " (" + parsedLecture.Building + ")";
            Lecturer = parsedLecture.Lecturer;

            Type = parsedLecture.KindOfWork;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append("🕖");
            str.Append(TimeStart);
            str.Append(" - ");
            str.Append(TimeEnd);
            str.Append('\n');
            str.Append("🏢");
            str.Append(LectureHall);
            str.Append(Constants.delimiter);
            str.Append(Type);
            str.Append('\n');
            str.Append("👤");
            str.Append(Lecturer);
            str.Append('\n');
            str.Append(Subject);

            return str.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ScheduleLecture);
        }

        public bool Equals(ScheduleLecture other)
        {
            return other != null &&
                   TimeStart == other.TimeStart &&
                   TimeEnd == other.TimeEnd &&
                   Subject == other.Subject &&
                   LectureHall == other.LectureHall &&
                   Lecturer == other.Lecturer &&
                   Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TimeStart, TimeEnd, Subject, LectureHall, Lecturer, Type);
        }

        public static bool operator ==(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            if (lecture2 is null)
                return lecture1 is null;
            else if (lecture2 is null
                || lecture1.TimeStart != lecture2.TimeStart
                || lecture1.TimeEnd != lecture2.TimeEnd
                || lecture1.Subject != lecture2.Subject
                || lecture1.LectureHall != lecture2.LectureHall
                || lecture1.Lecturer != lecture2.Lecturer
                || lecture1.Type != lecture2.Type)
                return false;
            else
                return true;
        }

        public static bool operator !=(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            return !(lecture1 == lecture2);
        }
    }
}

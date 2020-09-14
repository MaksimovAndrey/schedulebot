using System;
using System.Text;
using Schedulebot.Parse.Enums;

namespace Schedulebot.Schedule
{
    public class ScheduleLecture : IEquatable<ScheduleLecture>
    {
        // статус (что спарсили?) F1, F2, F3, N0, N2 (F - нашли всё, что есть | N - нашли не всё | int - количество найденных аргументов)
        public ParseStatus Status { get; }

        public string TimeStart { get; }
        public string TimeEnd { get; }

        public string Body { get; }

        public string Subject { get; }
        public string LectureHall { get; }
        public string Lecturer { get; }

        public bool IsLecture { get; }
        public bool IsSeminar { get; }
        public bool IsLab { get; }
        public bool IsRemotely { get; }

        public string ErrorType { get; }

        public ScheduleLecture(
            ParseStatus status = default(ParseStatus),

            string timeStart = "",
            string timeEnd = "",

            string body = null,

            string subject = null,
            string lectureHall = null,
            string lecturer = null,

            bool isLecture = false,
            bool isSeminar = false,
            bool isLab = false,
            bool isRemotely = false,

            string errorType = null
        )
        {
            Status = status;

            TimeStart = timeStart;
            TimeEnd = timeEnd;

            Body = body;

            Subject = subject;
            LectureHall = lectureHall;
            Lecturer = lecturer;

            IsLecture = isLecture;
            IsSeminar = isSeminar;
            IsLab = isLab;

            IsRemotely = isRemotely;

            ErrorType = errorType;
        }

        // Собираем лекцию полностью с временем
        public string ToString(bool withTime = true, bool withSubject = true)
        {
            StringBuilder resultBuilder = new StringBuilder();

            if (withTime)
            {
                StringBuilder timeBuilder = new StringBuilder();
                if (string.IsNullOrEmpty(TimeEnd))
                {
                    if (!string.IsNullOrEmpty(TimeStart))
                    {
                        timeBuilder.Append(TimeStart);
                        timeBuilder.Append(' ');
                    }
                }
                else
                {
                    timeBuilder.Append(TimeStart);
                    timeBuilder.Append('-');
                    timeBuilder.Append(TimeEnd);
                    timeBuilder.Append(' ');
                }
                resultBuilder.Append(timeBuilder.ToString());
            }

            StringBuilder lectureBuilder = new StringBuilder();
            if (Status == ParseStatus.N0)
            {
                lectureBuilder.Append(Body);
            }
            else
            {
                if (withSubject)
                    lectureBuilder.Append(Subject);
                if (Lecturer != null)
                {
                    if (lectureBuilder.Length != 0)
                        lectureBuilder.Append(ScheduleBot.delimiter);
                    lectureBuilder.Append(Lecturer);
                }
                if (LectureHall != null)
                {
                    if (lectureBuilder.Length != 0)
                        lectureBuilder.Append(ScheduleBot.delimiter);
                    lectureBuilder.Append(LectureHall);
                }
            }
            if (lectureBuilder.Length == 0)
                lectureBuilder.Append("Ошибка");

            resultBuilder.Append(lectureBuilder.ToString());

            if (IsRemotely)
            {
                resultBuilder.Append(ScheduleBot.delimiter);
                resultBuilder.Append(ScheduleBot.remotelySign);
            }
            if (IsLecture)
            {
                resultBuilder.Append(ScheduleBot.delimiter);
                resultBuilder.Append(ScheduleBot.lectureSign);
            }
            if (IsLab)
            {
                resultBuilder.Append(ScheduleBot.delimiter);
                resultBuilder.Append(ScheduleBot.labSign);
            }
            if (IsSeminar)
            {
                resultBuilder.Append(ScheduleBot.delimiter);
                resultBuilder.Append(ScheduleBot.seminarSign);
            }

            //lectureBuilder.Append(ErrorType);
            return resultBuilder.ToString();
        }

        public ScheduleLecture GetLectureWithOnlySubject()
        {
            if (string.IsNullOrEmpty(Subject))
                return new ScheduleLecture();
            return new ScheduleLecture(
                status: ParseStatus.F1,
                subject: Subject
            );
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleLecture lecture
                && Status == lecture.Status
                && TimeStart == lecture.TimeStart
                && TimeEnd == lecture.TimeEnd
                && Body == lecture.Body
                && Subject == lecture.Subject
                && LectureHall == lecture.LectureHall
                && Lecturer == lecture.Lecturer
                && IsLecture == lecture.IsLecture
                && IsSeminar == lecture.IsSeminar
                && IsRemotely == lecture.IsRemotely
                && ErrorType == lecture.ErrorType;
        }

        public bool Equals(ScheduleLecture other)
        {
            return other != null &&
                   Status == other.Status &&
                   TimeStart == other.TimeStart &&
                   TimeEnd == other.TimeEnd &&
                   Body == other.Body &&
                   Subject == other.Subject &&
                   LectureHall == other.LectureHall &&
                   Lecturer == other.Lecturer &&
                   IsLecture == other.IsLecture &&
                   IsSeminar == other.IsSeminar &&
                   IsLab == other.IsLab &&
                   IsRemotely == other.IsRemotely &&
                   ErrorType == other.ErrorType;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Status);
            hash.Add(TimeStart);
            hash.Add(TimeEnd);
            hash.Add(Body);
            hash.Add(Subject);
            hash.Add(LectureHall);
            hash.Add(Lecturer);
            hash.Add(IsLecture);
            hash.Add(IsSeminar);
            hash.Add(IsLab);
            hash.Add(IsRemotely);
            hash.Add(ErrorType);
            return hash.ToHashCode();
        }

        public static bool operator ==(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            if (lecture2 is null)
                return lecture1 is null;
            else if (lecture2 is null
                || lecture1.Status != lecture2.Status
                || lecture1.TimeStart != lecture2.TimeStart
                || lecture1.TimeEnd != lecture2.TimeEnd
                || lecture1.Body != lecture2.Body
                || lecture1.Subject != lecture2.Subject
                || lecture1.LectureHall != lecture2.LectureHall
                || lecture1.Lecturer != lecture2.Lecturer
                || lecture1.IsLecture != lecture2.IsLecture
                || lecture1.IsSeminar != lecture2.IsSeminar
                || lecture1.IsRemotely != lecture2.IsRemotely
                || lecture1.ErrorType != lecture2.ErrorType)
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
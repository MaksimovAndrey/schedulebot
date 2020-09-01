using System;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleLecture
    {
        // статус (что спарсили?) F1, F2, F3, N0, N2 (F - нашли всё, что есть | N - нашли не всё | int - количество найденных аргументов)
        public string Status { get; }
        public string Subject { get; }
        public string LectureHall { get; }
        public string Lecturer { get; }
        // true - лекция, false - практика
        public bool IsLecture { get; }
        
        public bool IsRemotely { get; }
        public string ErrorType { get; }

        public ScheduleLecture(
            string status = null,
            string subject = null,
            string lectureHall = null,
            string lecturer = null,
            bool isLecture = false,
            bool isRemotely = false,
            string errorType = null
        )
        {
            Status = status;
            Subject = subject;
            LectureHall = lectureHall;
            Lecturer = lecturer;
            IsLecture = isLecture;
            IsRemotely = isRemotely;
            ErrorType = errorType;
        }
        
        public bool IsEmpty()
        {
            return Status == null ? true : false;
        }
        
        public string ConstructLecture() // собираем лекцию полностью
        {
            if (IsEmpty())
                return "";
            StringBuilder lectureBuilder = new StringBuilder();
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
            if (lectureBuilder.Length == 0)
                lectureBuilder.Append("Error");
            
            if (IsRemotely)
            {
                lectureBuilder.Append(ScheduleBot.delimiter);
                lectureBuilder.Append(ScheduleBot.remotelySign);
            }
            if (IsLecture)
            {
                lectureBuilder.Append(ScheduleBot.delimiter);
                lectureBuilder.Append(ScheduleBot.lectureSign);
            }
            lectureBuilder.Append(ErrorType);
            return lectureBuilder.ToString();
        }
        
        public string ConstructLectureWithoutSubject() // собираем лекцию без предмета
        {
            if (IsEmpty())
                return "";
            StringBuilder lectureWithoutSubjectBuilder = new StringBuilder();
            lectureWithoutSubjectBuilder.Append(Lecturer);
            if (LectureHall != null)
            {
                if (lectureWithoutSubjectBuilder.Length != 0)
                    lectureWithoutSubjectBuilder.Append(ScheduleBot.delimiter);
                lectureWithoutSubjectBuilder.Append(LectureHall);
            }
            if (IsRemotely)
            {
                lectureWithoutSubjectBuilder.Append(ScheduleBot.delimiter);
                lectureWithoutSubjectBuilder.Append(ScheduleBot.remotelySign);
            }
            if (IsLecture)
            {
                lectureWithoutSubjectBuilder.Append(ScheduleBot.delimiter);
                lectureWithoutSubjectBuilder.Append(ScheduleBot.lectureSign);
            }
            lectureWithoutSubjectBuilder.Append(ErrorType);
            return lectureWithoutSubjectBuilder.ToString();
        }
        
        public ScheduleLecture GetLectureWithOnlySubject()
        {
            if (IsEmpty())
                return new ScheduleLecture();
            return new ScheduleLecture(
                status: "F1",
                subject: Subject
            );
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleLecture lecture
                && Status == lecture.Status
                && Subject == lecture.Subject
                && LectureHall == lecture.LectureHall
                && Lecturer == lecture.Lecturer
                && IsLecture == lecture.IsLecture
                && IsRemotely == lecture.IsRemotely
                && ErrorType == lecture.ErrorType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Status, Subject, LectureHall, Lecturer, IsLecture, IsRemotely, ErrorType);
        }

        public static bool operator ==(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            if (lecture1.IsLecture != lecture2.IsLecture
                || lecture1.Status != lecture2.Status
                || lecture1.Subject != lecture2.Subject
                || lecture1.LectureHall != lecture2.LectureHall
                || lecture1.Lecturer != lecture2.Lecturer
                || lecture1.ErrorType != lecture2.ErrorType)
                return false;
            return true;
        }
        
        public static bool operator !=(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            return !(lecture1 == lecture2);
        }
    }
}
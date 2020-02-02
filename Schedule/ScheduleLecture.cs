using System.Text.RegularExpressions;
using System.Text;

using Schedulebot.Parse;
using System;

namespace Schedulebot.Schedule
{
    public class ScheduleLecture
    {
        public const string delimiter = " · ";
        public string status = null; // статус (что спарсили?) F1, F2, F3, N0, N2 (F - нашли всё, что есть | N - нашли не всё | int - количество найденных аргументов)
        public string subject = null;
        public string lectureHall = null;
        public string lecturer = null;
        public bool isLecture = false; // true - лекция, false - практика
        public string errorType = null;

        public ScheduleLecture() { }
        
        public ScheduleLecture(string _status, string _subject)
        {
            status = _status;
            subject = _subject;
        }
        
        public bool IsEmpty()
        {
            if (status == null)
                return true;
            return false;
        }
        
        public string ConstructLecture() // собираем лекцию полностью
        {
            StringBuilder lectureBuilder = new StringBuilder();
            lectureBuilder.Append(subject);
            if (lecturer != null)
            {
                if (lectureBuilder.Length != 0)
                    lectureBuilder.Append(delimiter);
                lectureBuilder.Append(lecturer);
            }
            if (lectureHall != null)
            {
                if (lectureBuilder.Length != 0)
                    lectureBuilder.Append(delimiter);
                lectureBuilder.Append(lectureHall);
            }
            if (lectureBuilder.Length == 0)
                lectureBuilder.Append("Error");
            if (isLecture)
            {
                lectureBuilder.Append(delimiter);
                lectureBuilder.Append('Л');
            }
            lectureBuilder.Append(errorType);
            return lectureBuilder.ToString();
        }
        
        public string ConstructLectureWithoutSubject() // собираем лекцию без предмета
        {
            StringBuilder lectureWithoutSubjectBuilder = new StringBuilder();
            lectureWithoutSubjectBuilder.Append(lecturer);
            if (lectureHall != null)
            {
                if (lectureWithoutSubjectBuilder.Length != 0)
                    lectureWithoutSubjectBuilder.Append(delimiter);
                lectureWithoutSubjectBuilder.Append(lectureHall);
            }
            if (isLecture)
            {
                lectureWithoutSubjectBuilder.Append(delimiter);
                lectureWithoutSubjectBuilder.Append('Л');
            }
            lectureWithoutSubjectBuilder.Append(errorType);
            return lectureWithoutSubjectBuilder.ToString();
        }
        
        public ScheduleLecture GetLectureWithOnlySubject()
        {
            return new ScheduleLecture("F1", subject);
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleLecture lecture &&
                   status == lecture.status &&
                   subject == lecture.subject &&
                   lectureHall == lecture.lectureHall &&
                   lecturer == lecture.lecturer &&
                   isLecture == lecture.isLecture &&
                   errorType == lecture.errorType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(status, subject, lectureHall, lecturer, isLecture, errorType);
        }

        public static bool operator ==(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            if (lecture1.isLecture != lecture2.isLecture
                || lecture1.status != lecture2.status
                || lecture1.subject != lecture2.subject
                || lecture1.lectureHall != lecture2.lectureHall
                || lecture1.lecturer != lecture2.lecturer
                || lecture1.errorType != lecture2.errorType)
                return false;
            return true;
        }
        
        public static bool operator !=(ScheduleLecture lecture1, ScheduleLecture lecture2)
        {
            if (lecture1.isLecture != lecture2.isLecture
                || lecture1.status != lecture2.status
                || lecture1.subject != lecture2.subject
                || lecture1.lectureHall != lecture2.lectureHall
                || lecture1.lecturer != lecture2.lecturer
                || lecture1.errorType != lecture2.errorType)
                return true;
            return false;
        }
    }
}
using System.Text.RegularExpressions;
using System.Text;

using Schedulebot.Parse;

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
        
        public ScheduleLecture(string parse)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (parse.Contains(Parsing.errors[i]))
                {
                    errorType += Parsing.errors[i];
                    parse = parse.Replace(Parsing.errors[i], "");
                }
            }
            if (parse.Contains(Parsing.lectureConst))
            {
                isLecture = true;
                parse = parse.Replace(Parsing.lectureConst, "");
            }
            Parse(parse);
            if (subject == "Физическая культура")
                isLecture = false;
        }
        
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
        
        public void Parse(string parsing) // Разбор на фио, аудиоторию и предмет
        {
            if (parsing.Trim() == "")
                return;
            Regex regexLectureHall = new Regex("[0-9]+([/]{1,2}[0-9]+)?( ?[(]{1}[0-9]+[)]{1})?( {1}[(]{1}[0-9]+ {1}корпус[)]{1})?");
            Regex regexFullName = new Regex("[А-Я]{1}[а-я]+([-]{1}[А-Я]{1}[а-я]+)? {1}[А-Я]{1}[.]{1}([А-Я]{1}[.]?)?");
            MatchCollection matches;
            // Чистим строку
            while (parsing.Contains("  "))
                parsing = parsing.Replace("  ", " ");
            // Ищем ФИО
            matches = regexFullName.Matches(parsing);
            if (matches.Count == 1)
            {
                lecturer = matches[0].ToString();
                parsing = parsing.Remove(matches[0].Index, matches[0].Length);
                while (parsing.Contains("  "))
                    parsing = parsing.Replace("  ", " ");
                parsing = parsing.Trim();
            }
            else if (matches.Count == 0)
            {
                int indexTemp;
                for (int i = 0; i < Glob.fullName.Count; ++i)
                {
                    indexTemp = parsing.IndexOf(Glob.fullName[i]);
                    if (indexTemp != -1)
                    {
                        lecturer = Glob.fullName[i];
                        parsing = parsing.Remove(indexTemp, Glob.fullName[i].Length);
                        while (parsing.Contains("  "))
                            parsing = parsing.Replace("  ", " ");
                        parsing = parsing.Trim();
                        break;
                    }
                }
            }
            else if (matches.Count >= 2)
            {
                if (Glob.doubleOptionallySubject.ContainsKey(parsing))
                {
                    status = "F2";
                    subject = Glob.doubleOptionallySubject[parsing];
                    return;
                }
            }
            // Ищем аудиторию
            matches = regexLectureHall.Matches(parsing);
            if (matches.Count != 0)
            {
                if (matches.Count == 1)
                {
                    lectureHall = matches[0].ToString();
                    parsing = parsing.Remove(matches[0].Index, matches[0].Length);
                    while (parsing.Contains("  "))
                        parsing = parsing.Replace("  ", " ");
                    parsing = parsing.Trim();
                }
                else
                {
                    for (int k = 0; k < matches.Count; ++k)
                    {
                        if (matches[k].Index != 0)
                        {
                            if (parsing[matches[k].Index - 1] != ' ')
                                continue;
                        }
                        if (matches[k].Index + matches[k].Length != parsing.Length)
                        {
                            if (parsing[matches[k].Index + matches[k].Length] != ' ' && parsing[matches[k].Index + matches[k].Length] != ',')
                                continue;
                        }
                        lectureHall = matches[k].ToString();
                        parsing = parsing.Remove(matches[k].Index, matches[k].Length);
                        while (parsing.Contains("  "))
                            parsing = parsing.Replace("  ", " ");
                        parsing = parsing.Trim();
                        break;
                    }
                }
            }
            // Выводы: F - полное, N - неполное, n - количество аргументов
            if (lectureHall == null)
            {
                if (parsing.ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА"))
                {
                    status = "F1";
                    subject = "Военная подготовка";
                    return;
                }
                else if (parsing.ToUpper().Contains("ФИЗИЧЕСКАЯ КУЛЬТУРА"))
                {
                    status = "F1";
                    subject = "Физическая культура";
                    return;
                }
                else if (lecturer != null)
                {
                    if (parsing.Contains("по выбору") || parsing.Contains("согласно"))
                    {
                        parsing = char.ToUpper(parsing[0]) + parsing.Substring(1).ToLower();
                    }
                    else
                    {
                        // Если все капсом и (более одного слова или больше 4 знаков), заглавной остается только первая буква
                        if ((parsing.Contains(' ') || parsing.Length > 4))
                        {
                            for (int k = 0; k < parsing.Length; ++k)
                            {
                                if (parsing[k] != char.ToUpper(parsing[k]))
                                    break;
                                if (k == parsing.Length - 1)
                                    parsing = char.ToUpper(parsing[0]) + parsing.Substring(1).ToLower();
                            }
                        }
                    }
                    status = "N2";
                    subject = parsing;
                    return;
                }
                else
                {
                    status = "N0";
                    subject = parsing;
                    return;
                }
            }
            else if (lecturer != null)
            {
                subject = parsing;
                if (Glob.acronymToPhrase.ContainsKey(subject))
                {
                    subject = Glob.acronymToPhrase[subject];
                }
                else if (parsing.Contains(' ') || parsing.Length > 4) // Если все капсом и более одного слова, заглавной остается только первая буква
                {
                    for (int k = 0; k < parsing.Length; ++k)
                    {
                        if (parsing[k] != char.ToUpper(parsing[k]))
                            break;
                        if (k == parsing.Length - 1)
                            subject = char.ToUpper(parsing[0]) + parsing.Substring(1).ToLower();
                    }
                }
                if (parsing.Contains("по выбору") || parsing.Contains("согласно"))
                    subject = char.ToUpper(parsing[0]) + parsing.Substring(1).ToLower();
                status = "F3";
                return;
            }
            else
            {
                if (parsing.Contains("по выбору") || parsing.Contains("согласно"))
                {
                    parsing = char.ToUpper(parsing[0]) + parsing.Substring(1).ToLower();
                }
                else
                {
                    // Если все капсом и (более одного слова или больше 4 знаков), заглавной остается только первая буква
                    if ((parsing.Contains(' ') || parsing.Length > 4))
                    {
                        for (int k = 0; k < parsing.Length; ++k)
                        {
                            if (parsing[k] != char.ToUpper(parsing[k]))
                                break;
                            if (k == parsing.Length - 1)
                                parsing = char.ToUpper(parsing[0]) + parsing.Substring(1).ToLower();
                        }
                    }
                }
                status = "N2";
                subject = parsing;
                return;
            }
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
using System;
using GemBox.Spreadsheet;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

using Schedulebot.Schedule;

namespace Schedulebot.Parse
{
    public static class Parsing
    {
        public static readonly string[] errors = { "¹", "²", "³" };
        public const string lectureConst = "+Л+";

        public static ScheduleLecture ParseLecture(string parse, Dictionaries dictionaries)
        {
            ScheduleLecture lecture = new ScheduleLecture();
            for (int i = 0; i < Parsing.errors.GetLength(0); ++i)
            {
                if (parse.Contains(Parsing.errors[i]))
                {
                    lecture.errorType += Parsing.errors[i];
                    parse = parse.Replace(Parsing.errors[i], "");
                }
            }
            if (parse.Contains(Parsing.lectureConst))
            {
                lecture.isLecture = true;
                parse = parse.Replace(Parsing.lectureConst, "");
            }
            if (parse.Trim() == "")
            {
                return lecture;
            }
            Regex regexLectureHall = new Regex("[0-9]+([/]{1,2}[0-9]+)?( ?[(]{1}[0-9]+[)]{1})?( {1}[(]{1}[0-9]+ {1}корпус[)]{1})?");
            Regex regexFullName = new Regex("[А-Я]{1}[а-я]+([-]{1}[А-Я]{1}[а-я]+)? {1}[А-Я]{1}[.]{1}([А-Я]{1}[.]?)?");
            MatchCollection matches;
            // Чистим строку
            while (parse.Contains("  "))
                parse = parse.Replace("  ", " ");
            // Ищем ФИО
            matches = regexFullName.Matches(parse);
            if (matches.Count == 1)
            {
                lecture.lecturer = matches[0].ToString();
                parse = parse.Remove(matches[0].Index, matches[0].Length);
                while (parse.Contains("  "))
                    parse = parse.Replace("  ", " ");
                parse = parse.Trim();
            }
            else if (matches.Count == 0)
            {
                int indexTemp;
                for (int i = 0; i < dictionaries.fullName.Count; ++i)
                {
                    indexTemp = parse.IndexOf(dictionaries.fullName[i]);
                    if (indexTemp != -1)
                    {
                        lecture.lecturer = dictionaries.fullName[i];
                        parse = parse.Remove(indexTemp, dictionaries.fullName[i].Length);
                        while (parse.Contains("  "))
                            parse = parse.Replace("  ", " ");
                        parse = parse.Trim();
                        break;
                    }
                }
            }
            else if (matches.Count >= 2)
            {
                if (dictionaries.doubleOptionallySubject.ContainsKey(parse))
                {
                    lecture.status = "F2";
                    lecture.subject = dictionaries.doubleOptionallySubject[parse];
                    return lecture;
                }
            }
            // Ищем аудиторию
            matches = regexLectureHall.Matches(parse);
            if (matches.Count != 0)
            {
                if (matches.Count == 1)
                {
                    lecture.lectureHall = matches[0].ToString();
                    parse = parse.Remove(matches[0].Index, matches[0].Length);
                    while (parse.Contains("  "))
                        parse = parse.Replace("  ", " ");
                    parse = parse.Trim();
                }
                else
                {
                    for (int k = 0; k < matches.Count; ++k)
                    {
                        if (matches[k].Index != 0)
                        {
                            if (parse[matches[k].Index - 1] != ' ')
                                continue;
                        }
                        if (matches[k].Index + matches[k].Length != parse.Length)
                        {
                            if (parse[matches[k].Index + matches[k].Length] != ' ' && parse[matches[k].Index + matches[k].Length] != ',')
                                continue;
                        }
                        lecture.lectureHall = matches[k].ToString();
                        parse = parse.Remove(matches[k].Index, matches[k].Length);
                        while (parse.Contains("  "))
                            parse = parse.Replace("  ", " ");
                        parse = parse.Trim();
                        break;
                    }
                }
            }
            // Выводы: F - полное, N - неполное, n - количество аргументов
            if (lecture.lectureHall == null)
            {
                if (parse.ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА"))
                {
                    lecture.status = "F1";
                    lecture.subject = "Военная подготовка";
                    lecture.isLecture = false;
                    return lecture;
                }
                else if (parse.ToUpper().Contains("ФИЗИЧЕСКАЯ КУЛЬТУРА"))
                {
                    lecture.status = "F1";
                    lecture.subject = "Физическая культура";
                    lecture.isLecture = false;
                    return lecture;
                }
                else if (lecture.lecturer != null)
                {
                    if (parse.Contains("по выбору") || parse.Contains("согласно"))
                    {
                        parse = char.ToUpper(parse[0]) + parse.Substring(1).ToLower();
                    }
                    else
                    {
                        // Если все капсом и (более одного слова или больше 4 знаков), заглавной остается только первая буква
                        if ((parse.Contains(' ') || parse.Length > 4))
                        {
                            for (int k = 0; k < parse.Length; ++k)
                            {
                                if (parse[k] != char.ToUpper(parse[k]))
                                    break;
                                if (k == parse.Length - 1)
                                    parse = char.ToUpper(parse[0]) + parse.Substring(1).ToLower();
                            }
                        }
                    }
                    lecture.status = "N2";
                    lecture.subject = parse;
                    return lecture;
                }
                else
                {
                    lecture.status = "N0";
                    lecture.subject = parse;
                    return lecture;
                }
            }
            else if (lecture.lecturer != null)
            {
                lecture.subject = parse;
                if (dictionaries.acronymToPhrase.ContainsKey(lecture.subject))
                {
                    lecture.subject = dictionaries.acronymToPhrase[lecture.subject];
                }
                else if (parse.Contains(' ') || parse.Length > 4) // Если все капсом и более одного слова, заглавной остается только первая буква
                {
                    for (int k = 0; k < parse.Length; ++k)
                    {
                        if (parse[k] != char.ToUpper(parse[k]))
                            break;
                        if (k == parse.Length - 1)
                            lecture.subject = char.ToUpper(parse[0]) + parse.Substring(1).ToLower();
                    }
                }
                if (parse.Contains("по выбору") || parse.Contains("согласно"))
                    lecture.subject = char.ToUpper(parse[0]) + parse.Substring(1).ToLower();
                lecture.status = "F3";
                return lecture;
            }
            else
            {
                if (parse.Contains("по выбору") || parse.Contains("согласно"))
                {
                    parse = char.ToUpper(parse[0]) + parse.Substring(1).ToLower();
                }
                else
                {
                    // Если все капсом и (более одного слова или больше 4 знаков), заглавной остается только первая буква
                    if ((parse.Contains(' ') || parse.Length > 4))
                    {
                        for (int k = 0; k < parse.Length; ++k)
                        {
                            if (parse[k] != char.ToUpper(parse[k]))
                                break;
                            if (k == parse.Length - 1)
                                parse = char.ToUpper(parse[0]) + parse.Substring(1).ToLower();
                        }
                    }
                }
                lecture.status = "N2";
                lecture.subject = parse;
                return lecture;
            }
        }
        
        public static List<Group> Mapper(string pathToFile, Dictionaries dictionaries)
        {
            string format = pathToFile.Substring(pathToFile.LastIndexOf('.') + 1);
            string[,] schedule = null;
            switch (format)
            {
                case "xls":
                {
                    try 
                    {
                        ExcelFile scheduleSource = ExcelFile.Load(pathToFile);   // Открытие Excel file
                        ExcelWorksheet worksheet = scheduleSource.Worksheets.ActiveWorksheet; // Выбор листа (worksheet)
                        schedule = ParseXls(worksheet);
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                }
            }
            int groupsAmount = schedule.GetLength(0);
            // Проверяем группы на наличие одинаковых
            List<string> groupsNames = new List<string>();
            List<int> uniqueGroups = new List<int>();
            for (int currentGroup = 0; currentGroup < groupsAmount; currentGroup += 2)
            {
                if (!groupsNames.Contains(schedule[currentGroup, 0]))
                {
                    groupsNames.Add(schedule[currentGroup, 0]);
                    uniqueGroups.Add(currentGroup / 2);
                }
                else
                {
                    int index = groupsNames.IndexOf(schedule[currentGroup, 0]);
                    int count = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 2; j < 98; j++)
                        {
                            if (schedule[index + i, j] != "")
                                ++count;
                        }
                    }
                    int count2 = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 2; j < 98; j++)
                        {
                            if (schedule[currentGroup + i, j] != "")
                                ++count;
                        }
                    }
                    if (count < count2)
                    {
                        uniqueGroups[index] = currentGroup / 2;
                    }
                }
            }
            // Собираем группы
            List<Group> groups = new List<Group>();
            for (int i = 0; i < uniqueGroups.Count; ++i)
            {
                groups.Add(new Group());
                groups[i].name = schedule[uniqueGroups[i] * 2, 0];
                for (int currentSubgroup = 0; currentSubgroup < 2; ++currentSubgroup)
                {
                    for (int currentWeek = 0; currentWeek < 2; ++currentWeek)
                    {
                        for (int currentDay = 0; currentDay < 6; ++currentDay)
                        {
                            for (int currentLecture = 0; currentLecture < 8; ++currentLecture)
                            {
                                groups[i].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture]
                                    = ParseLecture(schedule[uniqueGroups[i] * 2 + currentSubgroup, 2 + currentDay * 16 + currentLecture * 2 + currentWeek], dictionaries);
                            }
                            groups[i].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].isStudying
                                = !groups[i].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].IsEmpty();
                        }
                    }
                }
            }
            return groups;
        }
        
        public static async Task<List<Group>> MapperAsync(string pathToFile, Dictionaries dictionaries)
        {
            return await Task.Run(async () => 
            {
                string format = pathToFile.Substring(pathToFile.LastIndexOf('.') + 1);
                string[,] schedule = null;
                switch (format)
                {
                    case "xls":
                    {
                        try 
                        {
                            ExcelFile scheduleSource = ExcelFile.Load(pathToFile);   // Открытие Excel file
                            ExcelWorksheet worksheet = scheduleSource.Worksheets.ActiveWorksheet; // Выбор листа (worksheet)
                            schedule = await ParseXlsAsync(worksheet);
                        }
                        catch
                        {
                            return null;
                        }
                        break;
                    }
                }
                int groupsAmount = schedule.GetLength(0);
                // Проверяем группы на наличие одинаковых
                List<string> groupsNames = new List<string>();
                List<int> uniqueGroups = new List<int>();
                for (int currentGroup = 0; currentGroup < groupsAmount; currentGroup += 2)
                {
                    if (!groupsNames.Contains(schedule[currentGroup, 0]))
                    {
                        groupsNames.Add(schedule[currentGroup, 0]);
                        uniqueGroups.Add(currentGroup / 2);
                    }
                    else
                    {
                        int index = groupsNames.IndexOf(schedule[currentGroup, 0]);
                        int count = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 2; j < 98; j++)
                            {
                                if (schedule[index + i, j] != "")
                                    ++count;
                            }
                        }
                        int count2 = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 2; j < 98; j++)
                            {
                                if (schedule[currentGroup + i, j] != "")
                                    ++count;
                            }
                        }
                        if (count < count2)
                        {
                            uniqueGroups[index] = currentGroup / 2;
                        }
                    }
                }
                // Собираем группы
                List<Group> groups = new List<Group>();                
                for (int i = 0; i < uniqueGroups.Count; ++i)
                {
                    groups.Add(new Group());
                    groups[i].name = schedule[uniqueGroups[i] * 2, 0];
                    for (int currentSubgroup = 0; currentSubgroup < 2; ++currentSubgroup)
                    {
                        for (int currentWeek = 0; currentWeek < 2; ++currentWeek)
                        {
                            for (int currentDay = 0; currentDay < 6; ++currentDay)
                            {
                                for (int currentLecture = 0; currentLecture < 8; ++currentLecture)
                                {
                                    groups[i].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture]
                                        = ParseLecture(schedule[uniqueGroups[i] * 2 + currentSubgroup, 2 + currentDay * 16 + currentLecture * 2 + currentWeek], dictionaries);
                                }
                                groups[i].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].isStudying
                                    = !groups[i].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].IsEmpty();
                            }
                        }
                    }
                }
                return groups;
            });
        }
        
        public class CurrentInfo
        {
            public int x;
            public int y;
            public Schedule schedule;
            public struct Schedule
            {
                public int x;
                public int y;
            }

            public CurrentInfo(int _x, int _y)
            {
                x = _x;
                y = _y;
                schedule.x = 0;
                schedule.y = 0;
            }
        }

        public static string[,] ParseXls(ExcelWorksheet worksheet)
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S]    -> Обработка расписания");
            int indent = 2; // отступ от времени (начало ячеек)
            CurrentInfo current = new CurrentInfo(indent, 0);
            // Определяем где группа и начало пар
            int groupNameY = 4; // линия, в которой содержатся имена групп
            for (int i = 0; i < 16; ++i)
            {
                if (worksheet.Cells[groupNameY, current.x].Value != null
                    && (worksheet.Cells[groupNameY, current.x].ValueType == CellValueType.String
                        || worksheet.Cells[groupNameY, current.x].ValueType == CellValueType.Int))
                {
                    if (worksheet.Cells[groupNameY, current.x].StringValue.Trim() != "")
                    {
                        if (worksheet.Cells[groupNameY, current.x].StringValue.Trim().IndexOf("38") == 0)
                            break;
                    }
                }
                ++groupNameY;
            }
            if (groupNameY == 20)
                throw new ArgumentOutOfRangeException("groupNameY");
            // Определяем где начало расписания
            int scheduleStartY = 1;
            while (true)
            {
                if (worksheet.Cells[scheduleStartY, 1].Value != null)
                    if (worksheet.Cells[scheduleStartY, 1].ValueType == CellValueType.DateTime)
                        if (((DateTime)worksheet.Cells[scheduleStartY, 1].Value).Hour == 7
                            && ((DateTime)worksheet.Cells[scheduleStartY, 1].Value).Minute == 30)
                            break;
                scheduleStartY++;
            }
            // Считаем сколько групп
            int countOfGroups = 0;
            while (worksheet.Cells[groupNameY, current.x].Value != null)
            {
                ++countOfGroups;
                current.x += 2;
            }
            string[,] schedule = new string[countOfGroups * 2, 98];
            current.x = indent;
            while (worksheet.Cells[groupNameY, current.x].Value != null)
            {
                for (current.y = scheduleStartY; current.y < scheduleStartY + 96; current.y += 2)
                {
                    // Отмена объединения ячеек
                    for (int j = 0; j < 2; ++j)
                    {
                        for (int k = 0; k < 2; ++k)
                        {
                            if (worksheet.Cells[current.y, current.x].Value != null)
                            {
                                if (!worksheet.Cells[current.y, current.x].Value.ToString().Trim().ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА"))
                                {
                                    CellRange mergedRange = worksheet.Cells[current.y + j, current.x + k].MergedRange;
                                    if (mergedRange != null)
                                    {
                                        var fillPattern = worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Style.FillPattern;
                                        worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Merged = false;
                                        worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Style.FillPattern = fillPattern;
                                    }
                                }
                            }
                            else
                            {
                                CellRange mergedRange = worksheet.Cells[current.y + j, current.x + k].MergedRange;
                                if (mergedRange != null)
                                {
                                    var fillPattern = worksheet.Cells.GetSubrangeAbsolute(
                                        mergedRange.FirstRowIndex,
                                        mergedRange.FirstColumnIndex,
                                        mergedRange.LastRowIndex,
                                        mergedRange.LastColumnIndex).Style.FillPattern;
                                    worksheet.Cells.GetSubrangeAbsolute(
                                        mergedRange.FirstRowIndex,
                                        mergedRange.FirstColumnIndex,
                                        mergedRange.LastRowIndex,
                                        mergedRange.LastColumnIndex).Merged = false;
                                    worksheet.Cells.GetSubrangeAbsolute(
                                        mergedRange.FirstRowIndex,
                                        mergedRange.FirstColumnIndex,
                                        mergedRange.LastRowIndex,
                                        mergedRange.LastColumnIndex).Style.FillPattern = fillPattern;
                                }
                            }
                        }
                    }
                }
                current.x += 2;
            }
            current.x = indent;
            // Проход по всем группам
            while (worksheet.Cells[groupNameY, current.x].Value != null)
            {
                current.schedule.y = 2;
                // записываем имя группы
                schedule[current.schedule.x, 0] = worksheet.Cells[groupNameY, current.x].StringValue.Trim();
                schedule[current.schedule.x + 1, 0] = schedule[current.schedule.x, 0];
                // записываем подгруппу
                schedule[current.schedule.x, 1] = "1";
                schedule[current.schedule.x + 1, 1] = "2";
                // Проход по ячейкам
                for (current.y = scheduleStartY; current.y < scheduleStartY + 96; current.y += 2)
                {
                    // Уже заполнена
                    if (schedule[current.schedule.x, current.schedule.y] != null
                        && schedule[current.schedule.x, current.schedule.y + 1] != null
                        && schedule[current.schedule.x + 1, current.schedule.y] != null
                        && schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                    {
                        current.schedule.y += 2;
                        continue; // переход к следующей группе ячеек
                    }
                    // Пустая ячейка
                    else if (worksheet.Cells[current.y, current.x].Value == null
                        && worksheet.Cells[current.y, current.x + 1].Value == null
                        && worksheet.Cells[current.y + 1, current.x].Value == null
                        && worksheet.Cells[current.y + 1, current.x + 1].Value == null
                        && worksheet.Cells[current.y, current.x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[current.y, current.x + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[current.y + 1, current.x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[current.y + 1, current.x + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && schedule[current.schedule.x, current.schedule.y] == null
                        && schedule[current.schedule.x, current.schedule.y + 1] == null
                        && schedule[current.schedule.x + 1, current.schedule.y] == null
                        && schedule[current.schedule.x + 1, current.schedule.y + 1] == null)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                schedule[current.schedule.x + i, current.schedule.y + j] = "";
                            }
                        }
                        current.schedule.y += 2;
                        continue; // переход к следующей группе ячеек
                    }
                    // ┏━━━━━━━━━━━━━┓ 
                    //                
                    //                
                    //               
                    // ┗━━━━━━━━━━━━━┛
                    else if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None
                        && worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None
                        && worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None
                        && worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                    {
                        // 0
                        if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None
                            && worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None
                            && worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None
                            && worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                        {
                            // 0-0
                            if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                            {
                                // 0-0-0
                                if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                {
                                    // 0-0-0-0
                                    if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                    {
                                        // 0-0-0-0-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            for (int i = 0; i < 2; i++)
                                                for (int j = 0; j < 2; j++)
                                                    Cell1x1(ref worksheet, ref schedule, current, i, j);
                                        }
                                        // 0-0-0-0-1
                                        else
                                        {
                                            for (int i = 0; i < 2; i++)
                                                Cell1x1(ref worksheet, ref schedule, current, i, 0);
                                            Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                        }
                                    }
                                    // 0-0-0-1
                                    else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        for (int i = 0; i < 2; i++)
                                            Cell1x1(ref worksheet, ref schedule, current, 0, i);
                                        Cell1x2(ref worksheet, ref schedule, current, 1);
                                    }
                                    // 0-0-0-2
                                    else
                                    {
                                        Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                        CellTandMore(ref worksheet, ref schedule, current, 1, 1);
                                    }
                                }
                                // 0-0-1
                                else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                {
                                    // 0-0-1-0
                                    if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                        for (int i = 0; i < 2; i++)
                                            Cell1x1(ref worksheet, ref schedule, current, i, 1);
                                    }
                                    // 0-0-1-1
                                    else
                                    {
                                        for (int i = 0; i < 2; i++)
                                            Cell2x1andMore(ref worksheet, ref schedule, current, i);
                                    }
                                }
                                // 0-0-2
                                else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                    Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                }
                                // 0-0-3
                                else
                                {
                                    worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                }
                            }
                            // 0-1
                            else if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                            {
                                // 0-1-0
                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                {
                                    // 0-1-0-0
                                    if (worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        Cell1x2(ref worksheet, ref schedule, current, 0);
                                        for (int i = 0; i < 2; i++)
                                            Cell1x1(ref worksheet, ref schedule, current, 1, i);
                                    }
                                    // 0-1-0-1
                                    else
                                    {
                                        CellTandMore(ref worksheet, ref schedule, current, 0, 1);
                                        Cell1x1(ref worksheet, ref schedule, current, 1, 0);
                                    }
                                }
                                // 0-1-1
                                else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    for (int i = 0; i < 2; i++)
                                        Cell1x2(ref worksheet, ref schedule, current, i);
                                }
                                // 0-1-2
                                else
                                {
                                    worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle = LineStyle.None;
                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                }
                            }
                            // 0-2
                            else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                            {
                                // 0-2-0
                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                    Cell1x1(ref worksheet, ref schedule, current, 1, 1);
                                }
                                // 0-2-1
                                else
                                {
                                    worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                }
                            }
                            // 0-3
                            else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                            {
                                worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle = LineStyle.None;
                                Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                            }
                            // 0-4
                            else
                            {
                                Cell2x2andMore(ref worksheet, ref schedule, current);
                            }
                        }
                        // 1
                        else
                        {
                            // 1-0
                            if (schedule[current.schedule.x, current.schedule.y] != null)
                            {
                                // 1-0-0
                                if (schedule[current.schedule.x, current.schedule.y + 1] != null)
                                {
                                    // 1-0-0-0
                                    if (schedule[current.schedule.x + 1, current.schedule.y] != null)
                                    {
                                        Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                    }
                                    // 1-0-0-1
                                    else if (schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                                    {
                                        Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                    }
                                    // 1-0-0-2
                                    else
                                    {
                                        // 1-0-0-2-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            for (int i = 0; i < 2; i++)
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                        }
                                        // 1-0-0-2-1
                                        else
                                        {
                                            Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                        }
                                    }
                                }
                                // 1-0-1
                                else
                                {
                                    // 1-0-1-0
                                    if (schedule[current.schedule.x + 1, current.schedule.y] != null)
                                    {
                                        // 1-0-1-0-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                            Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                        }
                                        // 1-0-1-0-1
                                        else
                                        {
                                            Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                        }
                                    }
                                    // 1-0-1-1
                                    else
                                    {
                                        // 1-0-1-1-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-0-1-1-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                            }
                                            // 1-0-1-1-0-1
                                            else
                                            {
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                                Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                            }
                                        }
                                        // 1-0-1-1-1
                                        else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                            Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                        }
                                        // 1-0-1-1-2
                                        else
                                        {
                                            CellTandMore(ref worksheet, ref schedule, current, 1, 1);
                                        }
                                    }
                                }
                            }
                            // 1-1
                            else if (schedule[current.schedule.x, current.schedule.y + 1] != null)
                            {
                                // 1-1-0
                                if (schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                                {
                                    // 1-1-0-0
                                    if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                        Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                    }
                                    // 1-1-0-1
                                    else
                                    {
                                        Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                    }
                                }
                                // 1-1-1
                                else
                                {
                                    // 1-1-1-0
                                    if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 1-1-1-0-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                            for (int i = 0; i < 2; i++)
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                        }
                                        // 1-1-1-0-1
                                        else
                                        {
                                            Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                            Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                        }
                                    }
                                    // 1-1-1-1
                                    else
                                    {
                                        // 1-1-1-1-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                            Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                        }
                                        // 1-1-1-1-1
                                        else
                                        {
                                            CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                        }
                                    }
                                }
                            }
                            // 1-2
                            else
                            {
                                // 1-2-0
                                if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                                {
                                    // 1-2-0-0
                                    if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 1-2-0-0-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-0-0-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, i);
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                            }
                                            // 1-2-0-0-0-1
                                            else
                                            {
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                                Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                            }
                                        }
                                        // 1-2-0-0-1
                                        else
                                        {
                                            // 1-2-0-0-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, i);
                                                Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                            }
                                            // 1-2-0-0-1-1
                                            else
                                            {
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                                CellTandMore(ref worksheet, ref schedule, current, 1, 1);
                                            }
                                        }
                                    }
                                    // 1-2-0-1
                                    else
                                    {
                                        // 1-2-0-1-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-0-1-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                            }
                                            // 1-2-0-1-0-1
                                            else
                                            {
                                                if (current.schedule.x == 14 && current.schedule.y == 86)
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                        Cell2x1andMore(ref worksheet, ref schedule, current, i);
                                                }
                                                else
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                        Cell2x1andMore(ref worksheet, ref schedule, current, i);
                                                }
                                            }
                                        }
                                        // 1-2-0-1-1
                                        else
                                        {
                                            // 1-2-0-1-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                            }
                                            // 1-2-0-1-1-1
                                            else
                                            {
                                                worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                                Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                            }
                                        }
                                    }
                                }
                                // 1-2-1
                                else
                                {
                                    // 1-2-1-0
                                    if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 1-2-1-0-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-1-0-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                Cell1x2(ref worksheet, ref schedule, current, 0);
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                            }
                                            // 1-2-1-0-0-1
                                            else
                                            {
                                                CellTandMore(ref worksheet, ref schedule, current, 0, 1);
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                            }
                                        }
                                        // 1-2-1-0-1
                                        else
                                        {
                                            // 1-2-1-0-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                Cell1x2(ref worksheet, ref schedule, current, 0);
                                                Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                            }
                                            // 1-2-1-0-1-1
                                            else
                                            {
                                                worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle = LineStyle.None;
                                                Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                            }
                                        }
                                    }
                                    // 1-2-1-1
                                    else
                                    {
                                        // 1-2-1-1-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-1-1-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                CellTandMore(ref worksheet, ref schedule, current, 0, 0);
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                            }
                                            // 1-2-1-1-0-1
                                            else
                                            {
                                                worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                                Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                            }
                                        }
                                        // 1-2-1-1-1
                                        else
                                        {
                                            // 1-2-1-1-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle = LineStyle.None;
                                                Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                            }
                                            // 1-2-1-1-1-1
                                            else
                                            {
                                                Cell2x2andMore(ref worksheet, ref schedule, current);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // в случае неограниченности сверху или снизу
                    {
                        // todo: изменить это временное решение

                        for (int i = 0; i < 2; ++i)
                        {
                            for (int j = 0; j < 2; ++j)
                                if (worksheet.Cells[current.y + i, current.x + j].Value != null)
                                    if (worksheet.Cells[current.y + i, current.x + j].ValueType == CellValueType.String)
                                        worksheet.Cells[current.y + i, current.x + j].Value = worksheet.Cells[current.y + i, current.x + j].StringValue + '⚠';
                        }

                        worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Top].LineStyle = LineStyle.Thin;
                        worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle = LineStyle.Thin;
                        worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.Thin;
                        worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.Thin;
                        current.schedule.y -= 2;
                        current.y -= 2;
                    }
                    current.schedule.y += 2;
                }
                current.schedule.x += 2;
                current.x += 2; //следующая группа
            }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E]    -> Обработка расписания");
            return schedule;
        }
    
        public static async Task<string[,]> ParseXlsAsync(ExcelWorksheet worksheet)
        {
            //? можно сделать закрытые ячейки 2x2 асинхронными
            return await Task.Run(() => 
            {
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S]    -> Обработка расписания");
                int indent = 2; // отступ от времени (начало ячеек)
                CurrentInfo current = new CurrentInfo(indent, 0);
                // Определяем где группа и начало пар
                int groupNameY = 4; // линия, в которой содержатся имена групп
                for (int i = 0; i < 16; ++i)
                {
                    if (worksheet.Cells[groupNameY, current.x].Value != null
                        && (worksheet.Cells[groupNameY, current.x].ValueType == CellValueType.String
                            || worksheet.Cells[groupNameY, current.x].ValueType == CellValueType.Int))
                    {
                        if (worksheet.Cells[groupNameY, current.x].StringValue.Trim() != "")
                        {
                            if (worksheet.Cells[groupNameY, current.x].StringValue.Trim().IndexOf("38") == 0)
                                break;
                        }
                    }
                    ++groupNameY;
                }
                if (groupNameY == 20)
                    throw new ArgumentOutOfRangeException("groupNameY");
                // Определяем где начало расписания
                int scheduleStartY = 1;
                while (true)
                {
                    if (worksheet.Cells[scheduleStartY, 1].Value != null)
                        if (worksheet.Cells[scheduleStartY, 1].ValueType == CellValueType.DateTime)
                            if (((DateTime)worksheet.Cells[scheduleStartY, 1].Value).Hour == 7
                                && ((DateTime)worksheet.Cells[scheduleStartY, 1].Value).Minute == 30)
                                break;
                    scheduleStartY++;
                }
                // Считаем сколько групп
                int countOfGroups = 0;
                while (worksheet.Cells[groupNameY, current.x].Value != null)
                {
                    ++countOfGroups;
                    current.x += 2;
                }
                string[,] schedule = new string[countOfGroups * 2, 98];
                current.x = indent;
                while (worksheet.Cells[groupNameY, current.x].Value != null)
                {
                    for (current.y = scheduleStartY; current.y < scheduleStartY + 96; current.y += 2)
                    {
                        // Отмена объединения ячеек
                        for (int j = 0; j < 2; ++j)
                        {
                            for (int k = 0; k < 2; ++k)
                            {
                                if (worksheet.Cells[current.y, current.x].Value != null)
                                {
                                    if (!worksheet.Cells[current.y, current.x].Value.ToString().Trim().ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА"))
                                    {
                                        CellRange mergedRange = worksheet.Cells[current.y + j, current.x + k].MergedRange;
                                        if (mergedRange != null)
                                        {
                                            var fillPattern = worksheet.Cells.GetSubrangeAbsolute(
                                                mergedRange.FirstRowIndex,
                                                mergedRange.FirstColumnIndex,
                                                mergedRange.LastRowIndex,
                                                mergedRange.LastColumnIndex).Style.FillPattern;
                                            worksheet.Cells.GetSubrangeAbsolute(
                                                mergedRange.FirstRowIndex,
                                                mergedRange.FirstColumnIndex,
                                                mergedRange.LastRowIndex,
                                                mergedRange.LastColumnIndex).Merged = false;
                                            worksheet.Cells.GetSubrangeAbsolute(
                                                mergedRange.FirstRowIndex,
                                                mergedRange.FirstColumnIndex,
                                                mergedRange.LastRowIndex,
                                                mergedRange.LastColumnIndex).Style.FillPattern = fillPattern;
                                        }
                                    }
                                }
                                else
                                {
                                    CellRange mergedRange = worksheet.Cells[current.y + j, current.x + k].MergedRange;
                                    if (mergedRange != null)
                                    {
                                        var fillPattern = worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Style.FillPattern;
                                        worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Merged = false;
                                        worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Style.FillPattern = fillPattern;
                                    }
                                }
                            }
                        }
                    }
                    current.x += 2;
                }
                current.x = indent;
                // Проход по всем группам
                while (worksheet.Cells[groupNameY, current.x].Value != null)
                {
                    current.schedule.y = 2;
                    // записываем имя группы
                    schedule[current.schedule.x, 0] = worksheet.Cells[groupNameY, current.x].StringValue.Trim();
                    schedule[current.schedule.x + 1, 0] = schedule[current.schedule.x, 0];
                    // записываем подгруппу
                    schedule[current.schedule.x, 1] = "1";
                    schedule[current.schedule.x + 1, 1] = "2";
                    // Проход по ячейкам
                    for (current.y = scheduleStartY; current.y < scheduleStartY + 96; current.y += 2)
                    {
                        // Уже заполнена
                        if (schedule[current.schedule.x, current.schedule.y] != null
                            && schedule[current.schedule.x, current.schedule.y + 1] != null
                            && schedule[current.schedule.x + 1, current.schedule.y] != null
                            && schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                        {
                            current.schedule.y += 2;
                            continue; // переход к следующей группе ячеек
                        }
                        // Пустая ячейка
                        else if (worksheet.Cells[current.y, current.x].Value == null
                            && worksheet.Cells[current.y, current.x + 1].Value == null
                            && worksheet.Cells[current.y + 1, current.x].Value == null
                            && worksheet.Cells[current.y + 1, current.x + 1].Value == null
                            && worksheet.Cells[current.y, current.x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                            && worksheet.Cells[current.y, current.x + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None
                            && worksheet.Cells[current.y + 1, current.x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                            && worksheet.Cells[current.y + 1, current.x + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None
                            && schedule[current.schedule.x, current.schedule.y] == null
                            && schedule[current.schedule.x, current.schedule.y + 1] == null
                            && schedule[current.schedule.x + 1, current.schedule.y] == null
                            && schedule[current.schedule.x + 1, current.schedule.y + 1] == null)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    schedule[current.schedule.x + i, current.schedule.y + j] = "";
                                }
                            }
                            current.schedule.y += 2;
                            continue; // переход к следующей группе ячеек
                        }
                        // ┏━━━━━━━━━━━━━┓ 
                        //                
                        //                
                        //               
                        // ┗━━━━━━━━━━━━━┛
                        else if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None
                            && worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None
                            && worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None
                            && worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                        {
                            // 0
                            if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None
                                && worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None
                                && worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None
                                && worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                            {
                                // 0-0
                                if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                                {
                                    // 0-0-0
                                    if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 0-0-0-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 0-0-0-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    for (int j = 0; j < 2; j++)
                                                        Cell1x1(ref worksheet, ref schedule, current, i, j);
                                            }
                                            // 0-0-0-0-1
                                            else
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1(ref worksheet, ref schedule, current, i, 0);
                                                Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                            }
                                        }
                                        // 0-0-0-1
                                        else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            for (int i = 0; i < 2; i++)
                                                Cell1x1(ref worksheet, ref schedule, current, 0, i);
                                            Cell1x2(ref worksheet, ref schedule, current, 1);
                                        }
                                        // 0-0-0-2
                                        else
                                        {
                                            Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                            CellTandMore(ref worksheet, ref schedule, current, 1, 1);
                                        }
                                    }
                                    // 0-0-1
                                    else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                    {
                                        // 0-0-1-0
                                        if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                            for (int i = 0; i < 2; i++)
                                                Cell1x1(ref worksheet, ref schedule, current, i, 1);
                                        }
                                        // 0-0-1-1
                                        else
                                        {
                                            for (int i = 0; i < 2; i++)
                                                Cell2x1andMore(ref worksheet, ref schedule, current, i);
                                        }
                                    }
                                    // 0-0-2
                                    else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                        Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                    }
                                    // 0-0-3
                                    else
                                    {
                                        worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                        Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                    }
                                }
                                // 0-1
                                else if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                {
                                    // 0-1-0
                                    if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                    {
                                        // 0-1-0-0
                                        if (worksheet.Cells[current.y + 1, current.x].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            Cell1x2(ref worksheet, ref schedule, current, 0);
                                            for (int i = 0; i < 2; i++)
                                                Cell1x1(ref worksheet, ref schedule, current, 1, i);
                                        }
                                        // 0-1-0-1
                                        else
                                        {
                                            CellTandMore(ref worksheet, ref schedule, current, 0, 1);
                                            Cell1x1(ref worksheet, ref schedule, current, 1, 0);
                                        }
                                    }
                                    // 0-1-1
                                    else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        for (int i = 0; i < 2; i++)
                                            Cell1x2(ref worksheet, ref schedule, current, i);
                                    }
                                    // 0-1-2
                                    else
                                    {
                                        worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle = LineStyle.None;
                                        Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                    }
                                }
                                // 0-2
                                else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                {
                                    // 0-2-0
                                    if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                        Cell1x1(ref worksheet, ref schedule, current, 1, 1);
                                    }
                                    // 0-2-1
                                    else
                                    {
                                        worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                        Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                    }
                                }
                                // 0-3
                                else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle = LineStyle.None;
                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                }
                                // 0-4
                                else
                                {
                                    Cell2x2andMore(ref worksheet, ref schedule, current);
                                }
                            }
                            // 1
                            else
                            {
                                // 1-0
                                if (schedule[current.schedule.x, current.schedule.y] != null)
                                {
                                    // 1-0-0
                                    if (schedule[current.schedule.x, current.schedule.y + 1] != null)
                                    {
                                        // 1-0-0-0
                                        if (schedule[current.schedule.x + 1, current.schedule.y] != null)
                                        {
                                            Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                        }
                                        // 1-0-0-1
                                        else if (schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                                        {
                                            Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                        }
                                        // 1-0-0-2
                                        else
                                        {
                                            // 1-0-0-2-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                            }
                                            // 1-0-0-2-1
                                            else
                                            {
                                                Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                            }
                                        }
                                    }
                                    // 1-0-1
                                    else
                                    {
                                        // 1-0-1-0
                                        if (schedule[current.schedule.x + 1, current.schedule.y] != null)
                                        {
                                            // 1-0-1-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                            }
                                            // 1-0-1-0-1
                                            else
                                            {
                                                Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                            }
                                        }
                                        // 1-0-1-1
                                        else
                                        {
                                            // 1-0-1-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                // 1-0-1-1-0-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                        Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                                }
                                                // 1-0-1-1-0-1
                                                else
                                                {
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                                    Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                                }
                                            }
                                            // 1-0-1-1-1
                                            else if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                            }
                                            // 1-0-1-1-2
                                            else
                                            {
                                                CellTandMore(ref worksheet, ref schedule, current, 1, 1);
                                            }
                                        }
                                    }
                                }
                                // 1-1
                                else if (schedule[current.schedule.x, current.schedule.y + 1] != null)
                                {
                                    // 1-1-0
                                    if (schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                                    {
                                        // 1-1-0-0
                                        if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                        {
                                            Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                            Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                        }
                                        // 1-1-0-1
                                        else
                                        {
                                            Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                        }
                                    }
                                    // 1-1-1
                                    else
                                    {
                                        // 1-1-1-0
                                        if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                        {
                                            // 1-1-1-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                                for (int i = 0; i < 2; i++)
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                            }
                                            // 1-1-1-0-1
                                            else
                                            {
                                                Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                                Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                            }
                                        }
                                        // 1-1-1-1
                                        else
                                        {
                                            // 1-1-1-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                                Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                            }
                                            // 1-1-1-1-1
                                            else
                                            {
                                                CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                            }
                                        }
                                    }
                                }
                                // 1-2
                                else
                                {
                                    // 1-2-0
                                    if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                                    {
                                        // 1-2-0-0
                                        if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-0-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                // 1-2-0-0-0-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                        Cell1x1(ref worksheet, ref schedule, current, 0, i);
                                                    for (int i = 0; i < 2; i++)
                                                        Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                                }
                                                // 1-2-0-0-0-1
                                                else
                                                {
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                                    Cell2x1andMore(ref worksheet, ref schedule, current, 1);
                                                }
                                            }
                                            // 1-2-0-0-1
                                            else
                                            {
                                                // 1-2-0-0-1-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                        Cell1x1(ref worksheet, ref schedule, current, 0, i);
                                                    Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                                }
                                                // 1-2-0-0-1-1
                                                else
                                                {
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, 0);
                                                    CellTandMore(ref worksheet, ref schedule, current, 1, 1);
                                                }
                                            }
                                        }
                                        // 1-2-0-1
                                        else
                                        {
                                            // 1-2-0-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                // 1-2-0-1-0-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    Cell2x1andMore(ref worksheet, ref schedule, current, 0);
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                                }
                                                // 1-2-0-1-0-1
                                                else
                                                {
                                                    if (current.schedule.x == 14 && current.schedule.y == 86)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                            Cell2x1andMore(ref worksheet, ref schedule, current, i);
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                            Cell2x1andMore(ref worksheet, ref schedule, current, i);
                                                    }
                                                }
                                            }
                                            // 1-2-0-1-1
                                            else
                                            {
                                                // 1-2-0-1-1-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    CellTandMore(ref worksheet, ref schedule, current, 1, 0);
                                                    Cell1x1(ref worksheet, ref schedule, current, 0, 1);
                                                }
                                                // 1-2-0-1-1-1
                                                else
                                                {
                                                    worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                                }
                                            }
                                        }
                                    }
                                    // 1-2-1
                                    else
                                    {
                                        // 1-2-1-0
                                        if (worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-1-0-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                // 1-2-1-0-0-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    Cell1x2(ref worksheet, ref schedule, current, 0);
                                                    for (int i = 0; i < 2; i++)
                                                        Cell1x1andMoreRight(ref worksheet, ref schedule, current, i);
                                                }
                                                // 1-2-1-0-0-1
                                                else
                                                {
                                                    CellTandMore(ref worksheet, ref schedule, current, 0, 1);
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, 0);
                                                }
                                            }
                                            // 1-2-1-0-1
                                            else
                                            {
                                                // 1-2-1-0-1-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    Cell1x2(ref worksheet, ref schedule, current, 0);
                                                    Cell1x2andMoreRight(ref worksheet, ref schedule, current);
                                                }
                                                // 1-2-1-0-1-1
                                                else
                                                {
                                                    worksheet.Cells[current.y, current.x].Style.Borders[IndividualBorder.Right].LineStyle = LineStyle.None;
                                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                                }
                                            }
                                        }
                                        // 1-2-1-1
                                        else
                                        {
                                            // 1-2-1-1-0
                                            if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                            {
                                                // 1-2-1-1-0-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    CellTandMore(ref worksheet, ref schedule, current, 0, 0);
                                                    Cell1x1andMoreRight(ref worksheet, ref schedule, current, 1);
                                                }
                                                // 1-2-1-1-0-1
                                                else
                                                {
                                                    worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                                }
                                            }
                                            // 1-2-1-1-1
                                            else
                                            {
                                                // 1-2-1-1-1-0
                                                if (worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                                {
                                                    worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Left].LineStyle = LineStyle.None;
                                                    Cell2x2andMore(ref worksheet, ref schedule, current, errors[0]);
                                                }
                                                // 1-2-1-1-1-1
                                                else
                                                {
                                                    Cell2x2andMore(ref worksheet, ref schedule, current);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else // в случае ошибки
                        {
                            for (int i = 0; i < 2; ++i)
                            {
                                for (int j = 0; j < 2; ++j)
                                    if (schedule[current.schedule.x + i, current.schedule.y + j] == null)
                                        schedule[current.schedule.x + i, current.schedule.y + j] = "ERROR";
                            }
                            // todo: в случае неограниченности сверху или снизу
                        }
                        current.schedule.y += 2;
                    }
                    current.schedule.x += 2;
                    current.x += 2; //следующая группа
                }
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E]    -> Обработка расписания");
                return schedule;
            });
        }
    

        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██████████████████████████████░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░██░░
        // ░░██░░░░░x=0░░░░██░░░░░x=1░░░░██░░
        // ░░██░░░░░y=0░░░░██░░░░░y=0░░░░██░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░██░░
        // ░░██████████████████████████████░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░██░░
        // ░░██░░░░░x=0░░░░██░░░░░x=1░░░░██░░
        // ░░██░░░░░y=1░░░░██░░░░░y=1░░░░██░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░██░░
        // ░░██████████████████████████████░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░





        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░████████████████░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░██░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░██░░██░░██░░██░░██░░
        // ░░██░░░░░░░░░░░░██░░██░░░░██░░░░██░░
        // ░░██░░░░░░░░░░░░██░░██░░██░░██░░██░░
        // ░░████████████████░░░░░░░░░░░░░░░░░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        private static void Cell1x1(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int x, int y)
        {
            string temp = "";
            if (worksheet.Cells[current.y + y, current.x + x].Value != null)
            {
                temp = worksheet.Cells[current.y + y, current.x + x].StringValue;
                if (worksheet.Cells[current.y + y, current.x + x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    temp += errors[1];
            }
            schedule[current.schedule.x + x, current.schedule.y + y] = temp;
        }

        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░████████████████░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░██░░██░░░░░░░░░░██████░░
        // ░░██░░░░░░░░░░░░██░░██░░██░░██░░░░░░██░░
        // ░░██░░░░░░░░░░░░██░░██░░░░██░░░░░░██░░░░
        // ░░██░░░░░░░░░░░░██░░██░░██░░██░░██████░░
        // ░░████████████████░░░░░░░░░░░░░░░░░░░░░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        private static void Cell1x2(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int x)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            for (int i = 0; i < 2; ++i)
            {
                if (worksheet.Cells[current.y + i, current.x + x].Value != null)
                {
                    string currentStr = worksheet.Cells[current.y + i, current.x + x].StringValue.Trim();
                    if (!temp.Contains(currentStr))
                        temp.Add(currentStr);
                }
            }
            for (int i = 1; i < temp.Count; ++i)
                temp[0] += temp[i] + ' ';
            temp[0] = temp[0].TrimEnd();
            for (int i = 0; i < 2; ++i)
            {
                schedule[current.schedule.x + x, current.schedule.y + i] = temp[0];
                if (temp[0] != "" && worksheet.Cells[current.y + i, current.x + x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                    && temp[0] != "")
                   schedule[current.schedule.x + x, current.schedule.y + i] += errors[1];
            }
        }
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██████████████████████████████░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░2x2...
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██████████████████████████████░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // Верно и для 2x2
        private static void Cell2x2andMore(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, string error = null)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            for (int i = 0; i < 2; ++i)
            {
                int x = current.x - 1; // + 1 переносим внутрь цикла
                do
                {
                    ++x;
                    if (worksheet.Cells[current.y + i, x].Value != null)
                    {
                        string tempStr = worksheet.Cells[current.y + i, x].StringValue.Trim();
                        if (!temp.Contains(tempStr))
                            temp.Add(tempStr);
                    }
                } while (worksheet.Cells[current.y + i, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            }
            if (temp.Count == 2)
                if (temp[1].ToUpper().Contains("ФИЗИЧЕСКАЯ КУЛЬТУРА"))
                {
                    temp[0] = "Физическая культура";
                }
                else
                {
                    for (int i = 1; i < temp.Count; i++)
                        temp[0] += temp[i] + ' ';
                    temp[0] = temp[0].TrimEnd();
                }
            else
            {
                for (int i = 1; i < temp.Count; i++)
                    temp[0] += temp[i] + ' ';
                temp[0] = temp[0].TrimEnd();
            }
            if (temp[0] != "")
            {
                if (worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None
                    || worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                    temp[0] += lectureConst;
                temp[0] += error;
            }
            for (int i = 0; i < 2; ++i)
            {
                int x = current.x - 1; // + 1 переносим внутрь цикла
                int startX = x + 1;
                do
                {
                    ++x;
                    schedule[current.schedule.x + x - startX, current.schedule.y + i] = temp[0];
                    if (worksheet.Cells[current.y + i, x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && temp[0] != "")
                        schedule[current.schedule.x + x - startX, current.schedule.y + i] += errors[1];
                } while (worksheet.Cells[current.y + i, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            }
        }

        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██████████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██████░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██░░██░░██░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██░░░░░░██░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██████░░██░░██░░██░░██░░██░░
        // ░░██████████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        private static void Cell2x1andMore(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int y)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            int x = current.x - 1; // + 1 переносим внутрь цикла
            do
            {
                ++x;
                if (worksheet.Cells[current.y + y, x].Value != null)
                {
                    string tempStr = worksheet.Cells[current.y + y, x].StringValue.Trim();
                    if (!temp.Contains(tempStr))
                        temp.Add(tempStr);
                }
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            for (int i = 1; i < temp.Count; ++i)
                temp[0] += temp[i] + ' ';
            temp[0] = temp[0].TrimEnd();
            if (temp[0] != "")
            {
                if (worksheet.Cells[current.y + y, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                    temp[0] += lectureConst;
            }
            x = current.x - 1; // + 1 переносим внутрь цикла
            int startX = x + 1;
            do
            {
                ++x;
                schedule[current.schedule.x + x - startX, current.schedule.y + y] = temp[0];
                if (worksheet.Cells[current.y + y, x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                    && temp[0] != "")
                    schedule[current.schedule.x + x - startX, current.schedule.y + y] += errors[1];
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
        }
        // с правой стороны открытая ячейка
        private static void Cell1x1andMoreRight(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int y)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            int x = current.x; // + 1 переносим внутрь цикла
            do
            {
                ++x;
                if (worksheet.Cells[current.y + y, x].Value != null)
                {
                    string tempStr = worksheet.Cells[current.y + y, x].StringValue.Trim();
                    if (!temp.Contains(tempStr))
                        temp.Add(tempStr);
                }
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            for (int i = 1; i < temp.Count; ++i)
                temp[0] += temp[i] + ' ';
            temp[0] = temp[0].TrimEnd();
            x = current.x; // + 1 переносим внутрь цикла
            int startX = x + 1;
            do
            {
                ++x;
                schedule[current.schedule.x + x - startX, current.schedule.y + y] = temp[0];
                if (worksheet.Cells[current.y + y, x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                    && temp[0] != "")
                    schedule[current.schedule.x + x - startX, current.schedule.y + y] += errors[1];
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
        }
        //! test it
        private static void Cell1x2andMoreRight(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            for (int i = 0; i < 2; ++i)
            {
                int x = current.x; // + 1 переносим внутрь цикла
                do
                {
                    ++x;
                    if (worksheet.Cells[current.y + i, x].Value != null)
                    {
                        string tempStr = worksheet.Cells[current.y + i, x].StringValue.Trim();
                        if (!temp.Contains(tempStr))
                            temp.Add(tempStr);
                    }
                } while (worksheet.Cells[current.y + i, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            }
            for (int i = 1; i < temp.Count; i++)
                temp[0] += temp[i] + ' ';
            temp[0] = temp[0].TrimEnd();
            if (temp[0] != "")
            {
                if (worksheet.Cells[current.y, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None
                    || worksheet.Cells[current.y + 1, current.x + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                    temp[0] += lectureConst;
            }
            for (int i = 0; i < 2; ++i)
            {
                int x = current.x; // + 1 переносим внутрь цикла
                int startX = x + 1;
                do
                {
                    ++x;
                    schedule[current.schedule.x + x - startX, current.schedule.y + 1] = temp[0];
                    if (worksheet.Cells[current.y + i, x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && temp[0] != "")
                        schedule[current.schedule.x + x - startX, current.schedule.y + 1] += errors[1];
                } while (worksheet.Cells[current.y + i, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            }
        }

        // how to use: y - ряд который парсим
        //             _x - ячейка которая не пуста
        private static void CellTandMore(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int _x, int y)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            if (worksheet.Cells[current.y + (y + 1) % 2, current.x + _x].Value != null)
            {
                string tempStr = worksheet.Cells[current.y + (y + 1) % 2, current.x + _x].StringValue.Trim();
                    if (!temp.Contains(tempStr))
                        temp.Add(tempStr);
            }
            int x = current.x - 1; // + 1 переносим внутрь цикла
            do
            {
                ++x;
                if (worksheet.Cells[current.y + y, x].Value != null)
                {
                    string tempStr = worksheet.Cells[current.y + y, x].StringValue.Trim();
                    if (!temp.Contains(tempStr))
                        temp.Add(tempStr);
                }
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            for (int i = 1; i < temp.Count; ++i)
                temp[0] += temp[i] + ' ';
            temp[0] = temp[0].TrimEnd();
            if (temp[0] != "")
            {
                if (worksheet.Cells[current.y + y, current.x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                    temp[0] += lectureConst;
                temp[0] += errors[2];
            }
            x = current.x - 1; // + 1 переносим внутрь цикла
            int startX = x + 1;
            do
            {
                ++x;
                schedule[current.schedule.x + x - startX, current.schedule.y + y] = temp[0];
                if (worksheet.Cells[current.y + y, x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                    && temp[0] != "")
                    schedule[current.schedule.x + x - startX, current.schedule.y + y] += errors[1];
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            schedule[current.schedule.x + _x, current.schedule.y + (y + 1) % 2] = temp[0];
            if (worksheet.Cells[current.y + (y + 1) % 2, current.x + _x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                && temp[0] != "")
                schedule[current.schedule.x + _x, current.schedule.y + (y + 1) % 2] += errors[1];
        }
    }
}
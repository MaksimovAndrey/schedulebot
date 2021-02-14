//using System;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using OfficeOpenXml;

//using Schedulebot.Parsing.Enums;

//using Schedulebot.Schedule;

//namespace Schedulebot.Parsing
//{
//    public static class ParserOld
//    {
//        private static readonly Type stringType = typeof(string);

//        public static ScheduleLecture ParseLecture(string timeStart, string timeEnd, string body, Dictionaries dictionaries)
//        {
//            const string labStr = "(Лабораторная)";
//            const string seminarStr = "(Практика (семинарские занятия))";
//            const string lectureStr = "(Лекция)";

//            const string lecturerRegexPattern = "[А-ЯЁ]{1}[а-яё]+([-]{1}[А-ЯЁ]{1}[а-яё]+)? {1}[А-ЯЁ]{1}[.]{1}([А-ЯЁ]{1}[.]?)?|!Вакансия";

//            bool isLecture = false;
//            bool isSeminar = false;
//            bool isLab = false;
//            bool isRemotely = false;

//            string tempBody = body;

//            if (tempBody.Contains(labStr))
//            {
//                tempBody = tempBody.Replace(labStr, "");
//                isLab = true;
//            }

//            if (tempBody.Contains(seminarStr))
//            {
//                tempBody = tempBody.Replace(seminarStr, "");
//                isSeminar = true;
//            }

//            if (tempBody.Contains(lectureStr))
//            {
//                tempBody = tempBody.Replace(lectureStr, "");
//                isLecture = true;
//            }

//            //* На будущее
//            //* Корпус № ?[0-9]+(\/([0-9]+[А-яЁё]?)?(ВП, \d этаж)?)?
//            //* Regex regexLectureHall = new Regex("");

//            // Чистим строку
//            tempBody = tempBody.Trim();
//            while (tempBody.Contains("  "))
//                tempBody = tempBody.Replace("  ", " ");

//            MatchCollection matches;

//            // Ищем аудиторию
//            const string lectureHallStr = "КОРПУС №";
//            string lectureHall = null;

//            int indexOfLectureHall = tempBody.ToUpper().IndexOf(lectureHallStr);
//            if (indexOfLectureHall != -1)
//            {
//                lectureHall = tempBody.Substring(indexOfLectureHall).Trim(); // Вариант с Корпус №
//                //lectureHall = tempBody.Substring(indexOfLectureHall + lectureHallStr.Length); // Вариант без Корпус №
//                tempBody = tempBody.Substring(0, indexOfLectureHall);
//            }
//            else
//            {
//                const string unknownHallVzda = "В.З./Д.А.";
//                indexOfLectureHall = tempBody.ToUpper().IndexOf(unknownHallVzda);
//                if (indexOfLectureHall != -1)
//                {
//                    lectureHall = tempBody.Substring(indexOfLectureHall).Trim();
//                    tempBody = tempBody.Substring(0, indexOfLectureHall);
//                }
//                else
//                {
//                    const string fizra = @"Спортзал на Гагарина/[0-9]+";
//                    matches = Regex.Matches(tempBody, fizra);
//                    if (matches.Count == 1)
//                    {
//                        lectureHall = matches[0].Value;
//                        tempBody = tempBody.Remove(matches[0].Index, matches[0].Length);
//                    }
//                    else
//                    {
//                        // не нашли аудиторию
//                    }
//                }
//            }

//            // Ищем ФИО
//            string lecturer = null;

//            Regex regexFullName =
//                new Regex(lecturerRegexPattern);

//            matches = regexFullName.Matches(tempBody);
//            if (matches.Count == 1)
//            {
//                lecturer = matches[0].Value;
//                tempBody = tempBody.Remove(matches[0].Index, matches[0].Length);
//            }
//            else if (matches.Count == 0)
//            {
//                int indexTemp;
//                for (int i = 0; i < dictionaries.fullName.Count; ++i)
//                {
//                    indexTemp = tempBody.IndexOf(dictionaries.fullName[i]);
//                    if (indexTemp != -1)
//                    {
//                        lecturer = dictionaries.fullName[i];
//                        tempBody = tempBody.Remove(indexTemp, dictionaries.fullName[i].Length);
//                        break;
//                    }
//                }
//            }
//            // Такого вроде нет сейчас
//            /*else if (matches.Count >= 2)
//            {
//                if (dictionaries.doubleOptionallySubject.ContainsKey(parse))
//                {
//                    return new ScheduleLecture(
//                        status: ParseStatus.F2,
//                        subject: dictionaries.doubleOptionallySubject[parse],
//                        isLecture: isLecture,
//                        isRemotely: isRemotely,
//                        errorType: errorType
//                    );
//                }
//            }*/

//            tempBody = tempBody.Replace('\n', ' ');
//            while (tempBody.Contains("  "))
//                tempBody = tempBody.Replace("  ", " ");
//            tempBody = tempBody.Trim();

//            // Выводы
//            ParseStatus status = ParseStatus.Unknown;
//            if (lectureHall == null && lecturer == null)
//            {
//                //Console.WriteLine("N0:" + body);
//                status = ParseStatus.N0;
//            }
//            else if (lectureHall != null && lecturer != null)
//            {
//                string subject = tempBody;
//                if (dictionaries.acronymToPhrase.ContainsKey(subject))
//                {
//                    subject = dictionaries.acronymToPhrase[subject];
//                }
//                else if (tempBody.Contains(' ') || tempBody.Length > 4) // Если все капсом и более одного слова, заглавной остается только первая буква
//                {
//                    for (int k = 0; k < tempBody.Length; ++k)
//                    {
//                        if (tempBody[k] != char.ToUpper(tempBody[k]))
//                            break;
//                        if (k == tempBody.Length - 1)
//                            subject = char.ToUpper(tempBody[0]) + tempBody.Substring(1).ToLower();
//                    }
//                }

//                tempBody = subject;
//                status = ParseStatus.F3;
//            }
//            else if (lectureHall == null && lecturer != null)
//            {
//                // Если все капсом и (более одного слова или больше 4 знаков), заглавной остается только первая буква
//                if ((tempBody.Contains(' ') || tempBody.Length > 4))
//                {
//                    for (int k = 0; k < tempBody.Length; ++k)
//                    {
//                        if (tempBody[k] != char.ToUpper(tempBody[k]))
//                            break;
//                        if (k == tempBody.Length - 1)
//                            tempBody = char.ToUpper(tempBody[0]) + tempBody.Substring(1).ToLower();
//                    }
//                }
//                //Console.WriteLine("N2:" + body);
//                status = ParseStatus.N1;
//            }
//            else if (lectureHall != null && lecturer == null)
//            {
//                // Если все капсом и (более одного слова или больше 4 знаков), заглавной остается только первая буква
//                if ((tempBody.Contains(' ') || tempBody.Length > 4))
//                {
//                    for (int k = 0; k < tempBody.Length; ++k)
//                    {
//                        if (tempBody[k] != char.ToUpper(tempBody[k]))
//                            break;
//                        if (k == tempBody.Length - 1)
//                            tempBody = char.ToUpper(tempBody[0]) + tempBody.Substring(1).ToLower();
//                    }
//                }
//                //Console.WriteLine("N2last:" + body);
//                status = ParseStatus.N1;
//            }

//            return new ScheduleLecture(
//                status: status,
//                timeStart: timeStart,
//                timeEnd: timeEnd,
//                body: body,
//                subject: tempBody,
//                lectureHall: lectureHall,
//                lecturer: lecturer,
//                isLecture: isLecture,
//                isSeminar: isSeminar,
//                isLab: isLab,
//                isRemotely: isRemotely
//            );
//        }
       
//        public static List<Group> Mapper(List<string> pathsToFile, Dictionaries dictionaries)
//        {
//            List<Group> groups = new List<Group>();
//            for (int currentFile = 0; currentFile < pathsToFile.Count; currentFile++)
//            {
//                string format = pathsToFile[currentFile].Substring(pathsToFile[currentFile].LastIndexOf('.') + 1);
//                switch (format)
//                {
//                    case "xlsx":
//                    {
//                        try
//                        {
//                            var excelPackage = new ExcelPackage(new FileInfo(pathsToFile[currentFile])); // Открытие Excel file
//                            var worksheet = excelPackage.Workbook.Worksheets[0];// Выбор листа (worksheet)

//                            List<Group> parsedGroups = ParseXlsx(worksheet, dictionaries);
//                            if (parsedGroups == null || parsedGroups.Count == 0)
//                                continue;

//                            groups.AddRange(parsedGroups);
//                        }
//                        catch
//                        {
//                            continue;
//                        }
//                        break;
//                    }
//                }
//            }
//            return groups;
//        }

//        public static List<Group> ParseXlsx(ExcelWorksheet worksheet, Dictionaries dictionaries)
//        {
//            List<Group> groups = new List<Group>();

//            // Ищем ячейку "Время"
//            const int timeCellC = 2;
//            int timeCellR = 1;

//            while (timeCellR < 100)
//            {
//                if (worksheet.Cells[timeCellR, timeCellC].Value != null
//                    && worksheet.Cells[timeCellR, timeCellC].GetValue<string>().Trim().ToUpper() == "ВРЕМЯ")
//                    break;
//                timeCellR++;
//            }
//            // Считаем количество групп и заполняем названия групп
//            int countOfGroups = 0;
//            int currentC = timeCellC + 1;

//            while (worksheet.Cells[timeCellR, currentC].Value != null)
//            {
//                groups.Add(new Group());

//                // todo: название лучше брать через Regex
//                const string trashStr = "ГРУППА";
//                string groupName = worksheet.Cells[timeCellR, currentC].GetValue<string>();
//                int indexOfTrash = groupName.ToUpper().IndexOf(trashStr);
//                if (indexOfTrash != -1)
//                {
//                    groupName = groupName.Substring(indexOfTrash + trashStr.Length).Trim();
//                }
//                // Удаляем _цифра после группы
//                if (groupName[^2] == '_')
//                    groupName = groupName[0..^2];

//                groups[countOfGroups].name = groupName;

//                currentC += 1;
//                countOfGroups++;
//            }

//            RemoveMergedRanges(worksheet);

//            // Проходим по ячейкам
//            const int dayCellC = 1;
//            int currentR = timeCellR + 1;
//            currentC = timeCellC + 1;

//            while (worksheet.Cells[currentR, dayCellC].Value != null) //! что-то надо придумать
//            {
//                for (int currentGroup = 0; currentGroup < countOfGroups; currentGroup++)
//                {
//                    for (int currentWeek = 0; currentWeek < 2; currentWeek++)
//                    {
//                        // Проверка тела
//                        if (worksheet.Cells[currentR + currentWeek, currentC + currentGroup].Value == null)
//                            continue;

//                        string body = worksheet.Cells[currentR + currentWeek, currentC + currentGroup].GetValue<string>().Trim();
//                        if (body.Length == 0)
//                            continue;

//                        // Проверка дня, выбрасываем null, если день нет возможности найти
//                        if (worksheet.Cells[currentR, dayCellC].Value == null)
//                            return null;

//                        // Проверка дня, выбрасываем null, если день не определили
//                        var tes = worksheet.Cells[currentR, dayCellC].GetValue<string>();
//                        string test = worksheet.Cells[currentR, dayCellC].GetValue<string>().Trim().ToUpper();
//                        int dayIndex = Utils.Converter.DayToIndex(worksheet.Cells[currentR, dayCellC].GetValue<string>().Trim().ToUpper());
//                        if (dayIndex == -1)
//                        {

//                            return null;
//                        }

//                        // Ищем время
//                        string timeStart;
//                        string timeEnd;
//                        if (worksheet.Cells[currentR, timeCellC].Value == null)
//                        {
//                            timeStart = "";
//                            timeEnd = "";
//                        }
//                        else
//                        {
//                            string time = worksheet.Cells[currentR, timeCellC].GetValue<string>().Trim();
//                            int indexOfHyphen = time.IndexOf('-');
//                            int indexOfLineBreak = time.IndexOf('\n');
//                            if (indexOfHyphen != -1)
//                            {
//                                timeStart = time.Substring(0, indexOfHyphen).Trim();
//                                timeEnd = time.Substring(indexOfHyphen + 1).Trim();
//                            }
//                            else if (indexOfLineBreak != -1)
//                            {
//                                timeStart = time.Substring(0, indexOfLineBreak).Trim();
//                                timeEnd = time.Substring(indexOfLineBreak + 1).Trim();
//                            }
//                            else
//                            {
//                                timeStart = time;
//                                timeEnd = "";
//                            }
//                        }
//                        // Анализируем body
//                        // 382007-2-1
//                        // 382007-2(1)
//                        var matches = Regex.Matches(body, groups[currentGroup].name + @"[-(]\d\)?");
//                        if (matches.Count == 2)
//                        {
//                            string part1 = body.Substring(matches[0].Index + matches[0].Length, matches[1].Index - matches[0].Length).Trim();
//                            string part2 = body.Substring(matches[1].Index + matches[1].Length).Trim();

//                            ScheduleLecture[] lectures = {
//                                ParseLecture(timeStart, timeEnd, part1, dictionaries),
//                                ParseLecture(timeStart, timeEnd, part2, dictionaries)
//                            };

//                            for (int currentMatch = 0; currentMatch < 2; currentMatch++)
//                            {
//                                char subgroupChar =
//                                    matches[currentMatch].Value[matches[currentMatch].Length - 1] == ')' ?
//                                        matches[currentMatch].Value[matches[currentMatch].Length - 2] :
//                                        matches[currentMatch].Value[matches[currentMatch].Length - 1];

//                                switch (subgroupChar)
//                                {
//                                    case '1':
//                                        groups[currentGroup].subgroups[0].weeks[currentWeek].days[dayIndex].lectures.Add(lectures[currentMatch]);
//                                        break;
//                                    case '2':
//                                        groups[currentGroup].subgroups[1].weeks[currentWeek].days[dayIndex].lectures.Add(lectures[currentMatch]);
//                                        break;
//                                }
//                            }
//                        }
//                        else if (matches.Count == 1)
//                        {
//                            if (body.IndexOf(groups[currentGroup].name) == 0)
//                            {
//                                char subgroupChar =
//                                    matches[0].Value[matches[0].Length - 1] == ')' ?
//                                        matches[0].Value[matches[0].Length - 2] :
//                                        matches[0].Value[matches[0].Length - 1];

//                                switch (subgroupChar)
//                                {
//                                    case '1':
//                                        groups[currentGroup].subgroups[0].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                            ParseLecture(timeStart, timeEnd, body.Substring(matches[0].Index + matches[0].Length), dictionaries)
//                                        );
//                                        break;
//                                    case '2':
//                                        groups[currentGroup].subgroups[1].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                            ParseLecture(timeStart, timeEnd, body.Substring(matches[0].Index + matches[0].Length), dictionaries)
//                                        );
//                                        break;
//                                    default:
//                                        ScheduleLecture lecture = ParseLecture(timeStart, timeEnd, body.Substring(groups[currentGroup].name.Length), dictionaries);
//                                        groups[currentGroup].subgroups[0].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                            lecture);
//                                        groups[currentGroup].subgroups[1].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                            lecture);
//                                        break;
//                                }
//                            }
//                            else
//                            {
//                                var lecture = ParseLecture(timeStart, timeEnd, body, dictionaries);

//                                groups[currentGroup].subgroups[0].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                    lecture);
//                                groups[currentGroup].subgroups[1].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                    lecture);
//                            }
//                        }
//                        else
//                        {
//                            var lecture = ParseLecture(timeStart, timeEnd, body, dictionaries);

//                            groups[currentGroup].subgroups[0].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                lecture);

//                            groups[currentGroup].subgroups[1].weeks[currentWeek].days[dayIndex].lectures.Add(
//                                lecture);
//                        }
//                    }
//                }
//                currentR += 2;
//            }
//            // Сортируем
//            for (int i = 0; i < groups.Count; i++)
//            {
//                groups[i].SortLectures();
//            }

//            return groups;
//        }
      
//        private static void RemoveMergedRanges(ExcelWorksheet worksheet)
//        {
//            ExcelWorksheet.MergeCellsCollection mergedCells = worksheet.MergedCells;
//            for (int i = 0; i < mergedCells.Count; i++)
//            {
//                UnmergeCells(mergedCells[i], worksheet);
//            }
//        }

//        private static void UnmergeCells(string mergedRange, ExcelWorksheet worksheet)
//        {
//            if (mergedRange != null)
//            {
//                object value = worksheet.Cells[mergedRange].Value;
//                Type type = value.GetType();
//                // Колхоз для клонирования значения ячеек
//                if (type == typeof(System.Object[,]))
//                {
//                    System.Object[,] tempObject = (System.Object[,])value;
//                    string tempValue = (string)tempObject[0, 0];
//                    for (int i = 1; i < tempObject.GetLength(0); i++)
//                    {
//                        tempValue += (string)tempObject[i, 0];
//                    }
//                    value = tempValue;
//                }
//                worksheet.Cells[mergedRange].Merge = false;
//                worksheet.Cells[mergedRange].Value = value;
//            }
//        }

//        private static void DeleteCell(ref ExcelWorksheet worksheet, (int, int) cellCoord)
//        {
//            if (worksheet.Cells[cellCoord.Item1, cellCoord.Item2].Merge == true)
//            {
//                var value = worksheet.Cells[2, 1].Value;
//                string range = GetMergedRange(ref worksheet, cellCoord); //get range of merged cells
//                worksheet.Cells[range].Merge = false; //unmerge range
//                //worksheet.Cells[cellAddress].Clear(); //clear value
//                worksheet.Cells[range].Value = value;
//                //merge the cells you want again.
//                //fill the value in cells again
//            }
//        }

//        private static string GetMergedRange(ref ExcelWorksheet worksheet, (int, int) cellCoord)
//        {
//            ExcelWorksheet.MergeCellsCollection mergedCells = worksheet.MergedCells;
//            foreach (var merged in mergedCells)
//            {
//                ExcelRange range = worksheet.Cells[merged];
//                ExcelCellAddress cell = new ExcelCellAddress(cellCoord.Item1, cellCoord.Item2);
//                if (range.Start.Row <= cell.Row && range.Start.Column <= cell.Column)
//                {
//                    if (range.End.Row >= cell.Row && range.End.Column >= cell.Column)
//                    {
//                        return merged.ToString();
//                    }
//                }
//            }
//            return "";
//        }
//    }
//}
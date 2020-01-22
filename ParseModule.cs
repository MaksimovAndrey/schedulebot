using System;
using GemBox.Spreadsheet;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace schedulebot
{
    public static class Parsing
    {
        public static readonly string[] errors = { "¹", "²", "³" };
        public const string lectureConst = "+Л+";
        public static Group[] Mapper(string pathToFile)
        {
            string format = pathToFile.Substring(pathToFile.LastIndexOf('.') + 1);
            string[,] temp = null;
            switch (format)
            {
                case "xls":
                {
                    ExcelFile scheduleSource = ExcelFile.Load(pathToFile);   // Открытие Excel file
                    ExcelWorksheet worksheet = scheduleSource.Worksheets.ActiveWorksheet; // Выбор листа (worksheet)
                    temp = ParseXls(worksheet);
                    break;
                }
            }
            int countOfGroups = temp.GetLength(0) / 2;
            Group[] groups = new Group[countOfGroups];
            // Засунул старый метод и забираю расписание, я клоун
            for (int i = 0; i < countOfGroups; ++i)
            {
                groups[i] = new Group();
                groups[i].name = temp[i * 2, 0];
                for (int j = 0; j < 2; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        for (int l = 0; l < 6; ++l)
                        {
                            for (int n = 0; n < 8; ++n)
                            {
                                groups[i].schedule[j].weeks[k].days[l].lectures[n].Parse(temp[i * 2 + j, 2 + l * 16 + n * 2 + k]);
                            }
                        }
                    }
                }
            }
            return groups;
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
        // todo: проверить условия
        // todo: доделать обработчики

        // todo: после парса надо проверить группы на дубликаты и оставить одну (в которой больше пар)
        // todo: собрать парсер
        public static string[,] ParseXls(ExcelWorksheet worksheet)
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S]    -> Обработка расписания");
            int indent = 2; // отступ от времени (начало ячеек)
            CurrentInfo current = new CurrentInfo(indent, 0);
            // Определяем где группа и начало пар
            int groupNameY = 10; // линия, в которой содержатся имена групп
            int indentGroupToSchedule = 3;
            for (int i = 0; i < 10; ++i)
            {
                if ((string)worksheet.Cells[groupNameY, current.x].Value != null)
                    if (((string)worksheet.Cells[groupNameY, current.x].Value).Trim() != "")
                        if (((string)worksheet.Cells[groupNameY, current.x].Value).Trim().IndexOf("38") == 0)
                            break;
                ++groupNameY;
            }
            // Считаем сколько групп
            int countOfGroups = 0;
            while (worksheet.Cells[groupNameY, current.x].Value != null)
            {
                ++countOfGroups;
                current.x += 2;
            }
            string[,] schedule = new string[countOfGroups * 2, 98];
            // Проход по всем группам
            while (worksheet.Cells[groupNameY, current.x].Value != null)
            {
                // записываем имя группы
                schedule[current.schedule.x, 0] = worksheet.Cells[groupNameY, current.x].StringValue.Trim();
                schedule[current.schedule.x + 1, 0] = schedule[current.schedule.x, 0];
                // записываем подгруппу
                schedule[current.schedule.x, 1] = "1";
                schedule[current.schedule.x + 1, 1] = "2";
                // Проход по ячейкам
                for (current.y = groupNameY + indentGroupToSchedule; current.y < groupNameY + indentGroupToSchedule + 96; current.y += 2)
                {
                    // Отмена объединения ячеек
                    for (int j = 0; j < 2; ++j)
                    {
                        for (int k = 0; k < 2; ++k)
                        {
                            if (worksheet.Cells[current.y, current.x].Value != null)
                            {
                                if (!worksheet.Cells[current.y, current.x].Value.ToString().Trim().ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА")
                                    && !worksheet.Cells[current.y, current.x].Value.ToString().Trim().ToUpper().Contains("ФИЗИЧЕСКАЯ КУЛЬТУРА"))
                                {
                                    CellRange mergedRange = worksheet.Cells[current.y + j, current.x + k].MergedRange;
                                    if (mergedRange != null)
                                        worksheet.Cells.GetSubrangeAbsolute(
                                            mergedRange.FirstRowIndex,
                                            mergedRange.FirstColumnIndex,
                                            mergedRange.LastRowIndex,
                                            mergedRange.LastColumnIndex).Merged = false;
                                }
                            }
                            else
                            {
                                CellRange mergedRange = worksheet.Cells[current.y + j, current.x + k].MergedRange;
                                if (mergedRange != null)
                                    worksheet.Cells.GetSubrangeAbsolute(
                                        mergedRange.FirstRowIndex,
                                        mergedRange.FirstColumnIndex,
                                        mergedRange.LastRowIndex,
                                        mergedRange.LastColumnIndex).Merged = false;
                            }
                        }
                    }
                    // Пустая ячейка
                    if (worksheet.Cells[current.y, current.x].Value == null
                        && worksheet.Cells[current.y, current.x + 1].Value == null
                        && worksheet.Cells[current.y + 1, current.x].Value == null
                        && worksheet.Cells[current.y + 1, current.x + 1].Value == null
                        && worksheet.Cells[current.y, current.x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[current.y, current.x + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[current.y + 1, current.x].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[current.y + 1, current.x + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                schedule[current.schedule.x + i, current.schedule.y + j] = "";
                            }
                        }
                        continue; // переход к следующей группе ячеек
                    }
                    // Уже заполнена
                    else if (schedule[current.schedule.x, current.schedule.y] != null
                        && schedule[current.schedule.x, current.schedule.y + 1] != null
                        && schedule[current.schedule.x + 1, current.schedule.y] != null
                        && schedule[current.schedule.x + 1, current.schedule.y + 1] != null)
                    {
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
                                if (schedule[current.schedule.x + 1, current.schedule.y] != null)
                                {
                                    // 1-0-0-0
                                    if (schedule[current.schedule.x, current.schedule.y + 1] != null)
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
                                    if (schedule[current.schedule.x, current.schedule.y + 1] != null)
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
                            else if (schedule[current.schedule.x + 1, current.schedule.y] != null)
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
                                                for (int i = 0; i < 2; i++)
                                                    Cell2x1andMore(ref worksheet, ref schedule, current, i);
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
        public static void Cell1x1(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int x, int y)
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
        public static void Cell1x2(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int x)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            for (int i = 0; i < 2; ++i)
            {
                if (worksheet.Cells[current.y + i, current.x + x].Value != null)
                {
                    string yaDebil = worksheet.Cells[current.y + i, current.x + x].StringValue.Trim();
                    if (!temp.Contains(yaDebil))
                        temp.Add(yaDebil);
                }
            }
            for (int i = 0; i < temp.Count; ++i)
                temp[0] += ' ' + temp[i];
            for (int i = 0; i < 2; ++i)
            {
                schedule[current.schedule.x + x, current.schedule.y + i] = temp[0];
                if (temp[0] != "" && worksheet.Cells[current.y + i, current.x + x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
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
        public static void Cell2x2andMore(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, string error = null)
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
            for (int i = 1; i < temp.Count; i++)
                temp[0] += ' ' + temp[i];
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
                    schedule[current.schedule.x + x - startX, current.schedule.y + 1] = temp[0];
                    if (worksheet.Cells[current.y + i, x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                        schedule[current.schedule.x + x - startX, current.schedule.y + 1] += errors[1];
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
        public static void Cell2x1andMore(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int y)
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
            for (int i = 0; i < temp.Count; ++i)
                temp[0] += ' ' + temp[i];
            if (temp[0] != "")
            {
                if (worksheet.Cells[current.y + y, current.x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                    temp[0] += lectureConst;
            }
            x = current.x - 1; // + 1 переносим внутрь цикла
            int startX = x + 1;
            do
            {
                ++x;
                schedule[current.schedule.x + x - startX, current.schedule.y + y] = temp[0];
                if (worksheet.Cells[current.y + y, x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    schedule[current.schedule.x + x - startX, current.schedule.y + y] += errors[1];
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
        }
        // с правой стороны открытая ячейка
        public static void Cell1x1andMoreRight(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int y)
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
            for (int i = 0; i < temp.Count; ++i)
                temp[0] += ' ' + temp[i];
            x = current.x; // + 1 переносим внутрь цикла
            int startX = x + 1;
            do
            {
                ++x;
                schedule[current.schedule.x + x - startX, current.schedule.y + y] = temp[0];
                if (worksheet.Cells[current.y + y, x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    schedule[current.schedule.x + x - startX, current.schedule.y + y] += errors[1];
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
        }
        //! test it
        public static void Cell1x2andMoreRight(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current)
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
                temp[0] += ' ' + temp[i];
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
                    if (worksheet.Cells[current.y + i, x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                        schedule[current.schedule.x + x - startX, current.schedule.y + 1] += errors[1];
                } while (worksheet.Cells[current.y + i, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            }
        }

        // how to use: y - ряд который парсим
        //             _x - ячейка которая не пуста
        public static void CellTandMore(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int _x, int y)
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
            for (int i = 0; i < temp.Count; ++i)
                temp[0] += ' ' + temp[i];
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
                if (worksheet.Cells[current.y + y, x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    schedule[current.schedule.x + x - startX, current.schedule.y + y] += errors[1];
            } while (worksheet.Cells[current.y + y, x].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
            schedule[current.schedule.x + _x, current.schedule.y + (y + 1) % 2] = temp[0];
            if (worksheet.Cells[current.y + (y + 1) % 2, current.x + _x].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                schedule[current.schedule.x + _x, current.schedule.y + (y + 1) % 2] += errors[1];
        }
    }
}

/* 2x1
// ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██████████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░░░██████░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░░░░░░░██░░██░░██░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░░░░░██░░░░░░██░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░░░██████░░██░░██░░██░░
        // ░░██████████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        public static void Cell2x1(ref ExcelWorksheet worksheet, ref string[,] schedule, CurrentInfo current, int y)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            for (int i = 0; i < 2; ++i)
            {
                if (worksheet.Cells[current.y + y, current.x + i].Value != null)
                {
                    string yaDebil = worksheet.Cells[current.y + y, current.x + i].StringValue.Trim();
                    if (!temp.Contains(yaDebil))
                        temp.Add(yaDebil);
                }
            }
            for (int i = 0; i < temp.Count; ++i)
                temp[0] += ' ' + temp[i];
            ScheduleLecture tempLecture = new ScheduleLecture();
            tempLecture.Parse(temp[0]);
            for (int i = 0; i < 2; ++i)
            {
                groups[current.group].schedule[i].weeks[y].days[current.day].lectures[current.lecture] = tempLecture;
                if (worksheet.Cells[current.y + y, current.x + i].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    groups[current.group].schedule[i].weeks[y].days[current.day].lectures[current.lecture].errorType += errors[1];
            }
        }
*/

/* 2x2
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        // ░░██████████████████████████████░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░2x2
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░
        // ░░██████████████████████████████░░
        // ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
        public static void Cell2x2(ref ExcelWorksheet worksheet, ref Group[] groups, CurrentInfo current)
        {
            List<string> temp = new List<string>();
            temp.Add("");
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (worksheet.Cells[current.y + j, current.x + i].Value != null)
                    {
                        string yaDebil = worksheet.Cells[current.y + j, current.x + i].StringValue.Trim();
                        if (!temp.Contains(yaDebil))
                            temp.Add(yaDebil);
                    }
                }
            }
            for (int i = 0; i < temp.Count; ++i)
                temp[0] += ' ' + temp[i];
            ScheduleLecture tempLecture = new ScheduleLecture();
            tempLecture.Parse(temp[0]);
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; j++)
                {
                    groups[current.group].schedule[i].weeks[j].days[current.day].lectures[current.lecture] = tempLecture;
                    if (worksheet.Cells[current.y + j, current.x + i].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                        groups[current.group].schedule[i].weeks[j].days[current.day].lectures[current.lecture].errorType += errors[1];
                }
            }
        }


*/

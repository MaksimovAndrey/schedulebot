using System;
using GemBox.Spreadsheet;
using System.IO;
using System.Text.RegularExpressions;

namespace schedulebot
{
    public static class Parse
    {
        // todo: пройти по // 0-* ячейкам и error 3 сделать
        public static int[,,] Schedule(int course, int[,,] sendScheduleUpdateGroups, bool sendUpdates = true)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S]    -> Обработка расписания"); // log
            ExcelFile current_xls = ExcelFile.Load(Const.path_downloads + (course + 1).ToString() + "_course_schedule.xls");   // Открытие Excel file
            ExcelWorksheet worksheet = current_xls.Worksheets.ActiveWorksheet; // Выбор листа (worksheet)
            int index = 0;
            int indent = 2; // отступ от времени (начало ячеек)
            string[,] scheduleTemp = new string[40, 98];
            int sendScheduleUpdateGroupsCount = 0;
            // Определяем где группа и начало пар
            int indentVertical = 10;
            int indentGroupToSchedule = 3;
            for (int i = 0; i < 10; ++i)
            {
                if ((string)worksheet.Cells[indentVertical, index + indent].Value != null)
                    if (((string)worksheet.Cells[indentVertical, index + indent].Value).Trim() != "")
                        if (((string)worksheet.Cells[indentVertical, index + indent].Value).Trim().IndexOf("38") == 0)
                            break;
                ++indentVertical;
            }
            // Проход по всем группам
            while (worksheet.Cells[indentVertical, index + indent].Value != null)
            {
                string group = worksheet.Cells[indentVertical, index + indent].StringValue.Trim(); // Номер группы
                int current = 0; // current - текущая строка расписания
                scheduleTemp[index, 0] = group;
                scheduleTemp[index + 1, 0] = group;
                scheduleTemp[index, 1] = "1";
                scheduleTemp[index + 1, 1] = "2";
                // Проход по ячейкам
                for (int i = indentVertical + indentGroupToSchedule; i < indentVertical + 96 + indentGroupToSchedule; i += 2)
                {
                    current += 2;
                    // Отмена объединения ячеек
                    for (int j = 0; j < 2; ++j)
                    {
                        for (int k = 0; k < 2; ++k)
                        {
                            if (worksheet.Cells[i, index + indent].Value != null)
                            {
                                if (!worksheet.Cells[i, index + indent].Value.ToString().Trim().ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА") && !worksheet.Cells[i, index + indent].Value.ToString().Trim().ToUpper().Contains("ФИЗИЧЕСКАЯ КУЛЬТУРА"))
                                {
                                    CellRange mergedRange = worksheet.Cells[i + j, index + indent + k].MergedRange;
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
                                CellRange mergedRange = worksheet.Cells[i + j, index + indent + k].MergedRange;
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
                    if (worksheet.Cells[i, index + indent].Value == null
                        && worksheet.Cells[i, index + indent + 1].Value == null
                        && worksheet.Cells[i + 1, index + indent].Value == null
                        && worksheet.Cells[i + 1, index + indent + 1].Value == null
                        && worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None
                        && worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                    {
                        scheduleTemp[index, current] = "0";
                        scheduleTemp[index, current + 1] = "0";
                        scheduleTemp[index + 1, current] = "0";
                        scheduleTemp[index + 1, current + 1] = "0";
                        continue; // переход к следующей группе ячеек
                    }
                    // Уже заполнена
                    else if (scheduleTemp[index, current] != null
                        && scheduleTemp[index, current + 1] != null
                        && scheduleTemp[index + 1, current] != null
                        && scheduleTemp[index + 1, current + 1] != null)
                    {
                        continue; // переход к следующей группе ячеек
                    }
                    // ┏━━━━━━━━━━━━━┓ 
                    //                
                    //                
                    //               
                    // ┗━━━━━━━━━━━━━┛
                    else if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None
                        && worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None
                        && worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None
                        && worksheet.Cells[i + 1, index + indent].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                    {
                        // 0
                        if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None
                            && worksheet.Cells[i + 1, index + indent].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None
                            && worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None
                            && worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                        {
                            // ! CHECK ALL
                            // 0-0
                            if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                            {
                                // 0-0-0
                                if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                {
                                    // 0-0-0-0
                                    if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                    {
                                        // 0-0-0-0-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                for (int k = 0; k < 2; ++k)
                                                {
                                                    if (worksheet.Cells[i + j, index + indent + k].Value == null)
                                                        scheduleTemp[index + k, current + j] = "0";
                                                    else if (worksheet.Cells[i + j, index + indent + k].StringValue.Trim().Length == 0)
                                                        scheduleTemp[index + k, current + j] = "0";
                                                    else
                                                    {
                                                        scheduleTemp[index + k, current + j] = String(worksheet.Cells[i + j, index + indent + k].StringValue);
                                                        if (worksheet.Cells[i + j, index + indent + k].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index + k, current + j] += '²';
                                                    }
                                                }
                                            }
                                        }
                                        // 0-0-0-0-1
                                        else
                                        {
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                if (worksheet.Cells[i, index + indent + j].Value == null)
                                                    scheduleTemp[index + j, current] = "0";
                                                else if (worksheet.Cells[i, index + indent + j].StringValue.Trim().Length == 0)
                                                    scheduleTemp[index + j, current] = "0";
                                                else
                                                {
                                                    scheduleTemp[index + j, current] = String(worksheet.Cells[i, index + indent + j].StringValue);
                                                    if (worksheet.Cells[i, index + indent + j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index + j, current] += '²';
                                                }
                                            }
                                            if (worksheet.Cells[i + 1, index + indent].Value == null
                                                && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                            {
                                                scheduleTemp[index, current + 1] = "0";
                                                scheduleTemp[index + 1, current + 1] = "0";
                                            }
                                            else
                                            {
                                                string temp1 = "", temp2 = "";
                                                if (worksheet.Cells[i + 1, index + indent].Value != null)
                                                    temp1 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                                if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                                    temp2 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                                if (temp1.Length == 0 && temp2.Length == 0)
                                                {
                                                    scheduleTemp[index, current + 1] = "0";
                                                    scheduleTemp[index + 1, current + 1] = "0";
                                                }
                                                else
                                                {
                                                    temp1 = String(temp1 + ' ' + temp2);
                                                    if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index, current + 1] = temp1 + '²';
                                                    else
                                                        scheduleTemp[index, current + 1] = temp1;
                                                    if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                                    else
                                                        scheduleTemp[index + 1, current + 1] = temp1;
                                                }
                                            }
                                        }
                                    }
                                    // 0-0-0-1
                                    else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        for (int j = 0; j < 2; ++j)
                                        {
                                            if (worksheet.Cells[i + j, index + indent].Value == null)
                                                scheduleTemp[index, current + j] = "0";
                                            else if (worksheet.Cells[i + j, index + indent].StringValue.Trim().Length == 0)
                                                scheduleTemp[index, current + j] = "0";
                                            else
                                            {
                                                scheduleTemp[index, current + j] = String(worksheet.Cells[i + j, index + indent].StringValue);
                                                if (worksheet.Cells[i + j, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current + j] += '²';
                                            }
                                        }
                                        if (worksheet.Cells[i, index + indent + 1].Value == null
                                            && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                        {
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                        }
                                        else
                                        {
                                            string temp1 = "", temp2 = "";
                                            if (worksheet.Cells[i, index + indent + 1].Value != null)
                                                temp1 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                                temp2 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                            if (temp1.Length == 0 && temp2.Length == 0)
                                            {
                                                scheduleTemp[index + 1, current] = "0";
                                                scheduleTemp[index + 1, current + 1] = "0";
                                            }
                                            else
                                            {
                                                temp1 = String(temp1 + ' ' + temp2);
                                                if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + 1, current + 1] = temp1;
                                                if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + 1, current] = temp1;
                                            }
                                        }
                                    }
                                    // 0-0-0-2
                                    else
                                    {
                                        if (worksheet.Cells[i, index + indent].Value == null)
                                            scheduleTemp[index, current] = "0";
                                        else if (worksheet.Cells[i, index + indent].StringValue.Trim().Length == 0)
                                            scheduleTemp[index, current] = "0";
                                        else
                                        {
                                            scheduleTemp[index, current] = String(worksheet.Cells[i, index + indent].StringValue);
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] += '²';
                                        }
                                        if (worksheet.Cells[i, index + indent + 1].Value == null
                                            && worksheet.Cells[i + 1, index + indent + 1].Value == null
                                            && worksheet.Cells[i + 1, index + indent].Value == null)
                                        {
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                        }
                                        else
                                        {
                                            string temp1 = "", temp2  = "", temp3 = "";
                                            if (worksheet.Cells[i, index + indent + 1].Value != null)
                                                temp1 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                                temp2 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent].Value != null)
                                                temp3 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                            if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0)
                                            {
                                                scheduleTemp[index + 1, current] = "0";
                                                scheduleTemp[index + 1, current + 1] = "0";
                                                scheduleTemp[index, current + 1] = "0";
                                            }
                                            else
                                            {
                                                temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3);
                                                if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + 1, current] = temp1;
                                                if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + 1, current + 1] = temp1;
                                                if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index, current + 1] = temp1;
                                            }
                                        }
                                    }
                                }
                                // 0-0-1
                                else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                {
                                    // 0-0-1-0
                                    if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        if (worksheet.Cells[i, index + indent].Value == null
                                            && worksheet.Cells[i, index + indent + 1].Value == null)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index + 1, current] = "0";
                                        }
                                        else
                                        {
                                            string temp1 = "", temp2 = "";
                                            if (worksheet.Cells[i, index + indent].Value != null)
                                                temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                            if (worksheet.Cells[i, index + indent + 1].Value != null)
                                                temp2 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                            if (temp1.Length == 0 && temp2.Length == 0)
                                            {
                                                scheduleTemp[index, current] = "0";
                                                scheduleTemp[index + 1, current] = "0";
                                            }
                                            else
                                            {
                                                temp1 = String(temp1 + ' ' + temp2);
                                                if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index, current] = temp1;
                                                if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + 1, current] = temp1;
                                            }
                                        }
                                        for (int j = 0; j < 2; ++j)
                                        {
                                            if (worksheet.Cells[i + 1, index + indent + j].Value == null)
                                                scheduleTemp[index + j, current + 1] = "0";
                                            else if (worksheet.Cells[i + 1, index + indent + j].StringValue.Trim().Length == 0)
                                                scheduleTemp[index + j, current + 1] = "0";
                                            else
                                            {
                                                scheduleTemp[index + j, current + 1] = String(worksheet.Cells[i + 1, index + indent + j].StringValue);
                                                if (worksheet.Cells[i + 1, index + indent + j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + j, current + 1] += '²';
                                            }
                                        }
                                    }
                                    // 0-0-1-1
                                    else
                                    {
                                        for (int j = 0; j < 2; ++j)
                                        {
                                            if (worksheet.Cells[i + j, index + indent].Value == null
                                                && worksheet.Cells[i + j, index + indent + 1].Value == null)
                                            {
                                                scheduleTemp[index, current + j] = "0";
                                                scheduleTemp[index + 1, current + j] = "0";
                                            }
                                            else
                                            {
                                                string temp1 = "", temp2 = "";
                                                if (worksheet.Cells[i + j, index + indent].Value != null)
                                                    temp1 = worksheet.Cells[i + j, index + indent].StringValue.Trim();
                                                if (worksheet.Cells[i + j, index + indent + 1].Value != null)
                                                    temp2 = worksheet.Cells[i + j, index + indent + 1].StringValue.Trim();
                                                if (temp1.Length == 0 && temp2.Length == 0)
                                                {
                                                    scheduleTemp[index, current + j] = "0";
                                                    scheduleTemp[index + 1, current + j] = "0";
                                                }
                                                else
                                                {
                                                    temp1 = String(temp1 + ' ' + temp2);
                                                    if (worksheet.Cells[i + j, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index, current + j] = temp1 + '²';
                                                    else
                                                        scheduleTemp[index, current + j] = temp1;
                                                    if (worksheet.Cells[i + j, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index + 1, current + j] = temp1 + '²';
                                                    else
                                                        scheduleTemp[index + 1, current + j] = temp1;
                                                }
                                            }
                                        }
                                    }
                                }
                                // 0-0-2
                                else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    if (worksheet.Cells[i + 1, index + indent].Value == null)
                                        scheduleTemp[index, current + 1] = "0";
                                    else if (worksheet.Cells[i + 1, index + indent].StringValue.Trim().Length == 0)
                                        scheduleTemp[index, current + 1] = "0";
                                    else
                                    {
                                        scheduleTemp[index, current + 1] = String(worksheet.Cells[i + 1, index + indent].StringValue);
                                        if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index, current + 1] += '²';
                                    }
                                    if (worksheet.Cells[i, index + indent].Value == null
                                        && worksheet.Cells[i, index + indent + 1].Value == null
                                        && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    }
                                    else
                                    {
                                        string temp1 = "", temp2  = "", temp3 = "";
                                        if (worksheet.Cells[i, index + indent].Value != null)
                                            temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i, index + indent + 1].Value != null)
                                            temp2 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                            temp3 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                        if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                        }
                                        else
                                        {
                                            temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3);
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current] = temp1;
                                            if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current + 1] = temp1;
                                        }
                                    }
                                }
                                // 0-0-3
                                else
                                {
                                    if (worksheet.Cells[i, index + indent].Value == null
                                        && worksheet.Cells[i + 1, index + indent].Value == null
                                        && worksheet.Cells[i, index + indent + 1].Value == null
                                        && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index, current + 1] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    }
                                    else
                                    {
                                        string temp1 = "", temp2  = "", temp3 = "", temp4 = "";
                                        if (worksheet.Cells[i, index + indent].Value != null)
                                            temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent].Value != null)
                                            temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i, index + indent + 1].Value != null)
                                            temp3 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                            temp4 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                        if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0 && temp4.Length == 0)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                        }
                                        else
                                        {
                                            temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3 + ' ' + temp4) + '¹';
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current + 1] = temp1;
                                            if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current + 1] = temp1;
                                        }
                                    }
                                }
                            }
                            // 0-1
                            else if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                            {
                                // 0-1-0
                                if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                {
                                    // 0-1-0-0
                                    if (worksheet.Cells[i + 1, index + indent].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                    {
                                        if (worksheet.Cells[i, index + indent].Value == null
                                            && worksheet.Cells[i + 1, index + indent].Value == null)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                        }
                                        else
                                        {
                                            string temp1 = "", temp2 = "";
                                            if (worksheet.Cells[i, index + indent].Value != null)
                                                temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent].Value != null)
                                                temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                            if (temp1.Length == 0 && temp2.Length == 0)
                                            {
                                                scheduleTemp[index, current] = "0";
                                                scheduleTemp[index, current + 1] = "0";
                                            }
                                            else
                                            {
                                                temp1 = String(temp1 + ' ' + temp2);
                                                if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index, current + 1] = temp1;
                                                if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index, current] = temp1;
                                            }
                                        }
                                        for (int j = 0; j < 2; ++j)
                                        {
                                            if (worksheet.Cells[i + j, index + indent + 1].Value == null)
                                                scheduleTemp[index + 1, current + j] = "0";
                                            else if (worksheet.Cells[i + j, index + indent + 1].StringValue.Trim().Length == 0)
                                                scheduleTemp[index + 1, current + j] = "0";
                                            else
                                            {
                                                scheduleTemp[index + 1, current + j] = String(worksheet.Cells[i + j, index + indent + 1].StringValue);
                                                if (worksheet.Cells[i + j, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current + j] += '²';
                                            }
                                        }
                                    }
                                    // 0-1-0-1
                                    else
                                    {
                                        if (worksheet.Cells[i, index + indent + 1].Value == null)
                                            scheduleTemp[index + 1, current] = "0";
                                        else if (worksheet.Cells[i, index + indent + 1].StringValue.Trim().Length == 0)
                                            scheduleTemp[index + 1, current] = "0";
                                        else
                                        {
                                            scheduleTemp[index + 1, current] = String(worksheet.Cells[i, index + indent + 1].StringValue);
                                            if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current] += '²';
                                        }
                                        if (worksheet.Cells[i, index + indent].Value == null
                                            && worksheet.Cells[i + 1, index + indent + 1].Value == null
                                            && worksheet.Cells[i + 1, index + indent].Value == null)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                        }
                                        else
                                        {
                                            string temp1 = "", temp2  = "", temp3 = "";
                                            if (worksheet.Cells[i, index + indent].Value != null)
                                                temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                                temp2 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent].Value != null)
                                                temp3 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                            if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0)
                                            {
                                                scheduleTemp[index, current] = "0";
                                                scheduleTemp[index + 1, current + 1] = "0";
                                                scheduleTemp[index, current + 1] = "0";
                                            }
                                            else
                                            {
                                                temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3);
                                                if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index, current] = temp1;
                                                if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + 1, current + 1] = temp1;
                                                if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index, current + 1] = temp1;
                                            }
                                        }
                                    }
                                }
                                // 0-1-1
                                else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    for (int j = 0; j < 2; ++j)
                                    {
                                        if (worksheet.Cells[i, index + indent + j].Value == null
                                            && worksheet.Cells[i + 1, index + indent + j].Value == null)
                                        {
                                            scheduleTemp[index + j, current] = "0";
                                            scheduleTemp[index + j, current + 1] = "0";
                                        }
                                        else
                                        {
                                            string temp1 = "", temp2 = "";
                                            if (worksheet.Cells[i, index + indent + j].Value != null)
                                                temp1 = worksheet.Cells[i, index + indent + j].StringValue.Trim();
                                            if (worksheet.Cells[i + 1, index + indent + j].Value != null)
                                                temp2 = worksheet.Cells[i + 1, index + indent + j].StringValue.Trim();
                                            if (temp1.Length == 0 && temp2.Length == 0)
                                            {
                                                scheduleTemp[index + j, current] = "0";
                                                scheduleTemp[index + j, current + 1] = "0";
                                            }
                                            else
                                            {
                                                temp1 = String(temp1 + ' ' + temp2);
                                                if (worksheet.Cells[i + 1, index + indent + j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + j, current + 1] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + j, current + 1] = temp1;
                                                if (worksheet.Cells[i, index + indent + j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index + j, current] = temp1 + '²';
                                                else
                                                    scheduleTemp[index + j, current] = temp1;
                                            }
                                        }
                                    }
                                }
                                // 0-1-2
                                else
                                {
                                    if (worksheet.Cells[i, index + indent].Value == null
                                        && worksheet.Cells[i + 1, index + indent].Value == null
                                        && worksheet.Cells[i, index + indent + 1].Value == null
                                        && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index, current + 1] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    }
                                    else
                                    {
                                        string temp1 = "", temp2  = "", temp3 = "", temp4 = "";
                                        if (worksheet.Cells[i, index + indent].Value != null)
                                            temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent].Value != null)
                                            temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i, index + indent + 1].Value != null)
                                            temp3 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                            temp4 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                        if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0 && temp4.Length == 0)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                        }
                                        else
                                        {
                                            temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3 + ' ' + temp4) + '¹';
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current + 1] = temp1;
                                            if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current + 1] = temp1;
                                        }
                                    }
                                }
                            }
                            // 0-2
                            else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                            {
                                // 0-2-0
                                if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                {
                                    if (worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                        scheduleTemp[index + 1, current + 1] = "0"; 
                                    else if (worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim().Length == 0)
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    else
                                    {
                                        scheduleTemp[index + 1, current + 1] = String(worksheet.Cells[i + 1, index + indent + 1].StringValue);
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index + 1, current + 1] += '²';
                                    }
                                    if (worksheet.Cells[i, index + indent].Value == null
                                        && worksheet.Cells[i, index + indent + 1].Value == null
                                        && worksheet.Cells[i + 1, index + indent].Value == null)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index, current + 1] = "0";
                                    }
                                    else
                                    {
                                        string temp1 = "", temp2  = "", temp3 = "";
                                        if (worksheet.Cells[i, index + indent].Value != null)
                                            temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i, index + indent + 1].Value != null)
                                            temp2 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent].Value != null)
                                            temp3 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                        if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                        }
                                        else
                                        {
                                            temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3);
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current] = temp1;
                                            if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current + 1] = temp1;
                                        }
                                    }
                                }
                                // 0-2-1
                                else
                                {
                                    if (worksheet.Cells[i, index + indent].Value == null
                                        && worksheet.Cells[i + 1, index + indent].Value == null
                                        && worksheet.Cells[i, index + indent + 1].Value == null
                                        && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index, current + 1] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    }
                                    else
                                    {
                                        string temp1 = "", temp2  = "", temp3 = "", temp4 = "";
                                        if (worksheet.Cells[i, index + indent].Value != null)
                                            temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent].Value != null)
                                            temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                        if (worksheet.Cells[i, index + indent + 1].Value != null)
                                            temp3 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                        if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                            temp4 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                        if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0 && temp4.Length == 0)
                                        {
                                            scheduleTemp[index, current] = "0";
                                            scheduleTemp[index, current + 1] = "0";
                                            scheduleTemp[index + 1, current] = "0";
                                            scheduleTemp[index + 1, current + 1] = "0";
                                        }
                                        else
                                        {
                                            temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3 + ' ' + temp4) + '¹';
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index, current + 1] = temp1;
                                            if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current] = temp1;
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                            else
                                                scheduleTemp[index + 1, current + 1] = temp1;
                                        }
                                    }
                                }
                            }
                            // 0-3
                            else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                            {
                                if (worksheet.Cells[i, index + indent].Value == null
                                    && worksheet.Cells[i + 1, index + indent].Value == null
                                    && worksheet.Cells[i, index + indent + 1].Value == null
                                    && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                {
                                    scheduleTemp[index, current] = "0";
                                    scheduleTemp[index, current + 1] = "0";
                                    scheduleTemp[index + 1, current] = "0";
                                    scheduleTemp[index + 1, current + 1] = "0";
                                }
                                else
                                {
                                    string temp1 = "", temp2  = "", temp3 = "", temp4 = "";
                                    if (worksheet.Cells[i, index + indent].Value != null)
                                        temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                    if (worksheet.Cells[i + 1, index + indent].Value != null)
                                        temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                    if (worksheet.Cells[i, index + indent + 1].Value != null)
                                        temp3 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                    if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                        temp4 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                    if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0 && temp4.Length == 0)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index, current + 1] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    }
                                    else
                                    {
                                        temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3 + ' ' + temp4) + '¹';
                                        if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index, current] = temp1 + '²';
                                        else
                                            scheduleTemp[index, current] = temp1;
                                        if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index, current + 1] = temp1 + '²';
                                        else
                                            scheduleTemp[index, current + 1] = temp1;
                                        if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index + 1, current] = temp1 + '²';
                                        else
                                            scheduleTemp[index + 1, current] = temp1;
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                        else
                                            scheduleTemp[index + 1, current + 1] = temp1;
                                    }
                                }
                            }
                            // 0-4
                            else
                            {
                                if (worksheet.Cells[i, index + indent].Value == null
                                    && worksheet.Cells[i + 1, index + indent].Value == null
                                    && worksheet.Cells[i, index + indent + 1].Value == null
                                    && worksheet.Cells[i + 1, index + indent + 1].Value == null)
                                {
                                    scheduleTemp[index, current] = "0";
                                    scheduleTemp[index, current + 1] = "0";
                                    scheduleTemp[index + 1, current] = "0";
                                    scheduleTemp[index + 1, current + 1] = "0";
                                }
                                else
                                {
                                    string temp1 = "", temp2  = "", temp3 = "", temp4 = "";
                                    if (worksheet.Cells[i, index + indent].Value != null)
                                        temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                    if (worksheet.Cells[i + 1, index + indent].Value != null)
                                        temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                    if (worksheet.Cells[i, index + indent + 1].Value != null)
                                        temp3 = worksheet.Cells[i, index + indent + 1].StringValue.Trim();
                                    if (worksheet.Cells[i + 1, index + indent + 1].Value != null)
                                        temp4 = worksheet.Cells[i + 1, index + indent + 1].StringValue.Trim();
                                    if (temp1.Length == 0 && temp2.Length == 0 && temp3.Length == 0 && temp4.Length == 0)
                                    {
                                        scheduleTemp[index, current] = "0";
                                        scheduleTemp[index, current + 1] = "0";
                                        scheduleTemp[index + 1, current] = "0";
                                        scheduleTemp[index + 1, current + 1] = "0";
                                    }
                                    else
                                    {
                                        temp1 = String(temp1 + ' ' + temp2 + ' ' + temp3 + ' ' + temp4);
                                        if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index, current] = temp1 + '²';
                                        else
                                            scheduleTemp[index, current] = temp1;
                                        if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index, current + 1] = temp1 + '²';
                                        else
                                            scheduleTemp[index, current + 1] = temp1;
                                        if (worksheet.Cells[i, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index + 1, current] = temp1 + '²';
                                        else
                                            scheduleTemp[index + 1, current] = temp1;
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                            scheduleTemp[index + 1, current + 1] = temp1 + '²';
                                        else
                                            scheduleTemp[index + 1, current + 1] = temp1;
                                    }
                                }
                            }
                        }
                        // 1
                        else
                        {
                            // 1-0
                            if (scheduleTemp[index, current] != null)
                            {
                                // 1-0-0
                                if (scheduleTemp[index, current + 1] != null)
                                {
                                    // 1-0-0-0
                                    if (scheduleTemp[index + 1, current] != null)
                                    {
                                        string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                        int count = 0;
                                        int k = index; // + 1 переносим внутрь цикла
                                        do
                                        {
                                            ++k;
                                            if (worksheet.Cells[i + 1, k + indent].Value != null)
                                            {
                                                if (worksheet.Cells[i + 1, k + indent].StringValue.Trim().Length != 0)
                                                {
                                                    temp[count] = worksheet.Cells[i + 1, k + indent].StringValue;
                                                    bool flag = true;
                                                    for (int j = 0; j < count; ++j)
                                                    {
                                                        if (worksheet.Cells[i + 1, k + indent].StringValue == temp[j])
                                                        {
                                                            flag = false;
                                                            break;
                                                        }
                                                    }
                                                    if (flag)
                                                    {
                                                        ++count;
                                                    }
                                                }
                                            }
                                        } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        k = index;
                                        if (count == 0)
                                        {
                                            do
                                            {
                                                ++k;
                                                scheduleTemp[k, current + 1] = "0";
                                            } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                        else
                                        {
                                            for (int l = 1; l < count; ++l)
                                                temp[0] += ' ' + temp[l];
                                            temp[0] = String(temp[0]);
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                temp[0] += " · Л";
                                            do
                                            {
                                                ++k;
                                                if (worksheet.Cells[i + 1, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[k, current + 1] = temp[0] + '²';
                                                else
                                                    scheduleTemp[k, current + 1] = temp[0];
                                            } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                    }
                                    // 1-0-0-1
                                    else if (scheduleTemp[index + 1, current + 1] != null)
                                    {
                                        string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                        int count = 0;
                                        int k = index; // + 1 переносим внутрь цикла
                                        do
                                        {
                                            ++k;
                                            if (worksheet.Cells[i, k + indent].Value != null)
                                            {
                                                if (worksheet.Cells[i, k + indent].StringValue.Trim().Length != 0)
                                                {
                                                    temp[count] = worksheet.Cells[i, k + indent].StringValue;
                                                    bool flag = true;
                                                    for (int j = 0; j < count; ++j)
                                                    {
                                                        if (worksheet.Cells[i, k + indent].StringValue == temp[j])
                                                        {
                                                            flag = false;
                                                            break;
                                                        }
                                                    }
                                                    if (flag)
                                                    {
                                                        ++count;
                                                    }
                                                }
                                            }
                                        } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        k = index;
                                        if (count == 0)
                                        {
                                            do
                                            {
                                                ++k;
                                                scheduleTemp[k, current] = "0";
                                            } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                        else
                                        {
                                            for (int l = 1; l < count; ++l)
                                                temp[0] += ' ' + temp[l];
                                            temp[0] = String(temp[0]);
                                            if (worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                temp[0] += " · Л";
                                            do
                                            {
                                                ++k;
                                                if (worksheet.Cells[i, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[k, current] = temp[0] + '²';
                                                else
                                                    scheduleTemp[k, current] = temp[0];
                                            } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                    }
                                    // 1-0-0-2
                                    else
                                    {
                                        // 1-0-0-2-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index; // + 1 переносим внутрь цикла
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + j, k + indent].Value != null)
                                                    {
                                                        if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                        {
                                                            temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                            bool flag = true;
                                                            for (int l = 0; l < count; ++l)
                                                            {
                                                                if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                {
                                                                    flag = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                ++count;
                                                            }
                                                        }
                                                    }
                                                } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                k = index;
                                                if (count == 0)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        scheduleTemp[k, current + j] = "0";
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]);
                                                    if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                        temp[0] += " · Л";
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[k, current + j] = temp[0] + '²';
                                                        else
                                                            scheduleTemp[k, current + j] = temp[0];
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                            }
                                        }
                                        // 1-0-0-2-1
                                        else
                                        {
                                            string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                            int count = 0;
                                            int k = index; // + 1 переносим внутрь цикла
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + j, k + indent].Value != null)
                                                    {
                                                        if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                        {
                                                            temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                            bool flag = true;
                                                            for (int l = 0; l < count; ++l)
                                                            {
                                                                if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                {
                                                                    flag = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                ++count;
                                                            }
                                                        }
                                                    }
                                                } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                k = index;
                                            }
                                            if (count == 0)
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        scheduleTemp[k, current + j] = "0";
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                }
                                            }
                                            else
                                            {
                                                for (int l = 1; l < count; ++l)
                                                    temp[0] += ' ' + temp[l];
                                                temp[0] = String(temp[0]);
                                                if (worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None
                                                    || worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                    temp[0] += " · Л";
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    k = index;
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[k, current + j] = temp[0] + '²';
                                                        else
                                                            scheduleTemp[k, current + j] = temp[0];
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                            }
                                        }
                                    }
                                }
                                // 1-0-1
                                else
                                {
                                    // 1-0-1-0
                                    if (scheduleTemp[index + 1, current] != null)
                                    {
                                        // 1-0-1-0-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            if (worksheet.Cells[i + 1, index + indent].Value == null)
                                                scheduleTemp[index, current + 1] = "0";
                                            else if (worksheet.Cells[i + 1, index + indent].StringValue.Trim().Length == 0)
                                                scheduleTemp[index, current + 1] = "0";
                                            else
                                            {
                                                scheduleTemp[index, current + 1] = String(worksheet.Cells[i + 1, index + indent].StringValue);
                                                if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current + 1] += '²';
                                            }
                                            string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                            int count = 0;
                                            int k = index; // + 1 переносим внутрь цикла
                                            do
                                            {
                                                ++k;
                                                if (worksheet.Cells[i + 1, k + indent].Value != null)
                                                {
                                                    if (worksheet.Cells[i + 1, k + indent].StringValue.Trim().Length != 0)
                                                    {
                                                        temp[count] = worksheet.Cells[i + 1, k + indent].StringValue;
                                                        bool flag = true;
                                                        for (int j = 0; j < count; ++j)
                                                        {
                                                            if (worksheet.Cells[i + 1, k + indent].StringValue == temp[j])
                                                            {
                                                                flag = false;
                                                                break;
                                                            }
                                                        }
                                                        if (flag)
                                                        {
                                                            ++count;
                                                        }
                                                    }
                                                }
                                            } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                            k = index;
                                            if (count == 0)
                                            {
                                                do
                                                {
                                                    ++k;
                                                    scheduleTemp[k, current + 1] = "0";
                                                } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                            }
                                            else
                                            {
                                                for (int l = 1; l < count; ++l)
                                                    temp[0] += ' ' + temp[l];
                                                temp[0] = String(temp[0]);
                                                if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                    temp[0] += " · Л";
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + 1, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[k, current + 1] = temp[0] + '²';
                                                    else
                                                        scheduleTemp[k, current + 1] = temp[0];
                                                } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                            }
                                        }
                                        // 1-0-1-0-1
                                        else
                                        {
                                            string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                            int count = 0;
                                            int k = index - 1; // - 1 так как ++k
                                            do
                                            {
                                                ++k;
                                                if (worksheet.Cells[i + 1, k + indent].Value != null)
                                                {
                                                    if (worksheet.Cells[i + 1, k + indent].StringValue.Trim().Length != 0)
                                                    {
                                                        temp[count] = worksheet.Cells[i + 1, k + indent].StringValue;
                                                        bool flag = true;
                                                        for (int j = 0; j < count; ++j)
                                                        {
                                                            if (worksheet.Cells[i + 1, k + indent].StringValue == temp[j])
                                                            {
                                                                flag = false;
                                                                break;
                                                            }
                                                        }
                                                        if (flag)
                                                        {
                                                            ++count;
                                                        }
                                                    }
                                                }
                                            } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                            k = index - 1;
                                            if (count == 0)
                                            {
                                                do
                                                {
                                                    ++k;
                                                    scheduleTemp[k, current + 1] = "0";
                                                } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                            }
                                            else
                                            {
                                                for (int l = 1; l < count; ++l)
                                                    temp[0] += ' ' + temp[l];
                                                temp[0] = String(temp[0]);
                                                if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                    temp[0] += " · Л";
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + 1, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[k, current + 1] = temp[0] + '²';
                                                    else
                                                        scheduleTemp[k, current + 1] = temp[0];
                                                } while (worksheet.Cells[i + 1, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                            }
                                        }
                                    }
                                    // 1-0-1-1
                                    else
                                    {
                                        // 1-0-1-1-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-0-1-1-0-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                if (worksheet.Cells[i + 1, index + indent].Value == null)
                                                    scheduleTemp[index, current + 1] = "0";
                                                else if (worksheet.Cells[i + 1, index + indent].StringValue.Trim().Length == 0)
                                                    scheduleTemp[index, current + 1] = "0";
                                                else
                                                {
                                                    scheduleTemp[index, current + 1] = String(worksheet.Cells[i + 1, index + indent].StringValue);
                                                    if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index, current + 1] += '²';
                                                }
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index; // + 1 переносим внутрь цикла
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-0-1-1-0-1
                                            else
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index; // + 1 переносим внутрь цикла
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent - j].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent - j].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent - j].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent - j].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent - j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k - j, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent - j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent - j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k - j, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k - j, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent - j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                        // 1-0-1-1-1
                                        else if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                        {
                                            if (worksheet.Cells[i + 1, index + indent].Value == null)
                                                scheduleTemp[index, current + 1] = "0";
                                            else if (worksheet.Cells[i + 1, index + indent].StringValue.Trim().Length == 0)
                                                scheduleTemp[index, current + 1] = "0";
                                            else
                                            {
                                                scheduleTemp[index, current + 1] = String(worksheet.Cells[i + 1, index + indent].StringValue);
                                                if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current + 1] += '²';
                                            }
                                            string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                            int count = 0;
                                            int k = index; // + 1 переносим внутрь цикла
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + j, k + indent].Value != null)
                                                    {
                                                        if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                        {
                                                            temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                            bool flag = true;
                                                            for (int l = 0; l < count; ++l)
                                                            {
                                                                if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                {
                                                                    flag = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                ++count;
                                                            }
                                                        }
                                                    }
                                                } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                k = index;
                                            }
                                            if (count == 0)
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        scheduleTemp[k, current + j] = "0";
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                }
                                            }
                                            else
                                            {
                                                for (int l = 1; l < count; ++l)
                                                    temp[0] += ' ' + temp[l];
                                                temp[0] = String(temp[0]);
                                                if (worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None
                                                    || worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                    temp[0] += " · Л";
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    k = index;
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[k, current + j] = temp[0] + '²';
                                                        else
                                                            scheduleTemp[k, current + j] = temp[0];
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                            }
                                        }
                                        // 1-0-1-1-2
                                        else
                                        {
                                            // todo: high index 3
                                        }
                                    }
                                }
                            }
                            // 1-1
                            else if (scheduleTemp[index, current + 1] != null)
                            {
                                // 1-1-0
                                if (scheduleTemp[index + 1, current + 1] != null)
                                {
                                    // 1-1-0-0
                                    if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        if (worksheet.Cells[i, index + indent].Value == null)
                                            scheduleTemp[index, current] = "0";
                                        else if (worksheet.Cells[i, index + indent].StringValue.Trim().Length == 0)
                                            scheduleTemp[index, current] = "0";
                                        else
                                        {
                                            scheduleTemp[index, current] = String(worksheet.Cells[i, index + indent].StringValue);
                                            if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                scheduleTemp[index, current] += '²';
                                        }
                                        string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                        int count = 0;
                                        int k = index; // + 1 переносим внутрь цикла
                                        do
                                        {
                                            ++k;
                                            if (worksheet.Cells[i, k + indent].Value != null)
                                            {
                                                if (worksheet.Cells[i, k + indent].StringValue.Trim().Length != 0)
                                                {
                                                    temp[count] = worksheet.Cells[i, k + indent].StringValue;
                                                    bool flag = true;
                                                    for (int j = 0; j < count; ++j)
                                                    {
                                                        if (worksheet.Cells[i, k + indent].StringValue == temp[j])
                                                        {
                                                            flag = false;
                                                            break;
                                                        }
                                                    }
                                                    if (flag)
                                                    {
                                                        ++count;
                                                    }
                                                }
                                            }
                                        } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        k = index;
                                        if (count == 0)
                                        {
                                            do
                                            {
                                                ++k;
                                                scheduleTemp[k, current] = "0";
                                            } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                        else
                                        {
                                            for (int j = 1; j < count; ++j)
                                                temp[0] += ' ' + temp[j];
                                            temp[0] = String(temp[0]);
                                            if (worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                temp[0] += " · Л";
                                            do
                                            {
                                                ++k;
                                                if (worksheet.Cells[i, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[k, current] = temp[0] + '²';
                                                else
                                                    scheduleTemp[k, current] = temp[0];
                                            } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                    }
                                    // 1-1-0-1
                                    else
                                    {
                                        string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                        int count = 0;
                                        int k = index - 1; // - 1 так как ++k
                                        do
                                        {
                                            ++k;
                                            if (worksheet.Cells[i, k + indent].Value != null)
                                            {
                                                if (worksheet.Cells[i, k + indent].StringValue.Trim().Length != 0)
                                                {
                                                    temp[count] = worksheet.Cells[i, k + indent].StringValue;
                                                    bool flag = true;
                                                    for (int j = 0; j < count; ++j)
                                                    {
                                                        if (worksheet.Cells[i, k + indent].StringValue == temp[j])
                                                        {
                                                            flag = false;
                                                            break;
                                                        }
                                                    }
                                                    if (flag)
                                                    {
                                                        ++count;
                                                    }
                                                }
                                            }
                                        } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        k = index - 1;
                                        if (count == 0)
                                        {
                                            do
                                            {
                                                ++k;
                                                scheduleTemp[k, current] = "0";
                                            } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                        else
                                        {
                                            for (int j = 1; j < count; ++j)
                                                temp[0] += ' ' + temp[j];
                                            temp[0] = String(temp[0]);
                                            if (worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                temp[0] += " · Л";
                                            do
                                            {
                                                ++k;
                                                if (worksheet.Cells[i, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[k, current] = temp[0] + '²';
                                                else
                                                    scheduleTemp[k, current] = temp[0];
                                            } while (worksheet.Cells[i, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                        }
                                    }
                                }
                                // 1-1-1
                                else
                                {
                                    // 1-1-1-0
                                    if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 1-1-1-0-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            if (worksheet.Cells[i, index + indent].Value == null)
                                                scheduleTemp[index, current] = "0";
                                            else if (worksheet.Cells[i, index + indent].StringValue.Trim().Length == 0)
                                                scheduleTemp[index, current] = "0";
                                            else
                                            {
                                                scheduleTemp[index, current] = String(worksheet.Cells[i, index + indent].StringValue);
                                                if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current] += '²';
                                            }
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index; // + 1 переносим внутрь цикла
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + j, k + indent].Value != null)
                                                    {
                                                        if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                        {
                                                            temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                            bool flag = true;
                                                            for (int l = 0; l < count; ++l)
                                                            {
                                                                if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                {
                                                                    flag = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                ++count;
                                                            }
                                                        }
                                                    }
                                                } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                k = index;
                                                if (count == 0)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        scheduleTemp[k, current + j] = "0";
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]);
                                                    if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                        temp[0] += " · Л";
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[k, current + j] = temp[0] + '²';
                                                        else
                                                            scheduleTemp[k, current + j] = temp[0];
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                            }
                                        }
                                        // 1-1-1-0-1
                                        else
                                        {
                                            if (worksheet.Cells[i, index + indent].Value == null)
                                                scheduleTemp[index, current] = "0";
                                            else if (worksheet.Cells[i, index + indent].StringValue.Trim().Length == 0)
                                                scheduleTemp[index, current] = "0";
                                            else
                                            {
                                                scheduleTemp[index, current] = String(worksheet.Cells[i, index + indent].StringValue);
                                                if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                    scheduleTemp[index, current] += '²';
                                            }
                                            string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                            int count = 0;
                                            int k = index; // + 1 переносим внутрь цикла
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + j, k + indent].Value != null)
                                                    {
                                                        if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                        {
                                                            temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                            bool flag = true;
                                                            for (int l = 0; l < count; ++l)
                                                            {
                                                                if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                {
                                                                    flag = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                ++count;
                                                            }
                                                        }
                                                    }
                                                } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                k = index;
                                            }
                                            if (count == 0)
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        scheduleTemp[k, current + j] = "0";
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                }
                                            }
                                            else
                                            {
                                                for (int l = 1; l < count; ++l)
                                                    temp[0] += ' ' + temp[l];
                                                temp[0] = String(temp[0]);
                                                if (worksheet.Cells[i, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None
                                                    || worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                    temp[0] += " · Л";
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    k = index;
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[k, current + j] = temp[0] + '²';
                                                        else
                                                            scheduleTemp[k, current + j] = temp[0];
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                            }
                                        }
                                    }
                                    // 1-1-1-1
                                    else
                                    {
                                        // 1-1-1-1-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            for (int j = 0; j < 2; ++j)
                                            {
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index - 1; // + 1 переносим внутрь цикла
                                                do
                                                {
                                                    ++k;
                                                    if (worksheet.Cells[i + j, k + indent + j].Value != null)
                                                    {
                                                        if (worksheet.Cells[i + j, k + indent + j].StringValue.Trim().Length != 0)
                                                        {
                                                            temp[count] = worksheet.Cells[i + j, k + indent + j].StringValue;
                                                            bool flag = true;
                                                            for (int l = 0; l < count; ++l)
                                                            {
                                                                if (worksheet.Cells[i + j, k + indent + j].StringValue == temp[l])
                                                                {
                                                                    flag = false;
                                                                    break;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                ++count;
                                                            }
                                                        }
                                                    }
                                                } while (worksheet.Cells[i + j, k + indent + j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                k = index - 1;
                                                if (count == 0)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        scheduleTemp[k + j, current + j] = "0";
                                                    } while (worksheet.Cells[i + j, k + indent + j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]);
                                                    if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                        temp[0] += " · Л";
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent + j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[k + j, current + j] = temp[0] + '²';
                                                        else
                                                            scheduleTemp[k + j, current + j] = temp[0];
                                                    } while (worksheet.Cells[i + j, k + indent + j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                }
                                            }
                                        }
                                        // 1-1-1-1-1
                                        else
                                        {
                                            // todo: high index 3
                                        }
                                    }
                                }
                            }
                            // 1-2
                            else
                            {
                                // 1-2-0
                                if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Bottom].LineStyle != LineStyle.None)
                                {
                                    // 1-2-0-0
                                    if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 1-2-0-0-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-0-0-0-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    if (worksheet.Cells[i + j, index + indent].Value == null)
                                                        scheduleTemp[index, current + j] = "0";
                                                    else if (worksheet.Cells[i + j, index + indent].StringValue.Trim().Length == 0)
                                                        scheduleTemp[index, current + j] = "0";
                                                    else
                                                    {
                                                        scheduleTemp[index, current + j] = String(worksheet.Cells[i + j, index + indent].StringValue);
                                                        if (worksheet.Cells[i + j, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index, current + j] += '²';
                                                    }
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index; // + 1 переносим внутрь цикла
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-2-0-0-0-1
                                            else
                                            {
                                                if (worksheet.Cells[i, index + indent].Value == null)
                                                    scheduleTemp[index, current] = "0";
                                                else if (worksheet.Cells[i, index + indent].StringValue.Trim().Length == 0)
                                                    scheduleTemp[index, current] = "0";
                                                else
                                                {
                                                    scheduleTemp[index, current] = String(worksheet.Cells[i, index + indent].StringValue);
                                                    if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index, current] += '²';
                                                }
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index; // + 1 переносим внутрь цикла
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent - j].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent - j].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent - j].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent - j].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent - j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k - j, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent - j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent - j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k - j, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k - j, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent - j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                        // 1-2-0-0-1
                                        else
                                        {
                                            // 1-2-0-0-1-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    if (worksheet.Cells[i + j, index + indent].Value == null)
                                                        scheduleTemp[index, current + j] = "0";
                                                    else if (worksheet.Cells[i + j, index + indent].StringValue.Trim().Length == 0)
                                                        scheduleTemp[index, current + j] = "0";
                                                    else
                                                    {
                                                        scheduleTemp[index, current + j] = String(worksheet.Cells[i + j, index + indent].StringValue);
                                                        if (worksheet.Cells[i + j, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index, current + j] += '²';
                                                    }
                                                }
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л";
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-2-0-0-1-1
                                            else
                                            {
                                                // todo: high index 3
                                            }
                                        }
                                    }
                                    // 1-2-0-1
                                    else
                                    {
                                        // 1-2-0-1-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-0-1-0-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                if (worksheet.Cells[i + 1, index + indent].Value == null)
                                                    scheduleTemp[index, current + 1] = "0";
                                                else if (worksheet.Cells[i + 1, index + indent].StringValue.Trim().Length == 0)
                                                    scheduleTemp[index, current + 1] = "0";
                                                else
                                                {
                                                    scheduleTemp[index, current + 1] = String(worksheet.Cells[i + 1, index + indent].StringValue);
                                                    if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                        scheduleTemp[index, current + 1] += '²';
                                                }
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index - 1; // + 1 переносим внутрь цикла
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent + j].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent + j].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent + j].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent + j].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent + j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k + j, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent + j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent + j].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k + j, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k + j, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent + j].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-2-0-1-0-1
                                            else
                                            {
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index - 1; // - 1 так как ++k
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                        // 1-2-0-1-1
                                        else
                                        {
                                            // 1-2-0-1-1-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                // todo: high index 3
                                            }
                                            // 1-2-0-1-1-1
                                            else
                                            {
                                                //worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Bottom].LineStyle = LineStyle.None;
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index - 1; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л¹";
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index - 1;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                // 1-2-1
                                else
                                {
                                    // 1-2-1-0
                                    if (worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle != LineStyle.None)
                                    {
                                        // 1-2-1-0-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-1-0-0-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                if (worksheet.Cells[i, index + indent].Value == null
                                                    && worksheet.Cells[i + 1, index + indent].Value == null)
                                                {
                                                    scheduleTemp[index, current] = "0";
                                                    scheduleTemp[index, current + 1] = "0";
                                                }
                                                else
                                                {
                                                    string temp1 = "", temp2 = "";
                                                    if (worksheet.Cells[i, index + indent].Value != null)
                                                        temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                                    if (worksheet.Cells[i + 1, index + indent].Value != null)
                                                        temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                                    if (temp1.Length == 0 && temp2.Length == 0)
                                                    {
                                                        scheduleTemp[index, current] = "0";
                                                        scheduleTemp[index, current + 1] = "0";
                                                    }
                                                    else
                                                    {
                                                        temp1 = String(temp1 + ' ' + temp2);
                                                        if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index, current] = temp1 + '²';
                                                        else
                                                            scheduleTemp[index, current] = temp1;
                                                        if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index, current + 1] = temp1 + '²';
                                                        else
                                                            scheduleTemp[index, current + 1] = temp1;
                                                    }
                                                }
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                    int count = 0;
                                                    int k = index; // + 1 переносим внутрь цикла
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                    if (count == 0)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                    else
                                                    {
                                                        for (int l = 1; l < count; ++l)
                                                            temp[0] += ' ' + temp[l];
                                                        temp[0] = String(temp[0]);
                                                        if (worksheet.Cells[i + j, index + indent + 1].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None)
                                                            temp[0] += " · Л";
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-2-1-0-0-1
                                            else
                                            {
                                                // todo: high index 3
                                            }
                                        }
                                        // 1-2-1-0-1
                                        else
                                        {
                                            // 1-2-1-0-1-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                if (worksheet.Cells[i, index + indent].Value == null
                                                    && worksheet.Cells[i + 1, index + indent].Value == null)
                                                {
                                                    scheduleTemp[index, current] = "0";
                                                    scheduleTemp[index, current + 1] = "0";
                                                }
                                                else
                                                {
                                                    string temp1 = "", temp2 = "";
                                                    if (worksheet.Cells[i, index + indent].Value != null)
                                                        temp1 = worksheet.Cells[i, index + indent].StringValue.Trim();
                                                    if (worksheet.Cells[i + 1, index + indent].Value != null)
                                                        temp2 = worksheet.Cells[i + 1, index + indent].StringValue.Trim();
                                                    if (temp1.Length == 0 && temp2.Length == 0)
                                                    {
                                                        scheduleTemp[index, current] = "0";
                                                        scheduleTemp[index, current + 1] = "0";
                                                    }
                                                    else
                                                    {
                                                        temp1 = String(temp1 + ' ' + temp2);
                                                        if (worksheet.Cells[i + 1, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index, current + 1] = temp1 + '²';
                                                        else
                                                            scheduleTemp[index, current + 1] = temp1;
                                                        if (worksheet.Cells[i, index + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                            scheduleTemp[index, current] = temp1 + '²';
                                                        else
                                                            scheduleTemp[index, current] = temp1;
                                                    }
                                                }
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л";
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-2-1-0-1-1
                                            else
                                            {
                                                worksheet.Cells[i, index + indent].Style.Borders[IndividualBorder.Right].LineStyle = LineStyle.None;
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index - 1; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л¹";
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index - 1;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // 1-2-1-1
                                    else
                                    {
                                        // 1-2-1-1-0
                                        if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle != LineStyle.None)
                                        {
                                            // 1-2-1-1-0-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                 // todo: high index 3
                                            }
                                            // 1-2-1-1-0-1
                                            else
                                            {
                                                //worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Top].LineStyle = LineStyle.None;
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index - 1; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л¹";
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index - 1;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                        // 1-2-1-1-1
                                        else
                                        {
                                            // 1-2-1-1-1-0
                                            if (worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle != LineStyle.None)
                                            {
                                                worksheet.Cells[i + 1, index + indent + 1].Style.Borders[IndividualBorder.Left].LineStyle = LineStyle.None;
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index - 1; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л¹";
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index - 1;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                            // 1-2-1-1-1-1
                                            else
                                            {
                                                string[] temp = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                                                int count = 0;
                                                int k = index - 1; // + 1 переносим внутрь цикла
                                                for (int j = 0; j < 2; ++j)
                                                {
                                                    do
                                                    {
                                                        ++k;
                                                        if (worksheet.Cells[i + j, k + indent].Value != null)
                                                        {
                                                            if (worksheet.Cells[i + j, k + indent].StringValue.Trim().Length != 0)
                                                            {
                                                                temp[count] = worksheet.Cells[i + j, k + indent].StringValue;
                                                                bool flag = true;
                                                                for (int l = 0; l < count; ++l)
                                                                {
                                                                    if (worksheet.Cells[i + j, k + indent].StringValue == temp[l])
                                                                    {
                                                                        flag = false;
                                                                        break;
                                                                    }
                                                                }
                                                                if (flag)
                                                                {
                                                                    ++count;
                                                                }
                                                            }
                                                        }
                                                    } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    k = index - 1;
                                                }
                                                if (count == 0)
                                                {
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        do
                                                        {
                                                            ++k;
                                                            scheduleTemp[k, current + j] = "0";
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                        k = index - 1;
                                                    }
                                                }
                                                else
                                                {
                                                    for (int l = 1; l < count; ++l)
                                                        temp[0] += ' ' + temp[l];
                                                    temp[0] = String(temp[0]) + " · Л"; // ! обратить внимание
                                                    for (int j = 0; j < 2; ++j)
                                                    {
                                                        k = index - 1;
                                                        do
                                                        {
                                                            ++k;
                                                            if (worksheet.Cells[i + j, k + indent].Style.FillPattern.PatternStyle == FillPatternStyle.None)
                                                                scheduleTemp[k, current + j] = temp[0] + '²';
                                                            else
                                                                scheduleTemp[k, current + j] = temp[0];
                                                        } while (worksheet.Cells[i + j, k + indent].Style.Borders[IndividualBorder.Right].LineStyle == LineStyle.None);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // в случае ошибки
                    {
                        for (int j = 0; j < 2; ++j)
                        {
                            for (int k = 0; k < 2; ++k)
                            {
                                if (scheduleTemp[index + j, current + k] == null)
                                    scheduleTemp[index + j, current + k] = "N0ERROR";
                            }
                        }
                        // todo: в случае неограниченности сверху или снизу
                    }
                }
                // вырезан алгоритм 1
                bool[] same = { true, true };
                lock (Glob.locker)
                {
                    for (int p = 0; p < 98; ++p)
                        if (scheduleTemp[index, p] != Glob.schedule[course, index, p])
                        {
                            same[0] = false;
                            break;
                        }
                    for (int p = 0; p < 98; ++p)
                        if (scheduleTemp[index + 1, p] != Glob.schedule[course, index + 1, p])
                        {
                            same[1] = false;
                            break;
                        }
                }
                for (int i = 0; i < 2; ++i)
                {
                    if (same[i])
                    {
                        if (sendUpdates)
                            Distribution.ToGroupSubgroup(group, (i + 1).ToString(), "Для Вас изменений нет");
                    }
                    else
                    {
                        lock (Glob.locker)
                        {
                            for (int p = 0; p < 98; ++p)
                                Glob.schedule[course, index + i, p] = scheduleTemp[index + i, p];
                        }
                        FileStream file = new FileStream(
                            Const.path_schedule + (course + 1).ToString() + "_" + (index + i).ToString() + ".txt",
                            FileMode.OpenOrCreate);
                        file.Close();
                        StreamWriter fileWrite = new StreamWriter(
                            Const.path_schedule + (course + 1).ToString() + "_" + (index + i).ToString() + ".txt",
                            false,
                            System.Text.Encoding.Default);
                        for (int p = 0; p < 98; ++p) //запись новой информации в файл с ссылками и датами
                            fileWrite.WriteLine(scheduleTemp[index + i, p]);
                        fileWrite.Close();
                        Process.Schedule(course, index + i);

                        sendScheduleUpdateGroups[course, 0, sendScheduleUpdateGroupsCount] = course;
                        sendScheduleUpdateGroups[course, 1, sendScheduleUpdateGroupsCount] = index + i;
                        ++sendScheduleUpdateGroupsCount;
                    }
                }
                index += 2; //следующая группа
            }
            sendScheduleUpdateGroups[course, 0, 100] = sendScheduleUpdateGroupsCount;
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E]    -> Обработка расписания"); // log
            return sendScheduleUpdateGroups;
        }
        public static string String(string parsing) // Разбор на фио, аудиоторию и предмет
        {
            if (parsing.Trim() == null)
                return "0";
            Regex regexLectureHall = new Regex("[0-9]+([/]{1,2}[0-9]+)?( ?[(]{1}[0-9]+[)]{1})?( {1}[(]{1}[0-9]+ {1}корпус[)]{1})?");
            Regex regexFullName = new Regex("[А-Я]{1}[а-я]+([-]{1}[А-Я]{1}[а-я]+)? {1}[А-Я]{1}[.]{1}([А-Я]{1}[.]?)?");
            MatchCollection matches;
            string subject, fullName = null, lectureHall = null;
            bool[] found = { false, false }; // Нашли: ФИО, аудиторию
            // Чистим строку
            while (parsing.Contains("  "))
                parsing = parsing.Replace("  ", " ");
            // Ищем ФИО
            matches = regexFullName.Matches(parsing);
            if (matches.Count == 1)
            {
                fullName = matches[0].ToString();
                found[0] = true;
                parsing = parsing.Remove(matches[0].Index, matches[0].Length);
                while (parsing.Contains("  "))
                    parsing = parsing.Replace("  ", " ");
                parsing = parsing.Trim();
            }
            else if (matches.Count == 0)
            {
                int indexTemp;
                for (int k = 0; k < Glob.full_name_count; ++k)
                {
                    indexTemp = parsing.IndexOf(Glob.full_name[k]);
                    if (indexTemp != -1)
                    {
                        fullName = Glob.full_name[k];
                        found[0] = true;
                        parsing = parsing.Remove(indexTemp, Glob.full_name[k].Length);
                        while (parsing.Contains("  "))
                            parsing = parsing.Replace("  ", " ");
                        parsing = parsing.Trim();
                        break;
                    }
                }
            }
            else if (matches.Count >= 2)
            {
                for (int j = 0; j < Glob.double_optionally_subject_count; ++j)
                {
                    if (parsing == Glob.double_optionally_subject[0, j])
                    {
                        return "F2" + Glob.double_optionally_subject[1, j];
                    }
                }
            }
            // Ищем аудиторию
            matches = regexLectureHall.Matches(parsing);
            if (matches.Count != 0)
            {
                if (matches.Count == 1)
                {
                    lectureHall = matches[0].ToString();
                    found[1] = true;
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
                        found[1] = true;
                        parsing = parsing.Remove(matches[k].Index, matches[k].Length);
                        while (parsing.Contains("  "))
                            parsing = parsing.Replace("  ", " ");
                        parsing = parsing.Trim();
                        break;
                    }
                }
            }
            // Выводы: F - полное, N - неполное, n - количество аргументов
            if (!found[1])
            {
                if (parsing.ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА"))
                    return "F1" + "Военная подготовка";
                else if (parsing.ToUpper().Contains("ФИЗИЧЕСКАЯ КУЛЬТУРА"))
                        return "F1" + "Физическая культура";
                else if (found[0])
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
                    return "N2" + parsing + " · " + fullName;
                }
                else
                {
                    return "N0" + parsing;
                }
            }
            else if (found[0])
            {
                subject = parsing;
                bool check = false;
                for (int k = 0; k < Glob.acronym_to_phrase_count; ++k)
                {
                    if (subject == Glob.acronym_to_phrase[0, k])
                    {
                        subject = Glob.acronym_to_phrase[1, k];
                        check = true;
                        break;
                    }
                }
                // Если все капсом и более одного слова, заглавной остается только первая буква
                if (!check && (parsing.Contains(' ') || parsing.Length > 4))
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
                return "F3" + subject + " · " + fullName + " · " + lectureHall;
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
                return "N2" + parsing + " · " + lectureHall;
            }
        }
    }
}
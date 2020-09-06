using System;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using Schedulebot.Schedule;

namespace Schedulebot.Drawing
{
    public struct DrawingStandartScheduleInfo
    {
        public ScheduleWeek[] weeks;
        public string group;
        public int subgroup;
        public string date;
        public string vkGroupUrl;
    }
    public struct DrawingDayScheduleInfo
    {
        public ScheduleDay day;
        public int dayOfWeek;
        public int weekProperties; // 0 - верхняя, 1 - нижняя
        public string group;
        public string subgroup;
        public string date;
        public string vkGroupUrl;
    }
    public static class DrawingSchedule
    {
        public static class StandartSchedule
        {
            const string c_noOneLecture = "____";
            public static byte[] Draw(DrawingStandartScheduleInfo drawingScheduleInfo) // Обработка расписания для рассылки
            {
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Обрабока расписания для рассылки " + course + " " + number);
                Bitmap temp = new Bitmap(Image.width, Image.height);
                temp.SetResolution(96.0F, 96.0F);
                System.Drawing.Image image = temp;
                Graphics graphics = Graphics.FromImage(image);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                // Заливаем фон
                graphics.Clear(backgroundColor);
                int pos = Border.size; // y
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                // Рисуем шапку
                string header = drawingScheduleInfo.group + " (" + drawingScheduleInfo.subgroup + ") " + drawingScheduleInfo.date;
                graphics.DrawString(
                    header,
                    headerTextProperties.font,
                    headerTextProperties.brush,
                    new RectangleF(
                        Border.size,
                        pos,
                        Image.width - Border.size * 2 - headerTextProperties.fix,
                        stringHeight + headerTextProperties.indent),
                    stringFormat);
                // Отделяем
                graphics.FillRectangle(
                    Line.brush,
                    Border.size,
                    pos + stringHeight,
                    Image.width - Border.size * 2,
                    Line.size);
                // Переносим координату
                pos += stringHeight + Line.size;
                // Проходим по дням недели
                for (int i = 0; i < 6; ++i)
                {
                    if (drawingScheduleInfo.weeks[0].days[i].IsStudying || drawingScheduleInfo.weeks[1].days[i].IsStudying)
                        DrawDay(new ScheduleDay[]
                            { 
                                new ScheduleDay(drawingScheduleInfo.weeks[0].days[i]),
                                new ScheduleDay(drawingScheduleInfo.weeks[1].days[i])
                            },
                            ref pos, ref image, Utils.Converter.IndexToDay(i));
                }
                // Рисуем подвал
                graphics.DrawString(
                    drawingScheduleInfo.vkGroupUrl + ScheduleBot.delimiter + ScheduleBot.version,
                    headerTextProperties.font,
                    headerTextProperties.brush,
                    new RectangleF(
                        Border.size,
                        pos,
                        Image.width - Border.size * 2 - headerTextProperties.fix,
                        stringHeight + headerTextProperties.indent),
                    stringFormat);
                pos += stringHeight + Border.size;
                // Рисуем границы
                graphics.FillRectangle(Line.brush, 0, 0, Image.width, Border.size);
                graphics.FillRectangle(Line.brush, 0, pos - Border.size, Image.width, Border.size);
                graphics.FillRectangle(Line.brush, 0, 0, Border.size, pos);
                graphics.FillRectangle(Line.brush, Image.width - Border.size, 0, Border.size, pos);
                // Обрезаем, сохраняем
                graphics.Save();
                graphics.Dispose();
                System.Drawing.Image imageCroped = new Bitmap(Image.width, pos);
                graphics = Graphics.FromImage(imageCroped);
                graphics.DrawImage(image, new Point(0, 0));
                graphics.Save();
                graphics.Dispose();
                return ImageToByteArray(imageCroped);
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Обрабока расписания для рассылки " + course + " " + number);
            }
            private static void DrawDay(ScheduleDay[] scheduleDays, ref int pos, ref System.Drawing.Image image, string day)
            {
                Graphics graphics = Graphics.FromImage(image);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                // Рисуем день
                graphics.DrawString(
                    day,
                    dayTextProperties.font,
                    dayTextProperties.brush,
                    new RectangleF(
                        timeendPosX + Line.size,
                        pos,
                        Image.width - timeendPosX - Line.size - Border.size - dayTextProperties.fix,
                        stringHeight + dayTextProperties.indent),
                    stringFormat);
                // Рисуем количество пар
                float countOfLectures = (scheduleDays[0].lectures.Count + scheduleDays[1].lectures.Count) / 2F;
                string countOfLecturesStr = countOfLectures.ToString();
                if (new float[] { 0.5F, 1.5F, 2F, 2.5F, 3F, 3.5F, 4F, 4.5F }.ToList().Contains(countOfLectures))
                {
                    countOfLecturesStr += " пары";
                }
                else if (countOfLectures == 1F)
                {
                    countOfLecturesStr += " пара";
                }
                else
                {
                    countOfLecturesStr += " пар";
                }
                graphics.DrawString(
                    countOfLecturesStr,
                    lectureProperties.textProperties.font,
                    lectureProperties.textProperties.brush,
                    new RectangleF(
                        Border.size,
                        pos,
                        timeendPosX - Border.size, // - lectureProperties.fix,
                        stringHeight + lectureProperties.textProperties.indent),
                    stringFormat);
                // Отделяем
                graphics.FillRectangle(
                    Line.brush,
                    Border.size,
                    pos + stringHeight,
                    Image.width - Border.size * 2,
                    Line.size);
                // Двигаем координату
                pos += stringHeight + Line.size;
                
                List<int>[] daysTimes = { new List<int>(), new List<int>() };
                for (int currentDay = 0; currentDay < 2; currentDay++)
                {
                    for (int currentLecture = 0; currentLecture < scheduleDays[currentDay].lectures.Count; currentLecture++)
                    {
                        daysTimes[currentDay].Add(scheduleDays[currentDay].lectures[currentLecture].TimeStartToInt());
                    }
                }

                for (int currentDay = 0; currentDay < 2; currentDay++)
                {
                    for (int currentLecture = 0; currentLecture < scheduleDays[currentDay].lectures.Count; currentLecture++)
                    {
                        if (daysTimes[currentDay][currentLecture] == 0)
                        {
                            if (currentDay == 0)
                                DrawLecture(scheduleDays[currentDay].lectures[currentLecture], null, ref pos, ref image);
                            else
                                DrawLecture(null, scheduleDays[currentDay].lectures[currentLecture], ref pos, ref image);

                            scheduleDays[currentDay].lectures.RemoveAt(currentLecture);
                            daysTimes[currentDay].RemoveAt(currentLecture);
                            currentLecture--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                while (daysTimes[0].Count != 0 && daysTimes[1].Count != 0)
                {
                    if (daysTimes[0][0] < daysTimes[1][0])
                    {
                        DrawLecture(scheduleDays[0].lectures[0], null, ref pos, ref image);
                        daysTimes[0].RemoveAt(0);
                        scheduleDays[0].lectures.RemoveAt(0);
                    }
                    else if (daysTimes[0][0] > daysTimes[1][0])
                    {
                        DrawLecture(null, scheduleDays[1].lectures[0], ref pos, ref image);
                        daysTimes[1].RemoveAt(0);
                        scheduleDays[1].lectures.RemoveAt(0);
                    }
                    else
                    {
                        if (scheduleDays[0].lectures[0].TimeEnd == scheduleDays[1].lectures[0].TimeEnd)
                        {
                            DrawLecture(scheduleDays[0].lectures[0], scheduleDays[1].lectures[0], ref pos, ref image);

                            daysTimes[0].RemoveAt(0);
                            scheduleDays[0].lectures.RemoveAt(0);

                            daysTimes[1].RemoveAt(0);
                            scheduleDays[1].lectures.RemoveAt(0);
                        }
                        else
                        {
                            DrawLecture(scheduleDays[0].lectures[0], null, ref pos, ref image);
                            daysTimes[0].RemoveAt(0);
                            scheduleDays[0].lectures.RemoveAt(0);

                            DrawLecture(null, scheduleDays[1].lectures[0], ref pos, ref image);
                            daysTimes[1].RemoveAt(0);
                            scheduleDays[1].lectures.RemoveAt(0);
                        }
                    }
                }

                for (int currentDay = 0; currentDay < 2; currentDay++)
                {
                    for (int currentLecture = 0; currentLecture < scheduleDays[currentDay].lectures.Count; currentLecture++)
                    {
                        if (currentDay == 0)
                            DrawLecture(scheduleDays[currentDay].lectures[currentLecture], null, ref pos, ref image);
                        else
                            DrawLecture(null, scheduleDays[currentDay].lectures[currentLecture], ref pos, ref image);
                    }
                }
            }
            private static void DrawLecture(ScheduleLecture upperWeekLecture, ScheduleLecture downWeekLecture, ref int pos, ref System.Drawing.Image image)
            {
                if (upperWeekLecture is null && downWeekLecture is null)
                    return;
                Graphics graphics = Graphics.FromImage(image);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                SizeF textSize;
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                if (upperWeekLecture is null)
                {
                    // квадрат между индикаторами
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight,
                        Indicator.size,
                        Line2.size);
                    // нижний индикатор
                    graphics.FillRectangle(
                        Indicator.brush,
                        Border.size,
                        pos + stringHeight + Line2.size,
                        Indicator.size,
                        stringHeight);
                    // закрывающая индикаторы
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size + Indicator.size,
                        pos,
                        Line2.size,
                        stringHeight * 2 + Line2.size);
                    // время начала
                    graphics.DrawString(
                        downWeekLecture.TimeStart,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(downWeekLecture.TimeEnd, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        downWeekLecture.TimeEnd,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        timeendPosX - textSize.Width - timeTextProperties.fix,
                        pos + stringHeight + Line2.size + timeTextProperties.indent);
                    // закрытие времени
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX,
                        pos,
                        Line.size,
                        stringHeight * 2 + Line2.size);
                    // пара верхняя
                    textSize = graphics.MeasureString(c_noOneLecture, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        c_noOneLecture,
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos,
                            lectureProperties.width, // - lectureProperties.fix,
                            stringHeight - lectureProperties.textProperties.indent),
                        stringFormat);
                    // пара нижняя
                    graphics.DrawString(
                        LectureShortening(downWeekLecture, lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos + stringHeight + Line2.size,
                            lectureProperties.width, // - lectureProperties.fix,
                            stringHeight + lectureProperties.textProperties.indent),
                        stringFormat);
                    // между парами
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX + Line.size,
                        pos + stringHeight,
                        Image.width - (timeendPosX + Line.size) - Border.size,
                        Line2.size);
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2 + Line2.size,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                else if (downWeekLecture is null)
                {
                    // верхний индикатор
                    graphics.FillRectangle(
                        Indicator.brush,
                        Border.size,
                        pos,
                        Indicator.size,
                        stringHeight);
                    // квадрат между индикаторами
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight,
                        Indicator.size,
                        Line2.size);
                    // закрывающая индикаторы
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size + Indicator.size,
                        pos,
                        Line2.size,
                        stringHeight * 2 + Line2.size);
                    // время начала
                    graphics.DrawString(
                        upperWeekLecture.TimeStart,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(upperWeekLecture.TimeEnd, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        upperWeekLecture.TimeEnd,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        timeendPosX - textSize.Width - timeTextProperties.fix,
                        pos + stringHeight + Line2.size + timeTextProperties.indent);
                    // закрытие времени
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX,
                        pos,
                        Line.size,
                        stringHeight * 2 + Line2.size);
                    // пара верхняя
                    graphics.DrawString(
                        LectureShortening(upperWeekLecture, lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos,
                            lectureProperties.width, // - lectureProperties.fix,
                            stringHeight + lectureProperties.textProperties.indent),
                        stringFormat);
                    // пара нижняя
                    textSize = graphics.MeasureString(c_noOneLecture, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        c_noOneLecture,
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos + stringHeight + Line2.size,
                            lectureProperties.width, // - lectureProperties.fix,
                            stringHeight - lectureProperties.textProperties.indent),
                        stringFormat);
                    // между парами
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX + Line.size,
                        pos + stringHeight,
                        Image.width - (timeendPosX + Line.size) - Border.size,
                        Line2.size);
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2 + Line2.size,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                else if (upperWeekLecture == downWeekLecture)
                {
                    // индикатор
                    graphics.FillRectangle(
                        Indicator.brush,
                        Border.size,
                        pos,
                        Indicator.size,
                        stringHeight * 2 + Line2.size);
                    // закрывающая индикатор
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size + Indicator.size,
                        pos,
                        Line2.size,
                        stringHeight * 2 + Line2.size);
                    // время начала
                    graphics.DrawString(
                        upperWeekLecture.TimeStart,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(upperWeekLecture.TimeEnd, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        upperWeekLecture.TimeEnd,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        timeendPosX - textSize.Width - timeTextProperties.fix,
                        pos + stringHeight + Line2.size + timeTextProperties.indent);
                    // закрытие времени
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX,
                        pos,
                        Line.size,
                        stringHeight * 2 + Line2.size);
                    // пары
                    if (upperWeekLecture.Status == "F3" || upperWeekLecture.Status == "N2")
                    {
                        // пара верхняя
                        graphics.DrawString(
                            LectureShortening(upperWeekLecture.GetLectureWithOnlySubject(), lectureProperties),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos,
                                lectureProperties.width, // - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                            stringFormat);
                        // пара нижняя
                        graphics.DrawString(
                            upperWeekLecture.ToString(withTime: false, withSubject: false),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos + stringHeight + Line2.size,
                                lectureProperties.width, // - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                            stringFormat);
                    }
                    else if (upperWeekLecture.Status == "F1" || upperWeekLecture.Status == "N0")
                    {
                        string lectureStr = upperWeekLecture.ToString(withTime: false);
                        // Проверяем есть ли 2 пробела
                        if (lectureStr.IndexOf(' ') != -1
                            && lectureStr.IndexOf(' ') != lectureStr.LastIndexOf(' ')
                            && graphics.MeasureString(lectureStr, soloLectureTextProperties.font).Width >= lectureProperties.width - lectureProperties.fix)
                        {
                            // пара верхняя
                            graphics.DrawString(
                                lectureStr.Substring(0, lectureStr.IndexOf(' ')),
                                lectureProperties.textProperties.font,
                                lectureProperties.textProperties.brush,
                                new RectangleF(
                                    timeendPosX + Line.size,
                                    pos,
                                    lectureProperties.width, // - lectureProperties.fix,
                                    stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                                stringFormat);
                            // пара нижняя
                            graphics.DrawString(
                                lectureStr.Substring(lectureStr.IndexOf(' ') + 1),
                                lectureProperties.textProperties.font,
                                lectureProperties.textProperties.brush,
                                new RectangleF(
                                    timeendPosX + Line.size,
                                    pos + stringHeight + Line2.size,
                                    lectureProperties.width, // - lectureProperties.fix,
                                    stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                                stringFormat);
                        }
                        else
                        {
                            // пара посередине
                            graphics.DrawString(
                                lectureStr,
                                soloLectureTextProperties.font,
                                soloLectureTextProperties.brush,
                                new RectangleF(
                                    timeendPosX + Line.size,
                                    pos,
                                    lectureProperties.width - soloLectureTextProperties.fix,
                                    stringHeight * 2 + Line2.size + soloLectureTextProperties.indent),
                                stringFormat);
                        }
                    }
                    /*else if (upperWeekLecture.Status == "F2")
                    {
                        string lectureStr = upperWeekLecture.ToString(withTime: false);
                        // пара верхняя
                        graphics.DrawString(
                            lectureStr.Substring(0, lectureStr.IndexOf(" или ") + 4),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                            stringFormat);
                        // пара нижняя
                        graphics.DrawString(
                            lectureStr.Substring(lectureStr.IndexOf(" или ") + 1),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos + stringHeight + Line2.size,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                            stringFormat);
                    }*/
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2 + Line2.size,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                // Если пары по верхним и нижним неделям отличаются
                else
                {
                    // верхний индикатор
                    graphics.FillRectangle(
                        Indicator.brush,
                        Border.size,
                        pos,
                        Indicator.size,
                        stringHeight);
                    // квадрат между индикаторами
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight,
                        Indicator.size,
                        Line2.size);
                    // нижний индикатор
                    graphics.FillRectangle(
                        Indicator.brush,
                        Border.size,
                        pos + stringHeight + Line2.size,
                        Indicator.size,
                        stringHeight);
                    // закрывающая индикаторы
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size + Indicator.size,
                        pos,
                        Line2.size,
                        stringHeight * 2 + Line2.size);
                    // время начала
                    graphics.DrawString(
                        upperWeekLecture.TimeStart,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(upperWeekLecture.TimeEnd, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        upperWeekLecture.TimeEnd,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        timeendPosX - textSize.Width - timeTextProperties.fix,
                        pos + stringHeight + Line2.size + timeTextProperties.indent);
                    // закрытие времени
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX,
                        pos,
                        Line.size,
                        stringHeight * 2 + Line2.size);
                    // пара верхняя
                    graphics.DrawString(
                        LectureShortening(upperWeekLecture, lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos,
                            lectureProperties.width, // - lectureProperties.fix,
                            stringHeight + lectureProperties.textProperties.indent),
                        stringFormat);
                    // пара нижняя
                    graphics.DrawString(
                        LectureShortening(downWeekLecture, lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos + stringHeight + Line2.size,
                            lectureProperties.width, // - lectureProperties.fix,
                            stringHeight + lectureProperties.textProperties.indent),
                        stringFormat);
                    // между парами
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX + Line.size,
                        pos + stringHeight,
                        Image.width - (timeendPosX + Line.size) - Border.size,
                        Line2.size);
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2 + Line2.size,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                pos += stringHeight * 2 + Line2.size + Line.size;
            }
            private static class Image
            {
                public const int width = 700; // ширина постоянная
                public const int height = 3000; // максимальная высота
            }
            private static LectureProperties lectureProperties = new LectureProperties()
            {
                sameFix = 6, // сдвиг вверх и вниз при одинаковых парах по верхним и нижним неделям
                fix = 4, // ???
                width = Image.width - timeendPosX - Line.size - Border.size,
                textProperties = new TextProperties
                {
                    brush = new SolidBrush(Color.Black),
                    font = new Font("TT Commons Light Italic", 18),
                    indent = 8,
                    fix = 2
                }
            };
            private static Color backgroundColor = Color.White; // цвет фона
            private const int stringHeight = 40; // высота строки
            private const int timeWidth = 120; // ширина времени пар
            private const int timeendPosX = Border.size + Indicator.size + Line2.size + timeWidth;
            private const int cellHeight = stringHeight * 2 + Line2.size;
            private static TextProperties headerTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 8,
                fix = 2
                // brush = new SolidBrush(Color.Black),
                // font = new Font("TT Commons Medium Italic", 18),
                // indent = 6,
                // fix = 2
            };
            private static TextProperties dayTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 8,
                fix = 2
                // brush = new SolidBrush(Color.Black),
                // font = new Font("TT Commons Italic", 18),
                // indent = 8,
                // fix = 2
            };
            private static TextProperties soloLectureTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 24),
                indent = 12,
                fix = 3
            };
            private static TextProperties timeTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 11,
                fix = 6
            };
        }
        public static class DaySchedule
        {
            // weekProperties: 0 - Верхняя, 1 - Нижняя
            public static byte[] Draw(DrawingDayScheduleInfo drawingScheduleInfo)
            {
                string week = drawingScheduleInfo.weekProperties == 0 ? "Верхняя" : "Нижняя";
                string day = Utils.Converter.IndexToDay(drawingScheduleInfo.dayOfWeek);
            
                System.Drawing.Image image = new Bitmap(Image.width, Image.height);
                Graphics graphics = Graphics.FromImage(image);
                // Заливаем фон
                graphics.Clear(backgroundColor);
                int pos = Border.size; // y
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                // Рисуем шапку
                graphics.DrawString(
                    drawingScheduleInfo.group + " (" + drawingScheduleInfo.subgroup + ") " + drawingScheduleInfo.date + ScheduleBot.delimiter + week,
                    headerTextProperties.font,
                    headerTextProperties.brush,
                    new RectangleF(
                        Border.size,
                        pos,
                        Image.width - Border.size * 2 - headerTextProperties.fix,
                        stringHeight + headerTextProperties.indent),
                    stringFormat);
                // Отделяем
                graphics.FillRectangle(
                    Line.brush,
                    Border.size,
                    pos + stringHeight,
                    Image.width - Border.size * 2,
                    Line.size);
                // Переносим координату
                pos += stringHeight + Line.size;
                // Рисуем день
                graphics.DrawString(
                    day,
                    dayTextProperties.font,
                    dayTextProperties.brush,
                    new RectangleF(
                        timeendPosX + Line.size,
                        pos,
                        Image.width - timeendPosX - Line.size - Border.size - dayTextProperties.fix,
                        stringHeight + dayTextProperties.indent),
                    stringFormat);
                // Рисуем количество пар
                int countOfLectures = drawingScheduleInfo.day.lectures.Count;
                string countOfLecturesStr = countOfLectures.ToString();
                if (new int[] { 2, 3, 4 }.ToList().Contains(countOfLectures))
                {
                    countOfLecturesStr += " пары";
                }
                else if (countOfLectures == 1)
                {
                    countOfLecturesStr += " пара";
                }
                else
                {
                    countOfLecturesStr += " пар";
                }
                graphics.DrawString(
                    countOfLecturesStr,
                    lectureProperties.textProperties.font,
                    lectureProperties.textProperties.brush,
                    new RectangleF(
                        Border.size,
                        pos,
                        timeendPosX - Border.size, // - lectureProperties.fix,
                        stringHeight + lectureProperties.textProperties.indent),
                    stringFormat);
                // Отделяем
                graphics.FillRectangle(
                    Line.brush,
                    Border.size,
                    pos + stringHeight,
                    Image.width - Border.size * 2,
                    Line.size);
                // Двигаем координату
                pos += stringHeight + Line.size;
                // Проходим по парам
                for (int currentLecture = 0; currentLecture < drawingScheduleInfo.day.lectures.Count; currentLecture++)
                {
                    DrawLecture(drawingScheduleInfo.day.lectures[currentLecture], ref pos, ref image);
                }
                // Рисуем подвал
                graphics.DrawString(
                    drawingScheduleInfo.vkGroupUrl + ScheduleBot.delimiter + ScheduleBot.version,
                    headerTextProperties.font,
                    headerTextProperties.brush,
                    new RectangleF(
                        Border.size,
                        pos,
                        Image.width - Border.size * 2 - headerTextProperties.fix,
                        stringHeight + headerTextProperties.indent),
                    stringFormat);
                pos += stringHeight;
                // Рисуем границы
                graphics.FillRectangle(Line.brush, 0, 0, Image.width, Border.size);
                graphics.FillRectangle(Line.brush, 0, pos, Image.width, Border.size);
                graphics.FillRectangle(Line.brush, 0, 0, Border.size, pos);
                graphics.FillRectangle(Line.brush, Image.width - Border.size, 0, Border.size, pos);
                pos += Border.size;
                // Обрезаем, сохраняем
                graphics.Save();
                graphics.Dispose();
                System.Drawing.Image imageCroped = new Bitmap(Image.width, pos);
                graphics = Graphics.FromImage(imageCroped);
                graphics.DrawImage(image, new Point(0, 0));
                graphics.Save();
                graphics.Dispose();

                return ImageToByteArray(imageCroped);
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Обрабока расписания для рассылки " + course + " " + number + " " + dayOfWeek + " " + weekProperties);
            }
            private static void DrawLecture(ScheduleLecture lecture, ref int pos, ref System.Drawing.Image image)
            {
                Graphics graphics = Graphics.FromImage(image);
                SizeF textSize;
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                if (lecture != null)
                {
                    // индикатор
                    graphics.FillRectangle(
                        Indicator.brush,
                        Border.size,
                        pos,
                        Indicator.size,
                        stringHeight * 2);
                    // закрывающая индикатор
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size + Indicator.size,
                        pos,
                        Line2.size,
                        stringHeight * 2);
                    // время начала
                    graphics.DrawString(
                        lecture.TimeStart,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(lecture.TimeEnd, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        lecture.TimeEnd,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        timeendPosX - textSize.Width - timeTextProperties.fix,
                        pos + stringHeight + timeTextProperties.indent);
                    // закрытие времени
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX,
                        pos,
                        Line.size,
                        stringHeight * 2);
                    // пары
                    //! тут тоже шиза
                    if (lecture.Status == "F3" || lecture.Status == "N2")
                    {
                        // верхняя часть пары (только предмет)
                        graphics.DrawString(
                            LectureShortening(lecture.GetLectureWithOnlySubject(), lectureProperties),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos,
                                lectureProperties.width, // - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                            stringFormat);
                        // нижняя часть пары
                        graphics.DrawString(
                            lecture.ToString(withTime: false, withSubject: false),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos + stringHeight + Line2.size,
                                lectureProperties.width, // - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                            stringFormat);
                    }
                    else if (lecture.Status == "F1" || lecture.Status == "N0")
                    {
                        string lectureStr = lecture.ToString(withTime: false);
                        // Проверяем есть ли 2 пробела
                        if (lectureStr.IndexOf(' ') != -1
                            && lectureStr.IndexOf(' ') != lectureStr.LastIndexOf(' ')
                            && graphics.MeasureString(lectureStr, soloLectureTextProperties.font).Width >= lectureProperties.width - lectureProperties.fix)
                        {
                            // пара верхняя
                            graphics.DrawString(
                                lectureStr.Substring(0, lectureStr.IndexOf(' ')),
                                lectureProperties.textProperties.font,
                                lectureProperties.textProperties.brush,
                                new RectangleF(
                                    timeendPosX + Line.size,
                                    pos,
                                    lectureProperties.width, // - lectureProperties.fix,
                                    stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                                stringFormat);
                            // пара нижняя
                            graphics.DrawString(
                                lectureStr.Substring(lectureStr.IndexOf(' ') + 1),
                                lectureProperties.textProperties.font,
                                lectureProperties.textProperties.brush,
                                new RectangleF(
                                    timeendPosX + Line.size,
                                    pos + stringHeight + Line2.size,
                                    lectureProperties.width, // - lectureProperties.fix,
                                    stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                                stringFormat);
                        }
                        else
                        {
                            // пара посередине
                            graphics.DrawString(
                                lectureStr,
                                soloLectureTextProperties.font,
                                soloLectureTextProperties.brush,
                                new RectangleF(
                                    timeendPosX + Line.size,
                                    pos,
                                    lectureProperties.width - soloLectureTextProperties.fix,
                                    stringHeight * 2 + Line2.size + soloLectureTextProperties.indent),
                                stringFormat);
                        }
                    }
                    /*else if (lecture.Status == "F2")
                    {
                        string lectureStr = lecture.ConstructLecture();
                        // пара верхняя
                        graphics.DrawString(
                            lectureStr.Substring(0, lectureStr.IndexOf(" или ") + 4),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                            stringFormat);
                        // пара нижняя
                        graphics.DrawString(
                            lectureStr.Substring(lectureStr.IndexOf(" или ") + 1),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos + stringHeight + Line2.size,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                            stringFormat);
                    }*/
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                else
                {
                    // закрывающая индикаторы
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size + Indicator.size,
                        pos,
                        Line2.size,
                        stringHeight * 2);
                    // время начала
                    graphics.DrawString(
                        lecture.TimeStart,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(lecture.TimeEnd, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        lecture.TimeEnd,
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        timeendPosX - textSize.Width - timeTextProperties.fix,
                        pos + stringHeight + timeTextProperties.indent);
                    // закрытие времени
                    graphics.FillRectangle(
                        Line.brush,
                        timeendPosX,
                        pos,
                        Line.size,
                        stringHeight * 2);
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                pos += stringHeight * 2 + Line.size;
            }
            private static class Image
            {
                public const int width = 500; // ширина
                public const int height = 1500; // 816; // высота
            }
            private static Color backgroundColor = Color.White; // цвет фона
            private static LectureProperties lectureProperties = new LectureProperties()
            {
                sameFix = 6, // сдвиг вверх и вниз при одинаковых парах по верхним и нижним неделям
                fix = 4, // ???
                width = Image.width - timeendPosX - Line.size - Border.size,
                textProperties = new TextProperties
                {
                    brush = new SolidBrush(Color.Black),
                    font = new Font("TT Commons Light Italic", 18),
                    indent = 8,
                    fix = 2
                },
                soloTextProperties = new TextProperties
                {
                    brush = new SolidBrush(Color.Black),
                    font = new Font("TT Commons Light Italic", 24),
                    indent = 12,
                    fix = 3
                }
            };
            private const int stringHeight = 40; // высота строки
            private const int timeWidth = 96; // ширина блока с отображением времени начала\конца пары
            private const int timeendPosX = Border.size + Indicator.size + Line2.size + timeWidth; // todo: сделать картинку с отображением всех этих параметров
            private const int cellHeight = stringHeight * 2;
            private const int lessonWidth = Image.width - timeendPosX - Line.size - Border.size;
            private static TextProperties headerTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 8,
                fix = 2
                // brush = new SolidBrush(Color.Black),
                // font = new Font("TT Commons Medium Italic", 18),
                // indent = 6,
                // fix = 2
            };
            private static TextProperties dayTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 8,
                fix = 2
                // brush = new SolidBrush(Color.Black),
                // font = new Font("TT Commons Italic", 18),
                // indent = 8,
                // fix = 2
            };
            private static TextProperties timeTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 11,
                fix = 6
            };
            private static TextProperties soloLectureTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 24),
                indent = 12,
                fix = 3
            };
        }
        private static string LectureShortening(ScheduleLecture lecture, LectureProperties lectureProperties)
        {
            if (lecture.Status == "F3" || lecture.Status == "N2")
            {
                Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
                string lectureStr = lecture.ToString(withTime: false);
                SizeF lectureSize = graphics.MeasureString(lectureStr, lectureProperties.textProperties.font);
                if (lectureSize.Width >= lectureProperties.width - lectureProperties.fix)
                {
                    string subject = lecture.Subject;
                    string lectureWithoutSubject = ScheduleBot.delimiter + lecture.ToString(withTime: false, withSubject: false);
                    while (lectureSize.Width >= lectureProperties.width - lectureProperties.fix)
                    {
                        subject = subject.Substring(0, subject.Length - 1);
                        lectureSize = graphics.MeasureString(subject + lectureWithoutSubject,  lectureProperties.textProperties.font);
                    }
                    subject = subject.Substring(0, subject.Length - 1);
                    while (subject[subject.Length - 1] == ' ' || subject[subject.Length - 1] == '(' || subject[subject.Length - 1] == '.') // пока в конце пробел или открывающая скобка, то убираем его
                        subject = subject.Substring(0, subject.Length - 1).TrimEnd();
                    subject += "...";
                    lectureStr = subject + lectureWithoutSubject;
                }
                graphics.Dispose();
                return lectureStr;
            }
            else if (lecture.Status == "F1" || lecture.Status == "F2")
            {
                Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
                string lectureStr = lecture.ToString(withTime: false);
                SizeF lectureSize = graphics.MeasureString(lectureStr, lectureProperties.textProperties.font);
                if (lectureSize.Width >= lectureProperties.width - lectureProperties.fix)
                {
                    while (lectureSize.Width >= lectureProperties.width - lectureProperties.fix)
                    {
                        lectureStr = lectureStr.Substring(0, lectureStr.Length - 1);
                        lectureSize = graphics.MeasureString(lectureStr, lectureProperties.textProperties.font);
                    }
                    lectureStr = lectureStr.Substring(0, lectureStr.Length - 1).Trim();
                    lectureStr += "...";
                }
                graphics.Dispose();
                return lectureStr;
            }
            else if (lecture.Status == "N0")
                return lecture.ToString(withTime: false);
            else 
                return lecture.ToString(withTime: false);
        }

        private static byte[] ImageToByteArray(System.Drawing.Image image)
        {
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            return memoryStream.ToArray();
        }
    }
    
    public static class Line
    {
        public static readonly Brush brush = new SolidBrush(Color.FromArgb(255, 211, 211, 211)); // кисть
        public const int size = 4; // ширина
    }

    public static class Line2
    {
        public static readonly Brush brush = new SolidBrush(Color.FromArgb(255, 211, 211, 211)); // кисть
        public const int size = 2; // ширина
    }

    public static class Indicator // indicator
    {
        public static Brush brush = new SolidBrush(Color.FromArgb(255, 112, 112, 112)); // кисть
        public const int size = 2; // ширина
    }

    public static class Border // border
    {
        public static Brush brush = new SolidBrush(Color.FromArgb(255, 211, 211, 211)); // кисть
        public const int size = 8; // ширина
    }

    public class LectureProperties
    {
        public int sameFix; // сдвиг вверх и вниз при одинаковых парах по верхним и нижним неделям
        public int fix; // ???
        public int width;
        public TextProperties textProperties;
        public TextProperties soloTextProperties;
    }

    public struct TextProperties
    {
        public Font font;
        public Brush brush;
        public int indent;
        public float fix;
    }
}
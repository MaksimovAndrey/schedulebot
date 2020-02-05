using System;
using System.Drawing;
using System.Linq;
using System.IO;

using Schedulebot.Schedule;

namespace Schedulebot.Drawing
{
    public struct DrawingStandartScheduleInfo
    {
        public ScheduleSubgroup schedule;
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
                string[] days = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
                for (int i = 0; i < 6; ++i)
                {
                    if (drawingScheduleInfo.schedule.weeks[0].days[i].isStudying || drawingScheduleInfo.schedule.weeks[1].days[i].isStudying)
                        DrawDay(new ScheduleDay[] { drawingScheduleInfo.schedule.weeks[0].days[i], drawingScheduleInfo.schedule.weeks[1].days[i] },
                            ref pos, ref image, days[i]);
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
            static void DrawDay(ScheduleDay[] scheduleDays, ref int pos, ref System.Drawing.Image image, string day)
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
                float countOfLectures = (scheduleDays[0].CountOfLectures() + scheduleDays[1].CountOfLectures()) / 2F;
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
                        timeendPosX - Border.size - lectureProperties.fix,
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
                if (scheduleDays[0].lectures[0].ConstructLecture().ToUpper().Contains("ВОЕННАЯ ПОДГОТОВКА")) //? если только по верхним/нижним неделям неверно, бывает ли такое?
                {
                    DrawLecture(new ScheduleLecture[] { scheduleDays[0].lectures[0], scheduleDays[1].lectures[0] }, ref pos, ref image, new string[] { "Весь", "день" });
                }
                else
                {
                    string[][] times =  { new string[] { "7:30", "9:00"   },
                                          new string[] { "9:10", "10:40"  },
                                          new string[] { "10:50", "12:20" },
                                          new string[] { "13:00", "14:30" },
                                          new string[] { "14:40", "16:10" },
                                          new string[] { "16:20", "17:50" },
                                          new string[] { "18:00", "19:30" },
                                          new string[] { "19:40", "21:10" } };
                    for (int i = 0; i < 8; ++i)
                    {
                        if (!(scheduleDays[0].lectures[i].IsEmpty() && scheduleDays[1].lectures[i].IsEmpty()))
                            DrawLecture(new ScheduleLecture[] { scheduleDays[0].lectures[i], scheduleDays[1].lectures[i] }, ref pos, ref image, times[i]);
                    }
                }
            }
            static void DrawLecture(ScheduleLecture[] lectures, ref int pos, ref System.Drawing.Image image, string[] times)
            {
                Graphics graphics = Graphics.FromImage(image);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                SizeF textSize;
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                if (lectures[0] == lectures[1])
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
                        times[0],
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(times[1], lectureProperties.textProperties.font);
                    graphics.DrawString(
                        times[1],
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
                    if (lectures[1].status == "F3" || lectures[1].status == "N2")
                    {
                        // пара верхняя
                        graphics.DrawString(
                            LectureShortening(lectures[1].GetLectureWithOnlySubject(), lectureProperties),
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
                            lectures[0].ConstructLectureWithoutSubject(),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos + stringHeight + Line2.size,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                            stringFormat);
                    }
                    else if (lectures[1].status == "F1" || lectures[1].status == "N0")
                    {
                        string lectureStr = lectures[1].ConstructLecture();
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
                                    lectureProperties.width - lectureProperties.fix,
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
                                    lectureProperties.width - lectureProperties.fix,
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
                    else if (lectures[1].status == "F2")
                    {
                        string lectureStr = lectures[1].ConstructLecture();
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
                    }
                    // после
                    graphics.FillRectangle(
                        Line.brush,
                        Border.size,
                        pos + stringHeight * 2 + Line2.size,
                        Image.width - Border.size * 2,
                        Line.size);
                }
                else if (lectures[1].IsEmpty())
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
                        times[0],
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(times[1], lectureProperties.textProperties.font);
                    graphics.DrawString(
                        times[1],
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
                    string noLecture = "____"; // todo: вынести noLecture в свойства расписания
                    textSize = graphics.MeasureString(noLecture, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        noLecture,
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos,
                            lectureProperties.width - lectureProperties.fix,
                            stringHeight - lectureProperties.textProperties.indent),
                        stringFormat);
                    // пара нижняя
                    graphics.DrawString(
                        LectureShortening(lectures[0], lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos + stringHeight + Line2.size,
                            lectureProperties.width - lectureProperties.fix,
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
                else if (lectures[0].IsEmpty())
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
                        times[0],
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(times[1], lectureProperties.textProperties.font);
                    graphics.DrawString(
                        times[1],
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
                        LectureShortening(lectures[1], lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos,
                            lectureProperties.width - lectureProperties.fix,
                            stringHeight + lectureProperties.textProperties.indent),
                        stringFormat);
                    // пара нижняя
                    string noLecture = "____"; // todo: вынести noLecture в свойства расписания
                    textSize = graphics.MeasureString(noLecture, lectureProperties.textProperties.font);
                    graphics.DrawString(
                        noLecture,
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos + stringHeight + Line2.size,
                            lectureProperties.width - lectureProperties.fix,
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
                        times[0],
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(times[1], lectureProperties.textProperties.font);
                    graphics.DrawString(
                        times[1],
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
                        LectureShortening(lectures[1], lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos,
                            lectureProperties.width - lectureProperties.fix,
                            stringHeight + lectureProperties.textProperties.indent),
                        stringFormat);
                    // пара нижняя
                    graphics.DrawString(
                        LectureShortening(lectures[0], lectureProperties),
                        lectureProperties.textProperties.font,
                        lectureProperties.textProperties.brush,
                        new RectangleF(
                            timeendPosX + Line.size,
                            pos + stringHeight + Line2.size,
                            lectureProperties.width - lectureProperties.fix,
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
            public static class Image
            {
                public const int width = 700; // ширина постоянная
                public const int height = 3000; // максимальная высота
            }
            public static LectureProperties lectureProperties = new LectureProperties()
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
            public static Color backgroundColor = Color.White; // цвет фона
            public const int stringHeight = 40; // высота строки
            public const int timeWidth = 120; // ширина времени пар
            public const int timeendPosX = Border.size + Indicator.size + Line2.size + timeWidth;
            public const int cellHeight = stringHeight * 2 + Line2.size;
            public static TextProperties headerTextProperties = new TextProperties
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
            public static TextProperties dayTextProperties = new TextProperties
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
            public static TextProperties soloLectureTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 24),
                indent = 12,
                fix = 3
            };
            public static TextProperties timeTextProperties = new TextProperties
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
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Обрабока расписания на завтра " + course + " " + number + " " + dayOfWeek + " " + weekProperties); // 2log
                //! кто меня из дурки выпустил? разобраться
                string week = "Верхняя";
                if (drawingScheduleInfo.weekProperties == 1)
                    week = "Нижняя";
                string day = "День недели";
                switch (drawingScheduleInfo.dayOfWeek)
                {
                    case 0:
                    {
                        day = "Понедельник";
                        break;
                    }
                    case 1:
                    {
                        day = "Вторник";
                        break;
                    }
                    case 2:
                    {
                        day = "Среда";
                        break;
                    }
                    case 3:
                    {
                        day = "Четверг";
                        break;
                    }
                    case 4:
                    {
                        day = "Пятница";
                        break;
                    }
                    case 5:
                    {
                        day = "Суббота";
                        break;
                    }
                }
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
                int countOfLectures = drawingScheduleInfo.day.CountOfLectures();
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
                        timeendPosX - Border.size - lectureProperties.fix,
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

                string[][] times =  { new string[] { "7:30", "9:00"   },
                                      new string[] { "9:10", "10:40"  },
                                      new string[] { "10:50", "12:20" },
                                      new string[] { "13:00", "14:30" },
                                      new string[] { "14:40", "16:10" },
                                      new string[] { "16:20", "17:50" },
                                      new string[] { "18:00", "19:30" },
                                      new string[] { "19:40", "21:10" } };
                for (int i = 0; i < 8; ++i)
                {
                    DrawLecture(drawingScheduleInfo.day.lectures[i], ref pos, ref image, times[i]);
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
                // Cохраняем
                graphics.Save();
                graphics.Dispose();
                return ImageToByteArray(image);
                // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Обрабока расписания для рассылки " + course + " " + number + " " + dayOfWeek + " " + weekProperties);
            }
            static void DrawLecture(ScheduleLecture lecture, ref int pos, ref System.Drawing.Image image, string[] times)
            {
                Graphics graphics = Graphics.FromImage(image);
                SizeF textSize;
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                if (!lecture.IsEmpty())
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
                        times[0],
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(times[1], lectureProperties.textProperties.font);
                    graphics.DrawString(
                        times[1],
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
                    // string temp = lesson;
                    if (lecture.status == "F3" || lecture.status == "N2")
                    {
                        // верхняя часть пары (только предмет)
                        graphics.DrawString(
                            LectureShortening(lecture.GetLectureWithOnlySubject(), lectureProperties),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent + lectureProperties.sameFix * 2),
                            stringFormat);
                        // нижняя часть пары
                        graphics.DrawString(
                            lecture.ConstructLectureWithoutSubject(),
                            lectureProperties.textProperties.font,
                            lectureProperties.textProperties.brush,
                            new RectangleF(
                                timeendPosX + Line.size,
                                pos + stringHeight + Line2.size,
                                lectureProperties.width - lectureProperties.fix,
                                stringHeight + lectureProperties.textProperties.indent - lectureProperties.sameFix * 2),
                            stringFormat);
                    }
                    else if (lecture.status == "F1" || lecture.status == "N0")
                    {
                        string lectureStr = lecture.ConstructLecture();
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
                                    lectureProperties.width - lectureProperties.fix,
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
                                    lectureProperties.width - lectureProperties.fix,
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
                    else if (lecture.status == "F2")
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
                    }
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
                        times[0],
                        timeTextProperties.font,
                        timeTextProperties.brush,
                        Border.size + Indicator.size + Line2.size + timeTextProperties.fix,
                        pos + timeTextProperties.indent);
                    // время конца
                    textSize = graphics.MeasureString(times[1], lectureProperties.textProperties.font);
                    graphics.DrawString(
                        times[1],
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
            public static class Image
            {
                public const int width = 500; // ширина
                public const int height = 816; // высота
            }
            public static Color backgroundColor = Color.White; // цвет фона
            public static LectureProperties lectureProperties = new LectureProperties()
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
            public const int stringHeight = 40; // высота строки
            public const int timeWidth = 96; // ширина блока с отображением времени начала\конца пары
            public const int timeendPosX = Border.size + Indicator.size + Line2.size + timeWidth; // todo: сделать картинку с отображением всех этих параметров
            public const int cellHeight = stringHeight * 2;
            public const int lessonWidth = Image.width - timeendPosX - Line.size - Border.size;
            public static TextProperties headerTextProperties = new TextProperties
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
            public static TextProperties dayTextProperties = new TextProperties
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
            public static TextProperties timeTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 18),
                indent = 11,
                fix = 6
            };
            public static TextProperties soloLectureTextProperties = new TextProperties
            {
                brush = new SolidBrush(Color.Black),
                font = new Font("TT Commons Light Italic", 24),
                indent = 12,
                fix = 3
            };
        }
        static string LectureShortening(ScheduleLecture lecture, LectureProperties lectureProperties)
        {
            if (lecture.status == "F3" || lecture.status == "N2")
            {
                Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
                string lectureStr = lecture.ConstructLecture();
                SizeF lectureSize = graphics.MeasureString(lectureStr, lectureProperties.textProperties.font);
                if (lectureSize.Width >= lectureProperties.width - lectureProperties.fix)
                {
                    string subject = lecture.subject; // было temp
                    string lectureWithoutSubject = ScheduleBot.delimiter + lecture.ConstructLectureWithoutSubject();
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
            else if (lecture.status == "F1" || lecture.status == "F2")
            {
                Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
                string lectureStr = lecture.ConstructLecture();
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
            else if (lecture.status == "N0")
            {
                return lecture.ConstructLecture();
            }
            return lecture.ConstructLecture();
        }
        public static byte[] ImageToByteArray(System.Drawing.Image image)
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
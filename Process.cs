using System;
using System.Drawing;
using System.Linq;
using System.Threading;


namespace schedulebot
{
    public static class Process
    {        
        public static void Schedule(int course, int number) // Обработка расписания для рассылки
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Обрабока расписания для рассылки " + course + " " + number); // 2log
            Bitmap temp = new Bitmap(Const.image_width, Const.image_height);
            temp.SetResolution(96.0F, 96.0F);
            System.Drawing.Image image = temp;
            Graphics gr = Graphics.FromImage(image);
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // Заливаем фон
            gr.Clear(Const.background_color);
            int pos = Const.border_size; // y
            StringFormat stringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            // Рисуем шапку
            string header;
            lock (Glob.locker)
            {
                header = Glob.schedule[course, number, 0] + " (" + Glob.schedule[course, number, 1] + ") " + Glob.data[course];
            }
            gr.DrawString(
                header,
                Const.header.font,
                Const.header.brush,
                new RectangleF(
                    Const.border_size,
                    pos,
                    Const.image_width - Const.border_size * 2 - Const.header.fix,
                    Const.string_height + Const.header.indent),
                stringFormat);
            // Отделяем
            gr.FillRectangle(
                Const.l_brush,
                Const.border_size,
                pos + Const.string_height,
                Const.image_width - Const.border_size * 2,
                Const.l1_size);
            // Переносим координату
            pos += Const.string_height + Const.l1_size;
            // Проходим по дням недели
            DayGraphics(course, number, 2, ref pos, ref image, "Понедельник");
            DayGraphics(course, number, 18, ref pos, ref image, "Вторник");
            DayGraphics(course, number, 34, ref pos, ref image, "Среда");
            DayGraphics(course, number, 50, ref pos, ref image, "Четверг");
            DayGraphics(course, number, 66, ref pos, ref image, "Пятница");
            DayGraphics(course, number, 82, ref pos, ref image, "Суббота");
            // Рисуем подвал
            gr.DrawString(
                Const.footer_text,
                Const.header.font,
                Const.header.brush,
                new RectangleF(
                    Const.border_size,
                    pos,
                    Const.image_width - Const.border_size * 2 - Const.header.fix,
                    Const.string_height + Const.header.indent),
                stringFormat);
            pos += Const.string_height + Const.border_size;
            // Рисуем границы
            gr.FillRectangle(Const.l_brush, 0, 0, Const.image_width, Const.border_size);
            gr.FillRectangle(Const.l_brush, 0, pos - Const.border_size, Const.image_width, Const.border_size);
            gr.FillRectangle(Const.l_brush, 0, 0, Const.border_size, pos);
            gr.FillRectangle(Const.l_brush, Const.image_width - Const.border_size, 0, Const.border_size, pos);
            // Обрезаем, сохраняем
            gr.Save();
            gr.Dispose();
            System.Drawing.Image imageCroped = new Bitmap(Const.image_width, pos);
            gr = Graphics.FromImage(imageCroped);
            gr.DrawImage(image, new Point(0, 0));
            gr.Save();
            gr.Dispose();
            imageCroped.Save(Const.path_images + course + "_" + number + ".jpg");
            Vk.UploadPhoto(Const.path_images + course + "_" + number + ".jpg", Const.mainAlbumId, header, course, number);
            Thread.Sleep(350);
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Обрабока расписания для рассылки " + course + " " + number); // 2log
        }
        static void DayGraphics(int course, int number, int i, ref int pos, ref System.Drawing.Image image, string day)
        {
            string[] tempSchedule = new string[16];
            lock (Glob.locker)
            {
                for (int j = 0; j < 16; ++j)
                    tempSchedule[j] = Glob.schedule[course, number, j + i];
            }
            if (
               !(
                   tempSchedule[0] == "0"
                   && tempSchedule[1] == "0"
                   && tempSchedule[2] == "0"
                   && tempSchedule[3] == "0"
                   && tempSchedule[4] == "0"
                   && tempSchedule[5] == "0"
                   && tempSchedule[6] == "0"
                   && tempSchedule[7] == "0"
                   && tempSchedule[8] == "0"
                   && tempSchedule[9] == "0"
                   && tempSchedule[10] == "0"
                   && tempSchedule[11] == "0"
                   && tempSchedule[12] == "0"
                   && tempSchedule[13] == "0"
                   && tempSchedule[14] == "0"
                   && tempSchedule[15] == "0"))
            {
                Graphics gr = Graphics.FromImage(image);
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                // Рисуем день
                gr.DrawString(
                    day,
                    Const.day.font,
                    Const.day.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos,
                        Const.image_width - Const.timeend_pos_x - Const.l1_size - Const.border_size - Const.day.fix,
                        Const.string_height + Const.day.indent),
                    stringFormat);
                // Рисуем количество пар
                float hours = 0;
                for (int j = 0; j < 16; ++j)
                    if (tempSchedule[j] != "0")
                        hours += 0.5F;
                string hoursText = hours.ToString();
                if (new float[] { 0.5F, 1.5F, 2F, 2.5F, 3F, 3.5F, 4F, 4.5F }.ToList().Contains(hours))
                {
                    hoursText += " пары";
                }
                else if (hours == 1F)
                {
                    hoursText += " пара";
                }
                else
                {
                    hoursText += " пар";
                }
                gr.DrawString(
                    hoursText,
                    Const.lesson.font,
                    Const.lesson.brush,
                     new RectangleF(
                        Const.border_size,
                        pos,
                        Const.timeend_pos_x - Const.border_size - Const.lesson.fix,
                        Const.string_height + Const.lesson.indent),
                    stringFormat);
                // Отделяем
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height,
                    Const.image_width - Const.border_size * 2,
                    Const.l1_size);
                // Двигаем координату
                pos += Const.string_height + Const.l1_size;
                // Проходим по парам
                if (tempSchedule[0].ToUpper().Contains("F1ВОЕННАЯ ПОДГОТОВКА"))
                {
                    LessonGraphics(tempSchedule[0], tempSchedule[1], ref pos, ref image, "Весь", "день");
                }
                else
                {
                    if (!(tempSchedule[0] == "0" && tempSchedule[1] == "0"))
                        LessonGraphics(tempSchedule[0], tempSchedule[1], ref pos, ref image, "7:30", "9:00");
                    if (!(tempSchedule[2] == "0" && tempSchedule[3] == "0"))
                        LessonGraphics(tempSchedule[2], tempSchedule[3], ref pos, ref image, "9:10", "10:40");
                    if (!(tempSchedule[4] == "0" && tempSchedule[5] == "0"))
                        LessonGraphics(tempSchedule[4], tempSchedule[5], ref pos, ref image, "10:50", "12:20");
                    if (!(tempSchedule[6] == "0" && tempSchedule[7] == "0"))
                        LessonGraphics(tempSchedule[6], tempSchedule[7], ref pos, ref image, "13:00", "14:30");
                    if (!(tempSchedule[8] == "0" && tempSchedule[9] == "0"))
                        LessonGraphics(tempSchedule[8], tempSchedule[9], ref pos, ref image, "14:40", "16:10");
                    if (!(tempSchedule[10] == "0" && tempSchedule[11] == "0"))
                        LessonGraphics(tempSchedule[10], tempSchedule[11], ref pos, ref image, "16:20", "17:50");
                    if (!(tempSchedule[12] == "0" && tempSchedule[13] == "0"))
                        LessonGraphics(tempSchedule[12], tempSchedule[13], ref pos, ref image, "18:00", "19:30");
                    if (!(tempSchedule[14] == "0" && tempSchedule[15] == "0"))
                        LessonGraphics(tempSchedule[14], tempSchedule[15], ref pos, ref image, "19:40", "21:10");
                }
            }
        }
        static void LessonGraphics(string lessonUp, string lessonDown, ref int pos, ref System.Drawing.Image image, string start, string end)
        {
            Graphics gr = Graphics.FromImage(image);
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            SizeF textSize;
            StringFormat stringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };
            if (lessonUp == lessonDown)
            {
                // индикатор
                gr.FillRectangle(
                    Const.indicator_brush,
                    Const.border_size,
                    pos,
                    Const.indicator_size,
                    Const.string_height * 2 + Const.l2_size);
                // закрывающая индикатор
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size + Const.indicator_size,
                    pos,
                    Const.l2_size,
                    Const.string_height * 2 + Const.l2_size);
                // время начала
                gr.DrawString(
                    start,
                    Const.time.font,
                    Const.time.brush,
                    Const.border_size + Const.indicator_size + Const.l2_size + Const.time.fix,
                    pos + Const.time.indent);
                // время конца
                textSize = gr.MeasureString(end, Const.lesson.font);
                gr.DrawString(
                    end,
                    Const.time.font,
                    Const.time.brush,
                    Const.timeend_pos_x - textSize.Width - Const.time.fix,
                    pos + Const.string_height + Const.l2_size + Const.time.indent);
                // закрытие времени
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x,
                    pos,
                    Const.l1_size,
                    Const.string_height * 2 + Const.l2_size);
                // пары
                string properties = lessonUp.Substring(0, 2);
                if (properties == "F3" || properties == "N2")
                {
                    lessonUp = LessonShortening("F1" + lessonUp.Substring(2, lessonUp.IndexOf('·') - 3));
                    lessonDown = lessonDown.Substring(lessonDown.IndexOf('·') + 2);
                }
                else if (properties == "F1" || properties == "N0")
                {
                    if (
                        lessonUp.IndexOf(' ') != -1
                        && lessonUp.IndexOf(' ') != lessonUp.LastIndexOf(' ')
                        && gr.MeasureString(lessonUp.Substring(2, lessonUp.IndexOf(' ') - 2), Const.lesson_solo.font).Width >= Const.lesson_width - Const.lesson_fix)
                    {
                        lessonUp = lessonUp.Substring(2, lessonUp.IndexOf(' ') - 2);
                        lessonDown = lessonDown.Substring(lessonDown.IndexOf(' ') + 1);
                    }
                    else
                        lessonDown = "+";
                }
                else if (properties == "F2")
                {
                    lessonUp = lessonUp.Substring(2, lessonUp.IndexOf(" или ") + 4);
                    lessonDown = lessonDown.Substring(lessonDown.IndexOf(" или ") + 1); // + 5
                }
                if (lessonDown == "+")
                {
                    // пара посередине
                    gr.DrawString(
                        lessonUp.Substring(2),
                        Const.lesson_solo.font,
                        Const.lesson_solo.brush,
                        new RectangleF(
                            Const.timeend_pos_x + Const.l1_size,
                            pos,
                            Const.lesson_width - Const.lesson_solo.fix,
                            Const.string_height * 2 + Const.l2_size + Const.lesson_solo.indent),
                        stringFormat);
                }
                else
                {
                    // пара верхняя
                    gr.DrawString(
                        lessonUp,
                        Const.lesson.font,
                        Const.lesson.brush,
                        new RectangleF(
                            Const.timeend_pos_x + Const.l1_size,
                            pos,
                            Const.lesson_width - Const.lesson.fix,
                            Const.string_height + Const.lesson.indent + Const.same_lessons * 2),
                        stringFormat);
                    // пара нижняя
                    gr.DrawString(
                        lessonDown,
                        Const.lesson.font,
                        Const.lesson.brush,
                        new RectangleF(
                            Const.timeend_pos_x + Const.l1_size,
                            pos + Const.string_height + Const.l2_size,
                            Const.lesson_width - Const.lesson.fix,
                            Const.string_height + Const.lesson.indent - Const.same_lessons * 2),
                        stringFormat);
                }
                // после
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height * 2 + Const.l2_size,
                    Const.image_width - Const.border_size * 2,
                    Const.l1_size);
            }
            else if (lessonUp == "0")
            {
                // квадрат между индикаторами
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height,
                    Const.indicator_size,
                    Const.l2_size);
                // нижний индикатор
                gr.FillRectangle(
                    Const.indicator_brush,
                    Const.border_size,
                    pos + Const.string_height + Const.l2_size,
                    Const.indicator_size,
                    Const.string_height);
                // закрывающая индикаторы
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size + Const.indicator_size,
                    pos,
                    Const.l2_size,
                    Const.string_height * 2 + Const.l2_size);
                // время начала
                gr.DrawString(
                    start,
                    Const.time.font,
                    Const.time.brush,
                    Const.border_size + Const.indicator_size + Const.l2_size + Const.time.fix,
                    pos + Const.time.indent);
                // время конца
                textSize = gr.MeasureString(end, Const.lesson.font);
                gr.DrawString(
                    end,
                    Const.time.font,
                    Const.time.brush,
                    Const.timeend_pos_x - textSize.Width - Const.time.fix,
                    pos + Const.string_height + Const.l2_size + Const.time.indent);
                // закрытие времени
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x,
                    pos,
                    Const.l1_size,
                    Const.string_height * 2 + Const.l2_size);
                // пара верхняя
                string noLesson = "____";
                textSize = gr.MeasureString(noLesson, Const.lesson.font);
                gr.DrawString(
                    noLesson,
                    Const.lesson.font,
                    Const.lesson.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos,
                        Const.lesson_width - Const.lesson.fix,
                        Const.string_height - Const.lesson.indent),
                    stringFormat);
                // пара нижняя
                lessonDown = LessonShortening(lessonDown);
                gr.DrawString(
                    lessonDown,
                    Const.lesson.font,
                    Const.lesson.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos + Const.string_height + Const.l2_size,
                        Const.lesson_width - Const.lesson.fix,
                        Const.string_height + Const.lesson.indent),
                    stringFormat);
                // между парами
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x + Const.l1_size,
                    pos + Const.string_height,
                    Const.image_width - (Const.timeend_pos_x + Const.l1_size) - Const.border_size,
                    Const.l2_size);
                // после
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height * 2 + Const.l2_size,
                    Const.image_width - Const.border_size * 2,
                    Const.l1_size);
            }
            else if (lessonDown == "0")
            {
                // верхний индикатор
                gr.FillRectangle(
                    Const.indicator_brush,
                    Const.border_size,
                    pos,
                    Const.indicator_size,
                    Const.string_height);
                // квадрат между индикаторами
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height,
                    Const.indicator_size,
                    Const.l2_size);
                // закрывающая индикаторы
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size + Const.indicator_size,
                    pos,
                    Const.l2_size,
                    Const.string_height * 2 + Const.l2_size);
                // время начала
                gr.DrawString(
                    start,
                    Const.time.font,
                    Const.time.brush,
                    Const.border_size + Const.indicator_size + Const.l2_size + Const.time.fix,
                    pos + Const.time.indent);
                // время конца
                textSize = gr.MeasureString(end, Const.lesson.font);
                gr.DrawString(
                    end,
                    Const.time.font,
                    Const.time.brush,
                    Const.timeend_pos_x - textSize.Width - Const.time.fix,
                    pos + Const.string_height + Const.l2_size + Const.time.indent);
                // закрытие времени
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x,
                    pos,
                    Const.l1_size,
                    Const.string_height * 2 + Const.l2_size);
                // пара верхняя
                lessonUp = LessonShortening(lessonUp);
                gr.DrawString(
                    lessonUp,
                    Const.lesson.font,
                    Const.lesson.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos,
                        Const.lesson_width - Const.lesson.fix,
                        Const.string_height + Const.lesson.indent),
                    stringFormat);
                // пара нижняя
                string noLesson = "____";
                textSize = gr.MeasureString(noLesson, Const.lesson.font);
                gr.DrawString(
                    noLesson,
                    Const.lesson.font,
                    Const.lesson.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos + Const.string_height + Const.l2_size,
                        Const.lesson_width - Const.lesson.fix,
                        Const.string_height - Const.lesson.indent),
                    stringFormat);
                // между парами
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x + Const.l1_size,
                    pos + Const.string_height,
                    Const.image_width - (Const.timeend_pos_x + Const.l1_size) - Const.border_size,
                    Const.l2_size);
                // после
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height * 2 + Const.l2_size,
                    Const.image_width - Const.border_size * 2,
                    Const.l1_size);
            }
            // Если пары по верхним и нижним неделям отличаются
            else
            {
                // верхний индикатор
                gr.FillRectangle(
                    Const.indicator_brush,
                    Const.border_size,
                    pos,
                    Const.indicator_size,
                    Const.string_height);
                // квадрат между индикаторами
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height,
                    Const.indicator_size,
                    Const.l2_size);
                // нижний индикатор
                gr.FillRectangle(
                    Const.indicator_brush,
                    Const.border_size,
                    pos + Const.string_height + Const.l2_size,
                    Const.indicator_size,
                    Const.string_height);
                // закрывающая индикаторы
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size + Const.indicator_size,
                    pos,
                    Const.l2_size,
                    Const.string_height * 2 + Const.l2_size);
                // время начала
                gr.DrawString(
                    start,
                    Const.time.font,
                    Const.time.brush,
                    Const.border_size + Const.indicator_size + Const.l2_size + Const.time.fix,
                    pos + Const.time.indent);
                // время конца
                textSize = gr.MeasureString(end, Const.lesson.font);
                gr.DrawString(
                    end,
                    Const.time.font,
                    Const.time.brush,
                    Const.timeend_pos_x - textSize.Width - Const.time.fix,
                    pos + Const.string_height + Const.l2_size + Const.time.indent);
                // закрытие времени
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x,
                    pos,
                    Const.l1_size,
                    Const.string_height * 2 + Const.l2_size);
                // пара верхняя
                lessonUp = LessonShortening(lessonUp);
                gr.DrawString(
                    lessonUp,
                    Const.lesson.font,
                    Const.lesson.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos,
                        Const.lesson_width - Const.lesson.fix,
                        Const.string_height + Const.lesson.indent),
                    stringFormat);
                // пара нижняя
                lessonDown = LessonShortening(lessonDown);
                gr.DrawString(
                    lessonDown,
                    Const.lesson.font,
                    Const.lesson.brush,
                    new RectangleF(
                        Const.timeend_pos_x + Const.l1_size,
                        pos + Const.string_height + Const.l2_size,
                        Const.lesson_width - Const.lesson.fix,
                        Const.string_height + Const.lesson.indent),
                    stringFormat);
                // между парами
                gr.FillRectangle(
                    Const.l_brush,
                    Const.timeend_pos_x + Const.l1_size,
                    pos + Const.string_height,
                    Const.image_width - (Const.timeend_pos_x + Const.l1_size) - Const.border_size,
                    Const.l2_size);
                // после
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.string_height * 2 + Const.l2_size,
                    Const.image_width - Const.border_size * 2,
                    Const.l1_size);
            }
            pos += Const.string_height * 2 + Const.l2_size + Const.l1_size;
        }
        public static void TomorrowSchedule(int course, int number, int dayOfWeek, int weekProperties)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Обрабока расписания на завтра " + course + " " + number + " " + dayOfWeek + " " + weekProperties); // 2log
            string week = " · Верхняя";
            if (weekProperties == 1)
                week = " · Нижняя";
            string day = "День недели";
            switch (dayOfWeek)
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
            string[] tempTomorrowSchedule = new string[8];
            lock (Glob.locker)
            {
                for (int i = 0; i < 16; i += 2)
                {
                    tempTomorrowSchedule[i / 2] = Glob.schedule[course, number, 2 + dayOfWeek * 16 + i + weekProperties];
                }
            }
            bool empty = true;
            for (int i = 0; i < 8; ++i)
            {
                if (tempTomorrowSchedule[i] != "0")
                {
                    empty = false;
                    break;
                }
            }
            if (!empty)
            {
                System.Drawing.Image image = new Bitmap(Const.tomorrow_image_width, Const.tomorrow_image_height);
                Graphics gr = Graphics.FromImage(image);
                // Заливаем фон
                gr.Clear(Const.background_color);
                int pos = Const.border_size; // y
                StringFormat stringFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                // Рисуем шапку
                string header;
                lock (Glob.locker)
                {
                    header = Glob.schedule[course, number, 0] + " (" + Glob.schedule[course, number, 1] + ") " + Glob.data[course] + week;
                }
                gr.DrawString(
                    header,
                    Const.header.font,
                    Const.header.brush,
                    new RectangleF(
                        Const.border_size,
                        pos,
                        Const.tomorrow_image_width - Const.border_size * 2 - Const.header.fix,
                        Const.tomorrow_string_height + Const.header.indent),
                    stringFormat);
                // Отделяем
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.tomorrow_string_height,
                    Const.tomorrow_image_width - Const.border_size * 2,
                    Const.l1_size);
                // Переносим координату
                pos += Const.string_height + Const.l1_size;
                // Рисуем день
                gr.DrawString(
                    day,
                    Const.day.font,
                    Const.day.brush,
                    new RectangleF(
                        Const.tomorrow_timeend_pos_x + Const.l1_size,
                        pos,
                        Const.tomorrow_image_width - Const.tomorrow_timeend_pos_x - Const.l1_size - Const.border_size - Const.day.fix,
                        Const.tomorrow_string_height + Const.day.indent),
                    stringFormat);
                // Рисуем количество пар
                int hours = 0;
                for (int i = 0; i < 8; ++i)
                    if (tempTomorrowSchedule[i] != "0")
                        ++hours;
                string hoursText = hours.ToString();
                if (new int[] { 2, 3, 4 }.ToList().Contains(hours))
                {
                    hoursText += " пары";
                }
                else if (hours == 1)
                {
                    hoursText += " пара";
                }
                else
                {
                    hoursText += " пар";
                }
                gr.DrawString(
                    hoursText,
                    Const.lesson.font,
                    Const.lesson.brush,
                     new RectangleF(
                        Const.border_size,
                        pos,
                        Const.tomorrow_timeend_pos_x - Const.border_size - Const.lesson.fix,
                        Const.tomorrow_string_height + Const.lesson.indent),
                    stringFormat);
                // Отделяем
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.tomorrow_string_height,
                    Const.tomorrow_image_width - Const.border_size * 2,
                    Const.l1_size);
                // Двигаем координату
                pos += Const.tomorrow_string_height + Const.l1_size;
                // Проходим по парам
                TomorrowLessonGraphics(tempTomorrowSchedule[0], ref pos, ref image, "7:30", "9:00");
                TomorrowLessonGraphics(tempTomorrowSchedule[1], ref pos, ref image, "9:10", "10:40");
                TomorrowLessonGraphics(tempTomorrowSchedule[2], ref pos, ref image, "10:50", "12:20");
                TomorrowLessonGraphics(tempTomorrowSchedule[3], ref pos, ref image, "13:00", "14:30");
                TomorrowLessonGraphics(tempTomorrowSchedule[4], ref pos, ref image, "14:40", "16:10");
                TomorrowLessonGraphics(tempTomorrowSchedule[5], ref pos, ref image, "16:20", "17:50");
                TomorrowLessonGraphics(tempTomorrowSchedule[6], ref pos, ref image, "18:00", "19:30");
                TomorrowLessonGraphics(tempTomorrowSchedule[7], ref pos, ref image, "19:40", "21:10");
                // Рисуем подвал
                gr.DrawString(
                    Const.footer_text,
                    Const.header.font,
                    Const.header.brush,
                    new RectangleF(
                        Const.border_size,
                        pos,
                        Const.tomorrow_image_width - Const.border_size * 2 - Const.header.fix,
                        Const.tomorrow_string_height + Const.header.indent),
                    stringFormat);
                pos += Const.tomorrow_string_height;
                // Рисуем границы
                gr.FillRectangle(Const.l_brush, 0, 0, Const.tomorrow_image_width, Const.border_size);
                gr.FillRectangle(Const.l_brush, 0, pos, Const.tomorrow_image_width, Const.border_size);
                gr.FillRectangle(Const.l_brush, 0, 0, Const.border_size, pos);
                gr.FillRectangle(Const.l_brush, Const.tomorrow_image_width - Const.border_size, 0, Const.border_size, pos);
                // Cохраняем
                gr.Save();
                gr.Dispose();
                image.Save(Const.path_images + @"tomorrow/" + course + "_" + number + "_" + dayOfWeek + "_" + weekProperties + ".jpg");
                Vk.UploadPhoto(Const.path_images + @"tomorrow/" + course + "_" + number + "_" + dayOfWeek + "_" + weekProperties + ".jpg", Const.tomorrowAlbumId, header, course, number, dayOfWeek, weekProperties);
            }
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Обрабока расписания для рассылки " + course + " " + number + " " + dayOfWeek + " " + weekProperties); // 2log
        }
        static void TomorrowLessonGraphics(string lesson, ref int pos, ref System.Drawing.Image image, string start, string end)
        {
            Graphics gr = Graphics.FromImage(image);
            SizeF textSize;
            StringFormat stringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };
            if (lesson != "0")
            {
                // индикатор
                gr.FillRectangle(
                    Const.indicator_brush,
                    Const.border_size,
                    pos,
                    Const.indicator_size,
                    Const.tomorrow_string_height * 2);
                // закрывающая индикатор
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size + Const.indicator_size,
                    pos,
                    Const.l2_size,
                    Const.tomorrow_string_height * 2);
                // время начала
                gr.DrawString(
                    start,
                    Const.time.font,
                    Const.time.brush,
                    Const.border_size + Const.indicator_size + Const.l2_size + Const.time.fix,
                    pos + Const.time.indent);
                // время конца
                textSize = gr.MeasureString(end, Const.lesson.font);
                gr.DrawString(
                    end,
                    Const.time.font,
                    Const.time.brush,
                    Const.tomorrow_timeend_pos_x - textSize.Width - Const.time.fix,
                    pos + Const.tomorrow_string_height + Const.time.indent);
                // закрытие времени
                gr.FillRectangle(
                    Const.l_brush,
                    Const.tomorrow_timeend_pos_x,
                    pos,
                    Const.l1_size,
                    Const.tomorrow_string_height * 2);
                // пары
                string properties = lesson.Substring(0, 2);
                string temp = lesson;
                if (properties == "F3" || properties == "N2")
                {
                    lesson = LessonShortening("F1" + lesson.Substring(2, lesson.IndexOf('·') - 3), Const.tomorrow_lesson_width);
                    temp = temp.Substring(temp.IndexOf('·') + 2);
                }
                else if (properties == "F1" || properties == "N0")
                {
                    if (
                        lesson.IndexOf(' ') != -1
                        && lesson.IndexOf(' ') != lesson.LastIndexOf(' ')
                        && gr.MeasureString(lesson.Substring(2, lesson.IndexOf(' ') - 2), Const.lesson_solo.font).Width >= Const.tomorrow_lesson_width - Const.tomorrow_lesson_fix)
                    {
                        lesson = lesson.Substring(2, lesson.IndexOf(' ') - 2);
                        temp = temp.Substring(temp.IndexOf(' ') + 1);
                    }
                    else
                        temp = "+";
                }
                else if (properties == "F2")
                {
                    lesson = lesson.Substring(2, lesson.IndexOf(" или ") + 4);
                    temp = temp.Substring(temp.IndexOf(" или ") + 1); // + 5
                }
                if (temp == "+")
                {
                    // пара посередине
                    gr.DrawString(
                        lesson.Substring(2),
                        Const.lesson_solo.font,
                        Const.lesson_solo.brush,
                        new RectangleF(
                            Const.tomorrow_timeend_pos_x + Const.l1_size,
                            pos,
                            Const.tomorrow_lesson_width - Const.lesson_solo.fix,
                            Const.tomorrow_string_height * 2 + Const.lesson_solo.indent),
                        stringFormat);
                }
                else
                {
                    // пара верхняя
                    gr.DrawString(
                        lesson,
                        Const.lesson.font,
                        Const.lesson.brush,
                        new RectangleF(
                            Const.tomorrow_timeend_pos_x + Const.l1_size,
                            pos,
                            Const.tomorrow_lesson_width - Const.lesson.fix,
                            Const.tomorrow_string_height + Const.lesson.indent + Const.tomorrow_same_lessons * 2),
                        stringFormat);
                    // пара нижняя
                    gr.DrawString(
                        temp,
                        Const.lesson.font,
                        Const.lesson.brush,
                        new RectangleF(
                            Const.tomorrow_timeend_pos_x + Const.l1_size,
                            pos + Const.tomorrow_string_height,
                            Const.tomorrow_lesson_width - Const.lesson.fix,
                            Const.tomorrow_string_height + Const.lesson.indent - Const.tomorrow_same_lessons * 2),
                        stringFormat);
                }
                // после
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.tomorrow_string_height * 2,
                    Const.tomorrow_image_width - Const.border_size * 2,
                    Const.l1_size);
            }
            else
            {
                // закрывающая индикаторы
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size + Const.indicator_size,
                    pos,
                    Const.l2_size,
                    Const.tomorrow_string_height * 2);
                // время начала
                gr.DrawString(
                    start,
                    Const.time.font,
                    Const.time.brush,
                    Const.border_size + Const.indicator_size + Const.l2_size + Const.time.fix,
                    pos + Const.time.indent);
                // время конца
                textSize = gr.MeasureString(end, Const.lesson.font);
                gr.DrawString(
                    end,
                    Const.time.font,
                    Const.time.brush,
                    Const.tomorrow_timeend_pos_x - textSize.Width - Const.time.fix,
                    pos + Const.tomorrow_string_height + Const.time.indent);
                // закрытие времени
                gr.FillRectangle(
                    Const.l_brush,
                    Const.tomorrow_timeend_pos_x,
                    pos,
                    Const.l1_size,
                    Const.tomorrow_string_height * 2);
                // после
                gr.FillRectangle(
                    Const.l_brush,
                    Const.border_size,
                    pos + Const.tomorrow_string_height * 2,
                    Const.tomorrow_image_width - Const.border_size * 2,
                    Const.l1_size);
            }
            pos += Const.tomorrow_string_height * 2 + Const.l1_size;
        }
        static string LessonShortening(string lesson, int lessonWidth = Const.lesson_width)
        {
            Graphics gr = Graphics.FromImage(new Bitmap(1, 1));
            string properties = lesson.Substring(0, 2);
            lesson = lesson.Substring(2);
            if (properties == "F3" || properties == "N2")
            {
                SizeF lessonSize = gr.MeasureString(lesson, Const.lesson.font);
                if (lessonSize.Width >= lessonWidth - Const.lesson_fix)
                {
                    string temp = lesson.Substring(0, lesson.IndexOf('·') - 1);
                    lesson = lesson.Substring(lesson.IndexOf('·') - 1);
                    while (lessonSize.Width >= lessonWidth - Const.lesson_fix)
                    {
                        temp = temp.Substring(0, temp.Length - 1);
                        lessonSize = gr.MeasureString(temp + lesson, Const.lesson.font);
                    }
                    temp = temp.Substring(0, temp.Length - 1);
                    if (temp[temp.Length - 1] == '·' || temp[temp.Length - 1] == ' ')
                    {
                        temp = temp.Substring(0, temp.Length - 1).Trim();
                    }
                    temp += "...";
                    lesson = temp + lesson;
                }
            }
            else if (properties == "F1" || properties == "F2")
            {
                SizeF lessonSize = gr.MeasureString(lesson, Const.lesson.font);
                if (lessonSize.Width >= lessonWidth - Const.lesson_fix)
                {
                    while (lessonSize.Width >= lessonWidth - Const.lesson_fix)
                    {
                        lesson = lesson.Substring(0, lesson.Length - 1);
                        lessonSize = gr.MeasureString(lesson, Const.lesson.font);
                    }
                    lesson = lesson.Substring(0, lesson.Length - 1).Trim();
                    lesson += "...";
                }
            }
            else if (properties == "N0")
            {
                lesson = lesson.Substring(2);
            }
            gr.Dispose();
            return lesson;
        }
    }
}
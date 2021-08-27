using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;

namespace Schedulebot.Utils
{
    public static class Converter
    {

        public static string LecturesCountToString(int lecturesCount)
        {
            if (lecturesCount >= 2 && lecturesCount <= 4)
                return lecturesCount.ToString() + " пары";
            else if (lecturesCount == 1)
                return lecturesCount.ToString() + " пара";
            else
                return lecturesCount.ToString() + " пар";
        }

        public static string DayOfWeekToString(DayOfWeek dayOfWeek)
        {
            string day = ScheduleBot.cultureInfo.DateTimeFormat.GetDayName(dayOfWeek);
            return char.ToUpper(day[0]) + day[1..];
        }

        public static string WeekToString(int week) // Определение недели (верхняя или нижняя)
        {
            return week == 0 ? Constants.upperWeek : Constants.downWeek;
        }

        private static IImageEncoder ImageEncoder { get; } = new JpegEncoder();

        public static byte[] ImageToByteArray(Image image)
        {
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageEncoder);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Ковертирует строковое представление времени в числовое представление
        /// </summary>
        /// <param name="time">Строка с временем</param>
        /// <returns>
        /// Числовое представление времени
        /// <br>0 - время было равно 00:00 или невозможно конвертировать данную строку</br>
        /// </returns>
        public static int TimeToInt(string time)
        {
            if (time.Length >= 3 && time.Contains(':'))
            {
                time = time.Replace(":", "");
                if (int.TryParse(time, out int timeInt))
                    return timeInt;
                else
                    return 0;
            }
            return 0;
        }

        /// <summary>
        /// Конвертирует строковое представление дня недели в индекс дня недели
        /// </summary>
        /// <param name="day">Строковое представление дня недели</param>
        /// <returns>
        /// Индекс дня недели
        /// <br>-1 в случае ненахода</br>
        /// </returns>
        public static int DayToIndex(string day)
        {
            return day switch
            {
                "ПОНЕДЕЛЬНИК" => 0,
                "ВТОРНИК" => 1,
                "СРЕДА" => 2,
                "ЧЕТВЕРГ" => 3,
                "ПЯТНИЦА" => 4,
                "СУББОТА" => 5,
                _ => -1,
            };
        }

        /// <summary>
        /// Конвертирует индекс дня недели в строку с названием этого дня недели
        /// </summary>
        /// <param name="index">Индекс дня недели</param>
        /// <returns>
        /// Строка с названием дня недели
        /// <br>"Неизвестно" в случае неверного индекса</br>
        /// </returns>
        public static string IndexToDay(int index)
        {
            return index switch
            {
                0 => "Понедельник",
                1 => "Вторник",
                2 => "Среда",
                3 => "Четверг",
                4 => "Пятница",
                5 => "Суббота",
                _ => "Неизвестно",
            };
        }

        /// <summary>
        /// Конвертирует строковое представление промежутка времени в индекс
        /// </summary>
        /// <param name="time">Строковое представление промежутка времени</param>
        /// <returns>
        /// Индекс промежутка времени
        /// <br>-1, если не найден</br>
        /// </returns>
        public static int TimeToIndex(string time)
        {
            return time switch
            {
                "9:00-10:30" => 0,
                "10:40-12:10" => 1,
                "12:20-13:50" => 2,
                "14:30-16:00" => 3,
                "16:10-17:40" => 4,
                "17:50-19:20" => 5,
                "19:30-21:00" => 6,
                _ => -1,
            };
        }
    }
}

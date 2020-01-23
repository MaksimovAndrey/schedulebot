using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;
using System.Net;
using System.Xml;
using GemBox.Spreadsheet;
using System.IO;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using VkNet.Categories;
using VkNet.Enums.SafetyEnums;
using System.Drawing;
using System.Text.RegularExpressions;
using VkNet.Model.Keyboard;
using VkNet.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace schedulebot
{
    public static class Utils
    {
        public static void StartUp() // Загрузка всех сохраненных данных
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка всех сохраненных данных");
            // //TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            // SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            // IO.LoadSettings();
            // Glob.api.Authorize(new ApiAuthParams() { AccessToken = Const.key });
            // Glob.apiPhotos.Authorize(new ApiAuthParams() { AccessToken = Const.keyPhotos });
            // Glob.api.RequestsPerSecond = 20;
            // IO.LoadManualFullName();
            // IO.LoadManualAcronymToPhrase();
            // IO.LoadManualDoubleOptionalSubject();
            // IO.LoadSubscribers();
            // IO.LoadDataUrls();
            // IO.LoadSavedSchedule();
            // IO.LoadUploadedSchedule();
            // //LoadUploadedTomorrowSchedule();
            // TomorrowStudying();
            // ScheduleMapping();
            // СonstructingKeyboards();
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка всех сохраненных данных");
        }
        public static void ScheduleMapping()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Маппинг расписания");
            // lock (Glob.locker)
            // {
            //     Glob.schedule_mapping.Clear();
            //     for (int i = 0; i < 4; ++i)
            //     {
            //         for (int j = 0; j < 40; ++j)
            //         {
            //             if (Glob.schedule[i, j, 0] != null)
            //             {
            //                 if (Glob.schedule_mapping.ContainsKey(new User(Glob.schedule[i, j, 0], Glob.schedule[i, j, 1])))
            //                 {
            //                     int course = Glob.schedule_mapping[new User(Glob.schedule[i, j, 0], Glob.schedule[i, j, 1])].Course;
            //                     int index = Glob.schedule_mapping[new User(Glob.schedule[i, j, 0], Glob.schedule[i, j, 1])].Index;
            //                     for (int k = 2; k < 98; ++k)
            //                     {
            //                         if (Glob.schedule[i, j, k] == "0" && Glob.schedule[course, index, k] != "0")
            //                         {
            //                             break;
            //                         }
            //                         if (Glob.schedule[i, j, k] != "0" && Glob.schedule[course, index, k] == "0")
            //                         {
            //                             Glob.schedule_mapping.Remove(new User(Glob.schedule[i, j, 0], Glob.schedule[i, j, 1]));
            //                             Glob.schedule_mapping.Add(new User(Glob.schedule[i, j, 0], Glob.schedule[i, j, 1]), new Mapping { Course = i, Index = j });
            //                             break;
            //                         }
            //                     }
            //                 }
            //                 else
            //                 {
            //                     Glob.schedule_mapping.Add(new User(Glob.schedule[i, j, 0], Glob.schedule[i, j, 1]), new Mapping { Course = i, Index = j });
            //                 }
            //             }
            //         }
            //     }
            // }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Маппинг расписания");
        }
        /* public static void TomorrowStudying()
        {
            lock (Glob.locker)
            {
                for (int i = 0; i < 4; ++i)
                    for (int j = 0; j < 40; ++j)
                        for (int k = 0; k < 6; ++k)
                            for (int m = 0; m < 2; ++m)
                            {
                                Glob.tomorrow_studying[i, j, k, m] = false;
                                for (int p = 0; p < 8; ++p)
                                {
                                    if (Glob.schedule[i, j, 2 + k * 16 + p * 2 + m] != "0")
                                    {
                                        Glob.tomorrow_studying[i, j, k, m] = true;
                                        break;
                                    }
                                }
                            }
            }         
        } */
        /* public static string CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            if ((DateTime.Now.DayOfYear - Glob.startDay) / 7 % 2 == 0)
            {
                return "Нижняя";
            }
            else
            {
                return "Верхняя";
            }
        } */
    }
}
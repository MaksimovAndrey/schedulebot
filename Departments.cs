using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;

using Schedulebot.Vk;

namespace Schedulebot
{
    public class ItmmDepartment : IDepartment
    {
        private readonly string path;
        private VkStuff vkStuff = new VkStuff();
        private CheckRelevanceStuff checkRelevanceStuffITMM = new CheckRelevanceStuffITMM();
        // private Dictionary<string, string> acronymToPhrase;
        // private Dictionary<string, string> doubleOptionallySubject;
        // private List<string> fullName;
        public int CoursesAmount { get; set; }
        private Course[] courses = new Course[4]; // 4 курса всегда ЫЫЫЫ
        private int startDay;
        private int startWeek;
        public ItmmDepartment(string _path)
        {
            path = _path + @"itmm\";
            LoadAcronymToPhrase();
            LoadDoubleOptionallySubject();
            LoadFullName();
            for (int i = 0; i < 4; ++i)
                courses[i] = new Course(path + @"downloads\" + i + "_course.xls");
        }
        
        private static class ConstructKeyboardsProperties
        {
            public const int buttonsInLine = 2; // 1..4
            public const int linesInKeyboard = 4; // 1..9 
        }

        private void СonstructKeyboards()
        {
            for (int currentCourse = 0; currentCourse < CoursesAmount; ++currentCourse)
            {
                int groupsAmount = courses[currentCourse].groups.GetLength(0);
                int pagesAmount = (int)Math.Ceiling((double)groupsAmount
                    / (double)(ConstructKeyboardsProperties.linesInKeyboard * ConstructKeyboardsProperties.buttonsInLine));
                int currentPage = 0;
                courses[currentCourse].keyboards = new List<MessageKeyboard>();
                List<MessageKeyboardButton> line = new List<MessageKeyboardButton>();
                List<List<MessageKeyboardButton>> buttons = new List<List<MessageKeyboardButton>>();
                List<MessageKeyboardButton> serviceLine = new List<MessageKeyboardButton>();
                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.GetLength(0); currentGroup++)
                {
                    line.Add(new MessageKeyboardButton()
                    {
                        Color = KeyboardButtonColor.Primary,
                        Action = new MessageKeyboardButtonAction
                        {
                            Type = KeyboardButtonActionType.Text,
                            Label = courses[currentCourse].groups[currentGroup].name,
                            Payload = "{\"menu\": \"30\", \"index\": \"" + currentGroup + "\", \"course\": \"" + currentCourse + "\"}"
                        }
                    });
                    if (line.Count == ConstructKeyboardsProperties.buttonsInLine
                        || (currentGroup + 1 == groupsAmount && line.Count != 0))
                    {
                        buttons.Add(new List<MessageKeyboardButton>(line));
                        line.Clear();
                    }
                    if (buttons.Count == ConstructKeyboardsProperties.linesInKeyboard
                        || (currentGroup + 1 == groupsAmount && buttons.Count != 0))
                    {
                        string payloadService = "{\"menu\": \"30\", \"page\": \"" + currentPage + "\", \"course\": \"" + currentCourse + "\"}";
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = "Назад",
                                Payload = payloadService
                            }
                        });
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = (currentPage + 1) + " из " + pagesAmount,
                                Payload = payloadService
                            }
                        });
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = "Вперед",
                                Payload = payloadService
                            }
                        });
                        buttons.Add(new List<MessageKeyboardButton>(serviceLine));
                        serviceLine.Clear();
                        courses[currentCourse].keyboards.Add(new MessageKeyboard
                        {
                            Buttons = new List<List<MessageKeyboardButton>>(buttons),
                            OneTime = false
                        });
                        buttons.Clear();
                        ++currentPage;
                    }
                }
            }
        }

        public string CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            if ((DateTime.Now.DayOfYear - startDay) / 7 % 2 != startWeek)
            {
                return "Нижняя";
            }
            return "Верхняя";
        }
        
        public void LoadSettings()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка настроек");
            using (StreamReader file = new StreamReader(
                path + "settings.txt",
                System.Text.Encoding.Default))
            {
                string str, value;
                while ((str = file.ReadLine()) != null)
                {
                    if (str.Contains(':'))
                    {
                        value = str.Substring(str.IndexOf(':') + 1);
                        str = str.Substring(0, str.IndexOf(':'));
                        switch (str)
                        {
                            case "key":
                            {
                                vkStuff.api.Authorize(new ApiAuthParams() { AccessToken = value });
                                break;
                            }
                            case "keyPhotos":
                            {
                                vkStuff.apiPhotos.Authorize(new ApiAuthParams() { AccessToken = value });
                                break;
                            }
                            case "groupId":
                            {
                                vkStuff.groupId = ulong.Parse(value);
                                break;
                            }
                            case "mainAlbumId":
                            {
                                vkStuff.mainAlbumId = Int64.Parse(value);
                                break;
                            }
                            case "tomorrowAlbumId":
                            {
                                vkStuff.tomorrowAlbumId = Int64.Parse(value);
                                break;
                            }
                            case "startDay":
                            {
                                startDay = Int32.Parse(value);
                                break;
                            }
                            case "startWeek":
                            {
                                startWeek = Int32.Parse(value);
                                break;
                            }
                        }
                    }
                }
            }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка настроек");
        }

        private void LoadAcronymToPhrase()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка ManualAcronymToPhrase");
            Glob.acronymToPhrase = new Dictionary<string,string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/acronymToPhrase.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                Glob.acronymToPhrase.Add(file.ReadLine(), file.ReadLine());
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка ManualAcronymToPhrase");
        }

        private void LoadDoubleOptionallySubject()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка DoubleOptionallySubject");
            Glob.doubleOptionallySubject = new Dictionary<string,string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/doubleOptionallySubject.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                Glob.doubleOptionallySubject.Add(file.ReadLine(), file.ReadLine());
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка DoubleOptionallySubject");
        }

        private void LoadFullName()
        {
            Glob.fullName = new List<string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/fullName.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                Glob.fullName.Add(file.ReadLine());
        }

        public void CheckRelevance()
        {
            DatesAndUrls newDatesAndUrls = checkRelevanceStuffITMM.CheckRelevance();
            if (newDatesAndUrls != null)
            {
                CoursesAmount = newDatesAndUrls.count;
                List<int> coursesToUpdate = AreScheduleRelevant(newDatesAndUrls);
                UpdateSchedule(coursesToUpdate);
            }
            return;
        }
        
        private void UpdateSchedule(List<int> coursesToUpdate)
        {
            for (int i = 0; i < coursesToUpdate.Count; ++i)
            {
                // async
                courses[i].Update();
            }
        }
        
        private List<int> AreScheduleRelevant(DatesAndUrls newDatesAndUrls)
        {
            List<int> notRelevantCourses = new List<int>();
            CoursesAmount = newDatesAndUrls.count;
            for (int i = 0; i < newDatesAndUrls.count; ++i)
            {
                if (newDatesAndUrls.dates[i] != null && courses[i].date != newDatesAndUrls.dates[i])
                {
                    notRelevantCourses.Add(i);
                }
            }
            return notRelevantCourses;
        }
    }
    





    public interface IDepartment
    {
        int CoursesAmount { get; set; }

        void CheckRelevance();

    }
}
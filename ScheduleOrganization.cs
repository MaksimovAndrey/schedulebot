using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;

namespace schedulebot
{
    public class CheckRelevanceStuffITMM : CheckRelevanceStuff
    {
        private const string url = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
        public override DatesAndUrls CheckRelevance()
        {
            HtmlDocument page;
            try
            {
                page = new HtmlWeb().Load(url);
            }
            catch 
            {
                //! ошибка загрузки страницы
                return null;
            }
            if (page != null)
                return Parse(page);
            return null;
        }
        private DatesAndUrls Parse(HtmlDocument htmlDocument)
        {
            // todo: изменить выражение (убрать символы &shy;)
            HtmlNodeCollection nodesWithDates = htmlDocument.DocumentNode.SelectNodes("//p[contains(text(), 'Рас­пи­са­ние ба­ка­лав­ров')]");
            HtmlNodeCollection nodesWithUrls = htmlDocument.DocumentNode.SelectNodes("//a[contains(text(), 'скачать')]");
            if (nodesWithDates.Count > 0 && nodesWithUrls.Count > 0 && nodesWithUrls.Count == nodesWithUrls.Count)
            {
                DatesAndUrls parseResult = new DatesAndUrls() { count = nodesWithDates.Count };
                for (int i = 0; i < nodesWithDates.Count; ++i)
                {
                    string nodeText = nodesWithDates[i].InnerText;
                    if (nodeText.Contains("(от "))
                    {
                        parseResult.dates[i] = nodeText.Substring(nodeText.LastIndexOf("(от") + 1, nodeText.LastIndexOf(')') - (nodeText.LastIndexOf("(от") + 1));
                    }
                }
                for (int i = 0; i < nodesWithUrls.Count; ++i)
                {
                    parseResult.urls[i] = nodesWithUrls[i].Attributes["href"].Value.Trim();
                    if (parseResult.urls[i] == "")
                    {
                        parseResult.dates[i] = "";
                    }
                }
                return parseResult;
            }
            else
            {
                //! уведомление об ошибке
                return null;
            }
        }
    }
    public abstract class Department
    {
        public string path;
        private CheckRelevanceStuff checkRelevanceStuff;
        private int coursesCount;
        private Course[] courses;
        public abstract List<int> CheckRelevance();
        public abstract void UpdateSchedule(List<int> coursesToUpdate);
        public abstract List<int> AreScheduleRelevant(DatesAndUrls newDatesAndUrls);
    }
    public class DepartmentITMM : Department
    {
        public string path;
        private VkStuff vkStuff = new VkStuff();
        private CheckRelevanceStuff checkRelevanceStuffITMM = new CheckRelevanceStuffITMM();
        public static Dictionary<string, string> acronymToPhrase;
        public Dictionary<string, string> doubleOptionallySubject;
        public string[] fullName;
        private int coursesAmount;
        private Course[] courses = new Course[5]; // не знаем сколько курсов, определять во время работы
        bool[] isCourseBroken = { false, false, false, false, false };
        public DepartmentITMM(string _path)
        {
            path = _path + "itmm/";
            for (int i = 0; i < 5; ++i)
            {
                courses[i] = new Course(path + i + "_course.xls");
            }
            LoadAcronymToPhrase();
            LoadDoubleOptionallySubject();
            LoadFullName();
        }
        
        public static class ConstructKeyboardsProperties
        {
            public const int buttonsInLine = 2; // 1..4
            public const int linesInKeyboard = 4; // 1..9 
        }

        public void СonstructKeyboards()
        {
            for (int currentCourse = 0; currentCourse < coursesAmount; ++currentCourse)
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


        public void LoadAcronymToPhrase()
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
        public void LoadDoubleOptionallySubject()
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
        public void LoadFullName()
        {
            List<string> fullNames = new List<string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/fullName.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                fullNames.Add(file.ReadLine());
            Glob.fullName = fullNames.ToArray();
        }
        public override List<int> CheckRelevance()
        {
            DatesAndUrls newDatesAndUrls = checkRelevanceStuffITMM.CheckRelevance();
            if (newDatesAndUrls != null)
            {
                coursesCount = newDatesAndUrls.count;
                List<int> coursesToUpdate = AreScheduleRelevant(newDatesAndUrls);
                UpdateSchedule(coursesToUpdate);
            }
            return null;
        }
        public override void UpdateSchedule(List<int> coursesToUpdate)
        {
            for (int i = 0; i < coursesToUpdate.Count; ++i)
            {
                // async
                isCourseBroken[i] = !courses[i].Update();
            }
        }
        public override List<int> AreScheduleRelevant(DatesAndUrls newDatesAndUrls)
        {
            List<int> notRelevantCourses = new List<int>();
            coursesCount = newDatesAndUrls.count;
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
    public class Course
    {
        public static string urlToFile;
        public string date;
        public static string pathToFile;
        //? возможно стоит сделать List<Group>
        public Group[] groups; //??
        public List<MessageKeyboard> keyboards;
        public Course(string _pathToFile)
        {
            pathToFile = _pathToFile;
        }
        // Обновляем расписание, true - успешно, false - не смогли
        public bool Update()
        {
            int count = 0;
            while (!Download())
            {
                ++count;
                if (count == 5)
                    return false;
                Thread.Sleep(60000);
            }
            Group[] newGroups = Parsing.Mapper(pathToFile);
            List<GroupWithSubgroups> notEqualGroupsWithSubgroups = CompareGroups(newGroups);
            
            return true;
        }
        public void ProcessSchedule(List<GroupWithSubgroups> notEqualGroupsWithSubgroups)
        {
            // todo: рисуем и заливаем картинки, формируем список photo_id + group_id + subgroup
        }
        public List<GroupWithSubgroups> CompareGroups(Group[] _groups)
        {
            List<GroupWithSubgroups> notEqualGroupsWithSubgroups = new List<GroupWithSubgroups>();
            for (int i = 0; i < _groups.GetLength(0); ++i)
            {
                for (int j = 0; j < groupsCount; ++j)
                {
                    if (groups[i].name == _groups[j].name)
                    {
                        List<int> notEqualSubgroups = groups[i].ScheduleCompare(_groups[i]);
                        if (notEqualSubgroups.Count != 0)
                            notEqualGroupsWithSubgroups.Add(new GroupWithSubgroups(groups[i].name, notEqualSubgroups));
                        break;
                    }
                }
            }
            // groupsCount != _groups.GetLength(0)
            // foreach (Group gr in newGroups)
            // {

            // }

            return null;
        }
        // Скачиваем новое расписание, true - успешно, false - не удалось скачать
        public static bool Download()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S]  -> Скачивание расписания");
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile(urlToFile, pathToFile);
                return true;
            }
            catch
            {
                return false;
            }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E]  -> Скачивание расписания");
        }
    }
    public class GroupWithSubgroups
    {
        public string name;
        public List<int> subgroups;
        public GroupWithSubgroups(string _name, List<int> _subgroups)
        {
            name = _name;
            subgroups = _subgroups;
        }
    }
    public class Group
    {
        public string name = "";
        public Schedule[] schedule = new Schedule[2]; // 2 подгруппы
        public Group()
        {
            for (int i = 0; i < 2; ++i)
                schedule[i] = new Schedule();
        }
        // Сравнивает расписание, возвращает список несовпадающих подгрупп
        public List<int> ScheduleCompare(Group group)
        {
            List<int> notEqualSubgroups = new List<int>();
            for (int i = 0; i < 2; ++i)
            {
                if (schedule[i] != group.schedule[i])
                {
                    notEqualSubgroups.Add(i);
                }
            }
            return notEqualSubgroups;
        }
    }

    public class DatesAndUrls
    {
        public int count;
        public string[] urls = new string[5];
        public string[] dates = new string[5];
    }

    public interface IDepartment
    {
        public string[] Parse(string str);
    }
    public abstract class CheckRelevanceStuff
    {
        private string url;
        public abstract DatesAndUrls CheckRelevance();
    }
}
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
    
    public class Course
    {
        public static string urlToFile;
        public string date;
        public static string pathToFile;
        public Group[] groups;
        public bool isBroken;
        public bool isUpdating;
        public List<MessageKeyboard> keyboards;
        public Course(string _pathToFile)
        {
            pathToFile = _pathToFile;
            Group[] groups = Parsing.Mapper(pathToFile);
            if (groups == null)
                isBroken = true;
            else
                isBroken = false;
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
                for (int j = 0; j < 11111111; ++j) //! error
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

    public abstract class CheckRelevanceStuff
    {
        private string url;
        public abstract DatesAndUrls CheckRelevance();
    }
}
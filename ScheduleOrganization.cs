using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Threading;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Schedulebot.Parse;
using Schedulebot.Schedule;

namespace Schedulebot
{
    public class CheckRelevanceStuffITMM : ICheckRelevanceStuff
    {
        private const string url = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
        public async Task<DatesAndUrls> CheckRelevance()
        {
            HtmlDocument htmlDocument;
            HtmlWeb htmlWeb = new HtmlWeb();
            try
            {
                htmlDocument = await htmlWeb.LoadFromWebAsync(url);
            }
            catch 
            {
                //! ошибка загрузки страницы
                return null;
            }
            if (htmlDocument != null)
                return await Parse(htmlDocument);
            return null;
        }
        private async Task<DatesAndUrls> Parse(HtmlDocument htmlDocument)
        {
            await Task.Run(() =>
            {
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"\u00ad|", "");
                HtmlNodeCollection nodesWithDates = htmlDocument.DocumentNode.SelectNodes("//p[contains(text(), 'Расписание бакалавров')]");
                HtmlNodeCollection nodesWithUrls = htmlDocument.DocumentNode.SelectNodes("//a[contains(text(), 'скачать')]");
                if (nodesWithDates == null || nodesWithUrls == null)
                {
                    // todo: throw error
                    return null;
                }
                else if (nodesWithDates.Count > 0 && nodesWithUrls.Count > 0 && nodesWithUrls.Count == nodesWithUrls.Count)
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
                return null;
            });
            return null;
        }
    }
    
    public class Course
    {
        public string urlToFile;
        public string date;
        public string pathToFile;
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
        public async Task Update()
        {
            isUpdating = true;
            int triesAmount = 0;
            while (true)
            {
                HttpResponseMessage response = await ScheduleBot.client.GetAsync(urlToFile);
                if (response.IsSuccessStatusCode)
                {
                    using (var fs = new FileStream(pathToFile, FileMode.CreateNew))
                        await response.Content.CopyToAsync(fs);
                    break;
                }
                ++triesAmount;
                if (triesAmount == 5)
                {
                    isBroken = true;
                    isUpdating = false;
                    return;
                }
                await Task.Delay(60000);
            }
            Group[] newGroups = Parsing.Mapper(pathToFile);


            // List<GroupWithSubgroups> notEqualGroupsWithSubgroups = CompareGroups(newGroups);
            
            isBroken = false;
            isUpdating = false;
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
        public ScheduleSubgroup[] schedule = new ScheduleSubgroup[2]; // 2 подгруппы
        public Group()
        {
            for (int i = 0; i < 2; ++i)
                schedule[i] = new ScheduleSubgroup();
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

    public interface ICheckRelevanceStuff
    {
        Task<DatesAndUrls> CheckRelevance();
    }
}
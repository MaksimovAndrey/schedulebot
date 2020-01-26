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
using Schedulebot.Drawing;
using Schedulebot.Vk;

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
        public async void UpdateAsync(string groupUrl, UpdateProperties updateProperties) 
        {
            await Task.Run(async () => 
            {
                isUpdating = true;
                int triesAmount = 0;
                while (true)
                {
                    HttpResponseMessage response = await ScheduleBot.client.GetAsync(urlToFile);
                    if (response.IsSuccessStatusCode)
                    {
                        using (FileStream fileStream = new FileStream(pathToFile, FileMode.CreateNew))
                            await response.Content.CopyToAsync(fileStream);
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
                Group[] newGroups = await Parsing.MapperAsync(pathToFile);
                List<Tuple<int, int>> groupsSubgroupToUpdate = CompareGroups(newGroups);
                groups = newGroups;
                updateProperties.drawingStandartScheduleInfo.date = date;
                for (int i = 0; i < groupsSubgroupToUpdate.Count; i++)
                {
                    groups[groupsSubgroupToUpdate[i].Item1].UpdateAsync(groupsSubgroupToUpdate[i].Item2, updateProperties);
                }
                isBroken = false;
                isUpdating = false;
            });
        }
        
        public void ProcessSchedule(List<Tuple<string, int>> groupSubgroupTuplesToUpdate)
        {
            // todo: рисуем и заливаем картинки, формируем список photo_id + group_id + subgroup
        }
        
        public List<Tuple<int, int>> CompareGroups(Group[] newGroups)
        {
            List<Tuple<int, int>> groupSubgroupTuplesToUpdate = new List<Tuple<int, int>>(); // index of a group, subgroup
            int groupsAmount = groups.GetLength(0);
            int newGroupsAmount = newGroups.GetLength(0);
            for (int currentNewGroup = 0; currentNewGroup < newGroupsAmount; ++currentNewGroup)
            {
                for (int currentGroup = 0; currentGroup < groupsAmount; ++currentGroup)
                {
                    if (groups[currentGroup].name == newGroups[currentNewGroup].name)
                    {
                        List<int> subgroupsToUpdate = groups[currentGroup].CompareSchedule(newGroups[currentNewGroup]);
                        for (int i = 0; i < subgroupsToUpdate.Count; ++i)
                            groupSubgroupTuplesToUpdate.Add(Tuple.Create(currentNewGroup, subgroupsToUpdate[i]));
                        break;
                    }
                }
            }
            return groupSubgroupTuplesToUpdate;
        }
        // Скачиваем новое расписание, true - успешно, false - не удалось скачать
    }
    
    public class Group
    {
        public string name = "";
        public ScheduleSubgroup[] scheduleSubgroups = new ScheduleSubgroup[2]; // 2 подгруппы
        public Group()
        {
            for (int i = 0; i < 2; ++i)
                scheduleSubgroups[i] = new ScheduleSubgroup();
        }
        // Сравнивает расписание, возвращает список несовпадающих подгрупп
        public List<int> CompareSchedule(Group group)
        {
            List<int> notEqualSubgroups = new List<int>();
            for (int i = 0; i < 2; ++i)
            {
                if (scheduleSubgroups[i] != group.scheduleSubgroups[i])
                {
                    notEqualSubgroups.Add(i);
                }
            }
            return notEqualSubgroups;
        }

        public async void UpdateAsync(int subgroup, UpdateProperties updateProperties)
        {
            await Task.Run(() =>
            {
                updateProperties.drawingStandartScheduleInfo.schedule = scheduleSubgroups[subgroup - 1];
                updateProperties.drawingStandartScheduleInfo.group = name;
                updateProperties.drawingStandartScheduleInfo.subgroup = subgroup;
                var test = DrawingSchedule.StandartSchedule.Draw(updateProperties.drawingStandartScheduleInfo);
                updateProperties.photoUploadProperties.Photo
                    = DrawingSchedule.StandartSchedule.Draw(updateProperties.drawingStandartScheduleInfo);

            });
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
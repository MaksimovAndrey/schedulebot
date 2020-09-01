using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using VkNet.Model.Keyboard;
using System.Threading.Tasks;

using Schedulebot.Vk;
using Schedulebot.Parse;
using Schedulebot;

namespace Schedulebot
{
     public class Course
    {
        public string pathToFile;
        public List<Group> groups = new List<Group>();
        public bool isBroken;
        public bool isUpdating = false;
        public List<MessageKeyboard> keyboards;
        
        public Course(string _pathToFile, Dictionaries dictionaries)
        {
            pathToFile = _pathToFile;
            groups = Parsing.MapperAsync(pathToFile, dictionaries).Result;
            if (groups == null)
                isBroken = true;
            else if (groups.Count == 0)
                isBroken = true;
            else
                isBroken = false;
        }

        public async Task<List<PhotoUploadProperties>> UpdateAsync(string urlToFile, string date, UpdateProperties updateProperties, Dictionaries dictionaries) 
        {
            int triesAmount = 0;
            isUpdating = true;
            while (true)
            {
                HttpResponseMessage response = await ScheduleBot.client.GetAsync(urlToFile);
                if (response.IsSuccessStatusCode)
                {
                    using (FileStream fileStream = new FileStream(pathToFile, FileMode.Create))
                        await response.Content.CopyToAsync(fileStream);
                    break;
                }
                ++triesAmount;
                if (triesAmount == 5)
                {
                    isBroken = true;
                    return null;
                }
                await Task.Delay(60000);
            }
            List<Group> newGroups = await Parsing.MapperAsync(pathToFile, dictionaries);
            if (newGroups == null)
            {
                isBroken = true;
                return null;
            }
            else
            {
                // List<(int, int)> groupsSubgroupToUpdate = CompareGroups(ref newGroups);
                List<(int, int, string)> groupsSubgroupToUpdate = CompareGroupsAndGetChanges(ref newGroups);

                groups = newGroups;
                updateProperties.drawingStandartScheduleInfo.date = date;
                List<PhotoUploadProperties> photosToUpload = new List<PhotoUploadProperties>();
                for (int i = 0; i < groupsSubgroupToUpdate.Count; i++)
                {
                    photosToUpload.Add(new PhotoUploadProperties(groups[groupsSubgroupToUpdate[i].Item1].UpdateSubgroup(groupsSubgroupToUpdate[i].Item2, updateProperties)));
                    if (groupsSubgroupToUpdate[i].Item3 != "new")
                        photosToUpload[i].Message += "\n\n• Изменения\n\n" + groupsSubgroupToUpdate[i].Item3;
                }
                isBroken = false;
                return photosToUpload;
            }
        }
        
        public List<(int, int)> CompareGroups(ref List<Group> newGroups)
        {
            List<(int, int)> groupSubgroupTuplesToUpdate = new List<(int, int)>(); // index of a group, subgroup
            for (int currentNewGroup = 0; currentNewGroup < newGroups.Count; ++currentNewGroup)
            {
                for (int currentGroup = 0; currentGroup < groups.Count; ++currentGroup)
                {
                    if (groups[currentGroup].name == newGroups[currentNewGroup].name)
                    {
                        for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
                        {
                            for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                            {
                                for (int currentDay = 0; currentDay < 6; currentDay++)
                                {
                                    if (groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
                                        == newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay])
                                    {
                                        newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].PhotoId
                                            = groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].PhotoId;
                                    }
                                }
                            }
                            if (groups[currentGroup].scheduleSubgroups[currentSubgroup]
                                == newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup])
                            {
                                newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].PhotoId
                                    = groups[currentGroup].scheduleSubgroups[currentSubgroup].PhotoId;
                            }
                            else
                            {
                                groupSubgroupTuplesToUpdate.Add((currentNewGroup, currentSubgroup));
                            }
                        }
                        break;
                    }
                }
            }
            return groupSubgroupTuplesToUpdate;
        }

        public List<(int, int, string)> CompareGroupsAndGetChanges(ref List<Group> newGroups)
        {
            List<(int, int, string)> groupSubgroupTuplesToUpdate = new List<(int, int, string)>(); // index of a group, subgroup
            for (int currentNewGroup = 0; currentNewGroup < newGroups.Count; ++currentNewGroup)
            {
                if (groups == null)
                {
                    groupSubgroupTuplesToUpdate.Add((currentNewGroup, 0, "new"));
                    groupSubgroupTuplesToUpdate.Add((currentNewGroup, 1, "new"));
                    continue;
                }
                for (int currentGroup = 0; currentGroup < groups.Count; ++currentGroup)
                {
                    if (groups[currentGroup].name == newGroups[currentNewGroup].name)
                    {
                        for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
                        {
                            if (groups[currentGroup].scheduleSubgroups[currentSubgroup]
                                == newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup])
                            {
                                newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup]
                                    = groups[currentGroup].scheduleSubgroups[currentSubgroup];
                            }
                            else
                            {
                                StringBuilder changesBuilder = new StringBuilder();
                                for (int currentDay = 0; currentDay < 6; currentDay++)
                                {
                                    if (groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[0].days[currentDay]
                                            != newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[0].days[currentDay]
                                        || groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[1].days[currentDay]
                                            != newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[1].days[currentDay])
                                    {
                                        string currentDayName = "";
                                        switch (currentDay)
                                        {
                                            case 0:
                                            {
                                                currentDayName = "Понедельник";
                                                break;
                                            }
                                            case 1:
                                            {
                                                currentDayName = "Вторник";
                                                break;
                                            }
                                            case 2:
                                            {
                                                currentDayName = "Среда";
                                                break;
                                            }
                                            case 3:
                                            {
                                                currentDayName = "Четверг";
                                                break;
                                            }
                                            case 4:
                                            {
                                                currentDayName = "Пятница";
                                                break;
                                            }
                                            case 5:
                                            {
                                                currentDayName = "Суббота";
                                                break;
                                            }
                                        }

                                        string[] changesWeek = new string[2];
                                        StringBuilder changesWeekBuilder = new StringBuilder();
                                        for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                                        {
                                            for (int currentLecture = 0;
                                                currentLecture < newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].LecturesAmount;
                                                currentLecture++)
                                            {
                                                if (groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture]
                                                    != newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture])
                                                {
                                                    changesWeekBuilder.Append((currentLecture + 1));
                                                    changesWeekBuilder.Append(" пара:\n");
                                                    if (groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture].IsEmpty())
                                                    {
                                                        // changesWeekBuilder.Append("Добавили: ");
                                                        changesWeekBuilder.Append('+');
                                                        changesWeekBuilder.Append(newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture].ConstructLecture());
                                                    }
                                                    else if (newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture].IsEmpty())
                                                    {
                                                        // changesWeekBuilder.Append("Убрали: ");
                                                        changesWeekBuilder.Append('-');
                                                        changesWeekBuilder.Append(groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture].ConstructLecture());
                                                    }
                                                    else
                                                    {
                                                        // changesWeekBuilder.Append("Было:  ");
                                                        changesWeekBuilder.Append('-');
                                                        changesWeekBuilder.Append(groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture].ConstructLecture());
                                                        // changesWeekBuilder.Append("\nСтало: ");
                                                        changesWeekBuilder.Append("\n+");
                                                        changesWeekBuilder.Append(newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures[currentLecture].ConstructLecture());
                                                    }
                                                    changesWeekBuilder.Append('\n');
                                                }
                                            }
                                            changesWeek[currentWeek] = changesWeekBuilder.ToString();
                                            changesWeekBuilder.Clear();
                                        }

                                        if (changesWeek[0] == changesWeek[1])
                                        {
                                            if (changesWeek[0] != "")
                                            {
                                                changesBuilder.Append("· ");
                                                changesBuilder.Append(currentDayName);
                                                changesBuilder.Append('\n');
                                                changesBuilder.Append(changesWeek[0]);
                                                changesBuilder.Append('\n');
                                            }
                                        }
                                        else
                                        {
                                            for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                                            {
                                                if (changesWeek[currentWeek] != "")
                                                {
                                                    changesBuilder.Append("· ");
                                                    changesBuilder.Append(currentDayName);
                                                    changesBuilder.Append(" (");
                                                    changesBuilder.Append(currentWeek == 0 ? "верхняя" : "нижняя");
                                                    changesBuilder.Append(")\n");
                                                    changesBuilder.Append(changesWeek[currentWeek]);
                                                    changesBuilder.Append('\n');
                                                }
                                            }
                                        }
                                    }
                                    // тут сохраняем в новое расписание загруженные фотки
                                    for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                                    {
                                        if (groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
                                            == newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay])
                                        {
                                            newGroups[currentNewGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
                                                = groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks[currentWeek].days[currentDay];
                                        }
                                    }
                                }
                                groupSubgroupTuplesToUpdate.Add((currentNewGroup, currentSubgroup, changesBuilder.ToString()));
                            }
                        }
                        break;
                    }
                }
            }
            return groupSubgroupTuplesToUpdate;
        }
    }
}
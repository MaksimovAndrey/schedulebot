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
        public List<string> PathsToFile { get; set; }
        public List<Group> groups = new List<Group>();
        public bool isBroken;
        public bool isUpdating = false;
        public List<MessageKeyboard> keyboards;
        
        public Course(List<string> pathsToFile, Dictionaries dictionaries)
        {
            PathsToFile = pathsToFile;
            groups = Parsing.MapperAsync(PathsToFile, dictionaries).Result;
            if (groups == null || groups.Count == 0)
                isBroken = true;
            else
                isBroken = false;
        }

        public async Task<List<PhotoUploadProperties>> UpdateAsync(string date, UpdateProperties updateProperties, Dictionaries dictionaries) 
        {
            isUpdating = true;
            List<Group> newGroups = await Parsing.MapperAsync(PathsToFile, dictionaries);
            if (newGroups == null || newGroups.Count == 0)
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
                    photosToUpload.Add(new PhotoUploadProperties(
                        groups[groupsSubgroupToUpdate[i].Item1].UpdateSubgroup(
                            groupsSubgroupToUpdate[i].Item2, updateProperties)));
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
                                    if (groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
                                        == newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay])
                                    {
                                        newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay].PhotoId
                                            = groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay].PhotoId;
                                    }
                                }
                            }
                            if (groups[currentGroup].subgroups[currentSubgroup]
                                == newGroups[currentNewGroup].subgroups[currentSubgroup])
                            {
                                newGroups[currentNewGroup].subgroups[currentSubgroup].PhotoId
                                    = groups[currentGroup].subgroups[currentSubgroup].PhotoId;
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
                if (groups == null || groups.Count == 0)
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
                            if (groups[currentGroup].subgroups[currentSubgroup]
                                == newGroups[currentNewGroup].subgroups[currentSubgroup])
                            {
                                newGroups[currentNewGroup].subgroups[currentSubgroup]
                                    = groups[currentGroup].subgroups[currentSubgroup];
                            }
                            else
                            {
                                StringBuilder changesBuilder = new StringBuilder();
                                for (int currentDay = 0; currentDay < 6; currentDay++)
                                {
                                    if (groups[currentGroup].subgroups[currentSubgroup].weeks[0].days[currentDay]
                                            != newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[0].days[currentDay]
                                        || groups[currentGroup].subgroups[currentSubgroup].weeks[1].days[currentDay]
                                            != newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[1].days[currentDay])
                                    {
                                        string currentDayName = Utils.Converter.IndexToDay(currentDay);

                                        string[] changesWeek = new string[2];
                                        for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                                        {
                                            changesWeek[currentWeek] = 
                                                groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay].GetChanges(
                                                    newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay].lectures);
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
                                        if (groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
                                            == newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay])
                                        {
                                            newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
                                                = groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay];
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
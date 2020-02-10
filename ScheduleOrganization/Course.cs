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
        public string urlToFile;
        public string date;
        public string pathToFile;
        public List<Group> groups = new List<Group>();
        public bool isBroken;
        public bool isUpdating = false;
        public List<MessageKeyboard> keyboards;
        
        public Course(string _pathToFile, Dictionaries dictionaries)
        {
            pathToFile = _pathToFile;
            groups = Parsing.Mapper(pathToFile, dictionaries);
            if (groups == null)
                isBroken = true;
            else if (groups.Count == 0)
                isBroken = true;
            else
                isBroken = false;
        }

        public async Task<List<PhotoUploadProperties>> UpdateAsync(UpdateProperties updateProperties, Dictionaries dictionaries) 
        {
            return await Task.Run(async () => 
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
                    List<(int, int)> groupsSubgroupToUpdate = CompareGroups(ref newGroups);
                    groups = newGroups;
                    updateProperties.drawingStandartScheduleInfo.date = date;
                    List<PhotoUploadProperties> photosToUpload = new List<PhotoUploadProperties>();
                    for (int i = 0; i < groupsSubgroupToUpdate.Count; i++)
                    {
                        photosToUpload.Add(new PhotoUploadProperties(groups[groupsSubgroupToUpdate[i].Item1].UpdateSubgroup(groupsSubgroupToUpdate[i].Item2, updateProperties)));
                    }
                    isBroken = false;
                    return photosToUpload;
                }
            });
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
    }
}
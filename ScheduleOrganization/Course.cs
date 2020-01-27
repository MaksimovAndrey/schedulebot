using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using VkNet.Model.Keyboard;
using System.Threading.Tasks;

using Schedulebot.Vk;
using Schedulebot.Parse;

namespace Schedulebot
{
     public class Course
    {
        public string urlToFile;
        public string date;
        public string pathToFile;
        public List<Group> groups = new List<Group>();
        public bool isBroken;
        public bool isUpdating;
        public List<MessageKeyboard> keyboards;
        public Course(string _pathToFile)
        {
            pathToFile = _pathToFile;
            groups = Parsing.Mapper(pathToFile);
            if (groups.Count == 0)
                isBroken = true;
            else
                isBroken = false;
        }
        // Обновляем расписание, true - успешно, false - не смогли
        public async Task<List<PhotoUploadProperties>> UpdateAsync(string groupUrl, UpdateProperties updateProperties) 
        {
            return await Task.Run(async () => 
            {
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
                        return null;
                    }
                    await Task.Delay(60000);
                }
                List<Group> newGroups = await Parsing.MapperAsync(pathToFile);
                List<Tuple<int, int>> groupsSubgroupToUpdate = CompareGroups(newGroups);
                groups = newGroups;
                updateProperties.drawingStandartScheduleInfo.date = date;
                List<Task<PhotoUploadProperties>> tasks = new List<Task<PhotoUploadProperties>>();
                for (int i = 0; i < groupsSubgroupToUpdate.Count; i++)
                {
                    tasks.Add(groups[groupsSubgroupToUpdate[i].Item1].UpdateAsync(groupsSubgroupToUpdate[i].Item2, updateProperties));
                }
                await Task.WhenAll(tasks);
                isBroken = false;
                List<PhotoUploadProperties> photosToUpload = new List<PhotoUploadProperties>();
                for (int i = 0; i < tasks.Count; i++)
                    photosToUpload.Add(tasks[i].Result);
                return photosToUpload;
            });
        }
        
        public void ProcessSchedule(List<Tuple<string, int>> groupSubgroupTuplesToUpdate)
        {
            // todo: рисуем и заливаем картинки, формируем список photo_id + group_id + subgroup
        }
        
        public List<Tuple<int, int>> CompareGroups(List<Group> newGroups)
        {
            List<Tuple<int, int>> groupSubgroupTuplesToUpdate = new List<Tuple<int, int>>(); // index of a group, subgroup
            for (int currentNewGroup = 0; currentNewGroup < newGroups.Count; ++currentNewGroup)
            {
                for (int currentGroup = 0; currentGroup < groups.Count; ++currentGroup)
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

}
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

using Schedulebot.Schedule;
using Schedulebot.Drawing;
using Schedulebot.Vk;

namespace Schedulebot
{
    public class Group
    {
        public string name = "";
        public ScheduleSubgroup[] scheduleSubgroups = new ScheduleSubgroup[2]; // 2 подгруппы
        public ulong[] photoIds = { 0, 0 };
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

        public async Task<PhotoUploadProperties> UpdateAsync(int subgroup, UpdateProperties updateProperties)
        {
            return await Task.Run(() => 
            {
                updateProperties.drawingStandartScheduleInfo.schedule = scheduleSubgroups[subgroup - 1];
                updateProperties.drawingStandartScheduleInfo.group = name;
                updateProperties.drawingStandartScheduleInfo.subgroup = subgroup;
                var test = DrawingSchedule.StandartSchedule.Draw(updateProperties.drawingStandartScheduleInfo);
                updateProperties.photoUploadProperties.Photo
                    = DrawingSchedule.StandartSchedule.Draw(updateProperties.drawingStandartScheduleInfo);
                return updateProperties.photoUploadProperties;
            });
        }
    }
}
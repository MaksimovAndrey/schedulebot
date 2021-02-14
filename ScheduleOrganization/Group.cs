using Schedulebot.Schedule;
using Schedulebot.Vk;
using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot
{
    public class Group
    {
        public string Name { get; }
        public List<ScheduleDay> days;
        public DateTime LastTimeUpdated { get; set; } = DateTime.MinValue;
        public bool IsUpdating { get; set; } = false;

        public Group(string name)
        {
            Name = name;
            days = new List<ScheduleDay>();
        }

        /// <summary>
        /// !!!НЕ РАБОТАЕТ!!!
        /// Ищет изменения в расписании этой группы, а также сохраняет актуальные фотографии
        /// </summary>
        /// <param name="oldGroup">Устаревшая группа</param>
        /// <returns>Список (подгруппа, изменения)</returns>
        //public List<(int, string)> GetChangesAndKeepActualPhotoIds(Group oldGroup)
        //{
        //    List<(int, string)> subgroupChangesTuples = new List<(int, string)>();
        //    for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
        //    {
        //        if (subgroups[currentSubgroup] == oldGroup.subgroups[currentSubgroup])
        //        {
        //            subgroups[currentSubgroup] = oldGroup.subgroups[currentSubgroup];
        //        }
        //        else
        //        {
        //            subgroupChangesTuples.Add((
        //                currentSubgroup,
        //                subgroups[currentSubgroup].GetChangesAndKeepActualPhotoIds(
        //                    oldGroup.subgroups[currentSubgroup])));
        //        }
        //    }
        //    return subgroupChangesTuples;
        //}

        //public void DrawSubgroupSchedule(int subgroup, ref UpdateProperties updateProperties)
        //{
        //    updateProperties.drawingStandartScheduleInfo.weeks = subgroups[subgroup].weeks;
        //    updateProperties.drawingStandartScheduleInfo.group = name;
        //    updateProperties.drawingStandartScheduleInfo.subgroup = subgroup + 1;

        //    updateProperties.photoUploadProperties.Photo
        //        = Drawing.Standart.Drawer.Draw(updateProperties.drawingStandartScheduleInfo);

        //    StringBuilder stringBuilder = new StringBuilder();
        //    stringBuilder.Append("Новое расписание для ");
        //    stringBuilder.Append(name);
        //    stringBuilder.Append(" (");
        //    stringBuilder.Append((subgroup + 1));
        //    stringBuilder.Append(") ");
        //    stringBuilder.Append(updateProperties.drawingStandartScheduleInfo.date);

        //    updateProperties.photoUploadProperties.Message = stringBuilder.ToString();
        //    updateProperties.photoUploadProperties.GroupName = name;
        //    updateProperties.photoUploadProperties.Subgroup = subgroup;
        //    updateProperties.photoUploadProperties.UploadingSchedule = UploadingSchedule.Week;

        //    //return updateProperties;
        //}
    }
}
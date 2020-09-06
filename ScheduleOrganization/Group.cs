using System.Collections.Generic;
using System.Text;
using Schedulebot.Schedule;
using Schedulebot.Drawing;
using Schedulebot.Vk;

namespace Schedulebot
{
    public class Group
    {
        public string name = "";
        public ScheduleSubgroup[] subgroups = new ScheduleSubgroup[2];
        public int SubgroupsCount { get; }
        
        public Group(int subgroupsCount = 2)
        {
            SubgroupsCount = subgroupsCount;
            for (int i = 0; i < SubgroupsCount; ++i)
                subgroups[i] = new ScheduleSubgroup();
        }

        public void EqualizeSubgroups(int whichToCopy = 0)
        {
            subgroups[whichToCopy + 1 % 2] = subgroups[whichToCopy];
        }

        public PhotoUploadProperties UpdateSubgroup(int subgroup, UpdateProperties updateProperties)
        {
            updateProperties.drawingStandartScheduleInfo.weeks = subgroups[subgroup].weeks;
            updateProperties.drawingStandartScheduleInfo.group = name;
            updateProperties.drawingStandartScheduleInfo.subgroup = subgroup + 1;
            
            updateProperties.photoUploadProperties.Photo
                = DrawingSchedule.StandartSchedule.Draw(updateProperties.drawingStandartScheduleInfo);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Новое расписание для ");
            stringBuilder.Append(name);
            stringBuilder.Append(" (");
            stringBuilder.Append((subgroup + 1));
            stringBuilder.Append(") ");
            stringBuilder.Append(updateProperties.drawingStandartScheduleInfo.date);
            
            updateProperties.photoUploadProperties.Message = stringBuilder.ToString();
            updateProperties.photoUploadProperties.GroupName = name;
            updateProperties.photoUploadProperties.Subgroup = subgroup;
            updateProperties.photoUploadProperties.UploadingSchedule = UploadingSchedule.Week;

            return updateProperties.photoUploadProperties;
        }

        public void SortLectures()
        {
            for (int i = 0; i < SubgroupsCount; i++)
            {
                subgroups[i].SortLectures();
            }
        }
    }
}
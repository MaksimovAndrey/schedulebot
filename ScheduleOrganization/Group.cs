using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Schedulebot.Schedule;
using Schedulebot.Drawing;
using Schedulebot.Vk;

namespace Schedulebot
{
    public class Group
    {
        public string name = "";
        public ScheduleSubgroup[] scheduleSubgroups = new ScheduleSubgroup[2];
        
        public Group()
        {
            for (int i = 0; i < 2; ++i)
                scheduleSubgroups[i] = new ScheduleSubgroup();
        }

        public List<int> CompareSubgroupsSchedule(Group group)
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

        public PhotoUploadProperties UpdateSubgroup(int subgroup, UpdateProperties updateProperties)
        {
            updateProperties.drawingStandartScheduleInfo.schedule = scheduleSubgroups[subgroup];
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
            updateProperties.photoUploadProperties.Group = name;
            updateProperties.photoUploadProperties.Subgroup = subgroup;

            return updateProperties.photoUploadProperties;
        }
    }
}
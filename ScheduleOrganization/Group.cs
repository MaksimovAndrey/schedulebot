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

        public async Task<PhotoUploadProperties> UpdateSubgroupAsync(int subgroup, UpdateProperties updateProperties)
        {
            return await Task.Run(() => 
            {
                updateProperties.drawingStandartScheduleInfo.schedule = scheduleSubgroups[subgroup - 1];
                updateProperties.drawingStandartScheduleInfo.group = name;
                updateProperties.drawingStandartScheduleInfo.subgroup = subgroup;
                updateProperties.photoUploadProperties.Photo
                    = DrawingSchedule.StandartSchedule.Draw(updateProperties.drawingStandartScheduleInfo);

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Новое расписание для ");
                stringBuilder.Append(updateProperties.photoUploadProperties.Group);
                stringBuilder.Append(" (");
                stringBuilder.Append(updateProperties.photoUploadProperties.Subgroup);
                stringBuilder.Append(") ");
                stringBuilder.Append(updateProperties.drawingStandartScheduleInfo.date);
                updateProperties.photoUploadProperties.Message = stringBuilder.ToString();

                return updateProperties.photoUploadProperties;
            });
        }
    }
}
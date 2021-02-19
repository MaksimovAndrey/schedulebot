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
        public List<ScheduleDay> uploadedDays;
        public DateTime LastTimeUpdated { get; set; } = DateTime.MinValue;
        public bool IsUpdating { get; set; } = false;

        public Group(string name)
        {
            Name = name;
            days = new List<ScheduleDay>();
            uploadedDays = new List<ScheduleDay>();
        }

        public void CheckForUploadedDays()
        {
            for (int day = 0; day < days.Count; day++)
            {
                for (int uploadedDay = 0; uploadedDay < uploadedDays.Count; uploadedDay++)
                {
                    if (days[day].EqualDay(uploadedDays[uploadedDay]))
                    {
                        days[day].PhotoId = uploadedDays[uploadedDay].PhotoId;
                        break;
                    }
                }
            }
        }
    }
}
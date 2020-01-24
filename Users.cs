using System;
using System.Collections.Generic;

namespace Schedulebot
{
    public class Users
    {
        List<User> users = new List<User>();

        public List<long> GetUserIds(ScheduleSettings scheduleSettings)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].scheduleSettings == scheduleSettings)
                    return users[i].userIds;
            }
            return null;
        }
    }
    public class User
    {
        public List<long> userIds;
        public ScheduleSettings scheduleSettings;
    }

    public class ScheduleSettings : IEquatable<ScheduleSettings>
    {
        public string group = null;
        public int subgroup = 0;

        public ScheduleSettings() { }
        public ScheduleSettings(string _group, int _subgroup) { group = _group; subgroup = _subgroup; }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as ScheduleSettings);
        }

        public bool Equals(ScheduleSettings other)
        {
            return other != null && group == other.group && subgroup == other.subgroup;
        }

        public override int GetHashCode()
        {
            var hashCode = 390074312;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(group);
            hashCode = hashCode * -1521134295 + EqualityComparer<int>.Default.GetHashCode(subgroup);
            return hashCode;
        }
    }
}
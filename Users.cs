using System;
using System.Collections.Generic;

namespace Schedulebot
{
    public class Users
    {
        List<UserGroup> userGroups = new List<UserGroup>();

        public List<long> GetUserIds(string group, int subgroup)
        {
            for (int i = 0; i < userGroups.Count; i++)
            {
                if (userGroups[i].group == group && userGroups[i].subgroup == subgroup)
                    return userGroups[i].userIds;
            }
            return null;
        }
    }

    public class UserGroup
    {
        public List<long> userIds = new List<long>();
        public string group = null;
        public int subgroup = 0;

        public UserGroup(string _group, int _subgroup)
        {
            group = _group;
            subgroup = _subgroup;
        }
    }
}
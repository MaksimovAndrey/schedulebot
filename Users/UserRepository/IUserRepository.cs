using Schedulebot.Users.Enums;
using System.Collections.Generic;


namespace Schedulebot.Users
{
    public interface IUserRepository
    {
        AddOrEditResult AddOrEdit(long? id, string group, int subgroup);
        void Add(User user);
        bool Delete(long? id);
        bool ChangeSubgroup(long? id, out User user);
        bool Contains(long? id);

        bool Get(long? id, out User user);

        List<long> GetIds();
        List<long> GetIds(string group);
        List<long> GetIds(string group, int subgroup);
        List<long> GetIds(List<(string, int)> oldGroupSubgroupList);
        List<long> GetIds(List<string> groupNames);

        void RemoveLastMessageId(long id);
        void RemoveLastMessageId(List<long> ids);
        long GetAndRemoveLastMessageId(long id);
        long GetLastMessageId(long id);

        void SetLastMessageId(long id, long messageId);
        void SetLastMessageId(long[] ids, long[] lastMessageIds);
    }
}

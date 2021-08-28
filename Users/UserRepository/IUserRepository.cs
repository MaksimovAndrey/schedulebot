using Schedulebot.Users.Enums;
using System.Collections.Generic;


namespace Schedulebot.Users
{
    public interface IUserRepository
    {
        AddOrEditResult AddOrEdit(long? id, string group, int subgroup, bool isActive = true);
        void Add(User user);
        bool Disable(long? id);
        bool TryChangeSubgroup(long? id, out User user);
        bool Contains(long? id, bool onlyActive = true);

        bool TryGet(long? id, out User user);

        List<long> GetIds(bool onlyActive = true);
        List<long> GetIds(string group, bool onlyActive = true);
        List<long> GetIds(string group, int subgroup, bool onlyActive = true);
        List<long> GetIds(List<string> groupNames, bool onlyActive = true);

        void RemoveLastMessageId(long id);
        void RemoveLastMessageId(List<long> ids);
        long GetAndRemoveLastMessageId(long id);
        long GetLastMessageId(long id);

        void SetLastMessageId(long id, long messageId);
        void SetLastMessageId(long[] ids, long[] lastMessageIds);
    }
}

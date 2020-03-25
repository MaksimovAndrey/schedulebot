using System.Collections.Generic;

namespace Schedulebot.Users
{
    public interface IUserRepository
    {
        void SaveUsers(string path);
        void AddUser(User user);
        bool DeleteUser(long id);
        void EditUser(long id, int newSubgroup, string newGroup = null);
        User ChangeSubgroup(long? id);
        bool ContainsUser(long? id);
        List<long> GetIds(int course, Mapper mapper);
        List<long> GetIds(string group);
        List<long> GetIds(string group, int subgroup);
    }
}
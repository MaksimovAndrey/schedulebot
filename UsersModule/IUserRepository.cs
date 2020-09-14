using System.Collections.Generic;
using Schedulebot.Users.Enums;

namespace Schedulebot.Users
{
    public interface IUserRepository
    {
        void SaveUsers(string path);
        AddOrEditUserResult AddOrEditUser(long? id, string group, int subgroup);
        void AddUser(User user);
        bool DeleteUser(long? id);
        bool ChangeSubgroup(long? id, out User user);
        bool ContainsUser(long? id);
        List<long> GetIds(int course, Mapper.Mapper mapper);
        List<long> GetIds(string group);
        List<long> GetIds(string group, int subgroup);
    }
}
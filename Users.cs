using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot
{
    public class User
    {
        public long Id { get; set; }
        public string Group { get; set; }
        public int Subgroup { get; set; }

        public User(long id, string group, int subgroup)
        {
            Id = id;
            Group = group;
            Subgroup = subgroup;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Id);
            stringBuilder.Append(' ');
            stringBuilder.Append(Group);
            stringBuilder.Append(' ');
            stringBuilder.Append(Subgroup);
            return stringBuilder.ToString();
        }

        public static bool TryParseUser(string rawUserLine, out User user)
        {
            try
            {
                var rawUserSpan = rawUserLine.AsSpan();

                int spaceIndex = rawUserSpan.IndexOf(' ');
                int lastSpaceIndex = rawUserSpan.LastIndexOf(' ');

                long id = Int64.Parse(rawUserSpan.Slice(0, spaceIndex));
                string group = rawUserSpan.Slice(spaceIndex + 1, lastSpaceIndex - spaceIndex - 1).ToString();
                int subgroup = int.Parse(rawUserSpan.Slice(lastSpaceIndex + 1, 1));

                user = new User(id, group, subgroup);
                return true;
            }
            catch
            {
                Console.WriteLine("\nЕсли ты это видишь, то ты, наверное, на голову ебнутый.\nСтруктура такая: long id \" \" string group \" \" int subgroup \"\\n\"");
                user = null;
                return false;
            }
        }
    }

    public class UserRepository : IUserRepository
    {
        private readonly List<User> users = new List<User>();
        public int UsersCount => users.Count;
        
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < users.Count; i++)
            {
                stringBuilder.Append(users[i].ToString());
                stringBuilder.Append('\n');
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }

        public User GetUser(long? id)
        {
            for (int currentUser = 0; currentUser < UsersCount; currentUser++)
            {
                if (users[currentUser].Id == id)
                    return users[currentUser];
            }
            return null;
        }
        
        public List<long> GetIds(int course, Mapper mapper)
        {
            List<string> groupNames = mapper.GetGroupNames(course);
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < UsersCount; currentUser++)
            {
                if (groupNames.Contains(users[currentUser].Group))
                    ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(string group)
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < UsersCount; currentUser++)
            {
                if (users[currentUser].Group == group)
                    ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(string group, int subgroup)
        {
            List<long> ids = new List<long>();
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Group == group && users[i].Subgroup == subgroup)
                {
                    ids.Add(users[i].Id);
                }
            }
            return ids;
        }

        public void AddUser(User user)
        {
            users.Add(user);
        }

        public void DeleteUser(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users.RemoveAt(i);
                    return;
                }
            }
        }

        public void EditUser(long id, int newSubgroup, string newGroup = null)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                     users[i].Subgroup = newSubgroup;
                    if (newGroup != null)
                        users[i].Group = newGroup;
                    return;
                }
            }
        }

        public User ChangeSubgroup(long? id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users[i].Subgroup = users[i].Subgroup % 2 + 1;
                    return users[i];
                }
            }
            return null;
        }

        public bool ContainsUser(long? id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public interface IUserRepository
    {
        void AddUser(User user);
        void DeleteUser(long id);
        void EditUser(long id, int newSubgroup, string newGroup = null);
        User ChangeSubgroup(long? id);
        bool ContainsUser(long? id);
        List<long> GetIds(int course, Mapper mapper);
        List<long> GetIds(string group);
        List<long> GetIds(string group, int subgroup);
    }
}
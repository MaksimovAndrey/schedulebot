using System.Collections.Generic;
using System.IO;
using System.Text;
using Schedulebot.Users.Enums;

namespace Schedulebot.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> users = new List<User>();

        public UserRepository(string path)
        {
            LoadUsers(path);
        }

        public AddOrEditUserResult AddOrEditUser(long? id, string group, int subgroup)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users[i].Group = group;
                    users[i].Subgroup = subgroup;
                    return AddOrEditUserResult.Edited;
                }
            }
            users.Add(new User((long)id, group, subgroup));
            return AddOrEditUserResult.Added;
        }

        private void LoadUsers(string path)
        {
            using StreamReader file = new StreamReader(path, Encoding.Default);
            while (!file.EndOfStream)
            {
                if (User.TryParseUser(file.ReadLine(), out var user))
                    AddUser(user);
            }
        }

        public void SaveUsers(string path)
        {
            using StreamWriter file = new StreamWriter(path + "users.txt");
            file.WriteLine(this.ToString());
        }


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

        public bool GetUser(long? id, out User user)
        {
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (users[currentUser].Id == id)
                {
                    user = users[currentUser];
                    return true;
                }
            }
            user = null;
            return false;
        }

        public List<long> GetIds()
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(List<(string, int)> oldGroupSubgroupList)
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                for (int currentOldGroupSubgroup = 0; currentOldGroupSubgroup < oldGroupSubgroupList.Count; currentOldGroupSubgroup++)
                {
                    if (users[currentUser].Group == oldGroupSubgroupList[currentOldGroupSubgroup].Item1
                        && users[currentUser].Subgroup == oldGroupSubgroupList[currentOldGroupSubgroup].Item2)
                    {
                        ids.Add(users[currentUser].Id);
                        break;
                    }
                }
            }
            return ids;
        }

        public List<long> GetIds(int course, Mapper.Mapper mapper)
        {
            List<string> groupNames = mapper.GetGroupNames(course);
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (groupNames.Contains(users[currentUser].Group))
                    ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(string group)
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
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

        public void AddUser(long id, string group, int subgroup)
        {
            users.Add(new User(id, group, subgroup));
        }

        /// <summary>
        /// ”дал€ет пользовател€ из базы
        /// </summary>
        /// <param name="id">Vk Id пользовател€</param>
        /// <returns>
        /// <see langword="true"/> если пользователь удалЄн
        /// <br><see langword="false"/> если пользовател€ не было в базе</br>
        /// </returns>
        public bool DeleteUser(long? id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool ChangeSubgroup(long? id, out User user)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users[i].Subgroup = users[i].Subgroup % 2 + 1;
                    user = users[i];
                    return true;
                }
            }
            user = null;
            return false;
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
}
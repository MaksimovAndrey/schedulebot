using Schedulebot.Users.Enums;
using Schedulebot.Users.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> users;

        public UserRepository()
        {
            this.users = new List<User>();
        }

        public AddOrEditResult AddOrEdit(long? id, string group, int subgroup, bool isActive = true)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users[i].Group = group;
                    users[i].Subgroup = subgroup;
                    users[i].IsActive = isActive;
                    return AddOrEditResult.Edited;
                }
            }
            users.Add(new User((long)id, group, subgroup, isActive: isActive));
            return AddOrEditResult.Added;
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

        public void SetLastMessageId(long id, long lastMessageId)
        {
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (users[currentUser].Id == id)
                {
                    users[currentUser].LastMessageId = lastMessageId;
                    return;
                }
            }
        }

        public void SetLastMessageId(long[] userIds, long[] lastMessageIds)
        {
            for (int i = userIds.Length - 1; i >= 0; i--)
            {
                if (userIds[i] == 0)
                    continue;

                for (int j = 0; j < i; j++)
                    if (userIds[j] != 0 && userIds[i] == userIds[j])
                        userIds[j] = 0;

                for (int currentUser = 0; currentUser < users.Count; currentUser++)
                {
                    if (users[currentUser].Id == userIds[i])
                    {
                        users[currentUser].LastMessageId = lastMessageIds[i];
                        break;
                    }
                }
            }
        }

        public void SetLastMessageInfo(long id, long lastMessageId, DateTime lastMessageTime)
        {
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (users[currentUser].Id == id)
                {
                    users[currentUser].LastMessageId = lastMessageId;
                    users[currentUser].LastMessageTime = lastMessageTime;
                    return;
                }
            }
        }

        public void SetLastMessageInfo(long[] userIds, long[] lastMessageIds, DateTime lastMessageTime)
        {
            for (int i = userIds.Length - 1; i >= 0; i--)
            {
                if (userIds[i] == 0)
                    continue;

                for (int j = 0; j < i; j++)
                    if (userIds[j] != 0 && userIds[i] == userIds[j])
                        userIds[j] = 0;

                for (int currentUser = 0; currentUser < users.Count; currentUser++)
                {
                    if (users[currentUser].Id == userIds[i])
                    {
                        users[currentUser].LastMessageId = lastMessageIds[i];
                        users[currentUser].LastMessageTime = lastMessageTime;
                        break;
                    }
                }
            }
        }


        public bool TryGet(long? id, out User user)
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

        public List<long> GetIds(bool onlyActive = true)
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (users[currentUser].IsActive || !onlyActive)
                    ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(List<string> groupNames, bool onlyActive)
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (groupNames.Contains(users[currentUser].Group) && (users[currentUser].IsActive || !onlyActive))
                    ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(string group, bool onlyActive = true)
        {
            List<long> ids = new List<long>();
            for (int currentUser = 0; currentUser < users.Count; currentUser++)
            {
                if (users[currentUser].Group == group && (users[currentUser].IsActive || !onlyActive))
                    ids.Add(users[currentUser].Id);
            }
            return ids;
        }

        public List<long> GetIds(string group, int subgroup, bool onlyActive = true)
        {
            List<long> ids = new List<long>();
            for (int i = 0; i < users.Count; i++)
                if (users[i].Group == group && users[i].Subgroup == subgroup && (users[i].IsActive || !onlyActive))
                    ids.Add(users[i].Id);

            return ids;
        }

        public void Add(User user)
        {
            users.Add(user);
        }

        public void Add(long id, string group, int subgroup, long lastMessageId = 0, bool isActive = true)
        {
            users.Add(new User(id, group, subgroup, lastMessageId, isActive: isActive));
        }

        /// <summary>
        /// Изменяет статус пользователя на "Неактивный"
        /// </summary>
        /// <param name="id">Vk Id пользователя</param>
        /// <returns>
        /// <see langword="true"/> если статус пользователя изменен
        /// <br><see langword="false"/> если пользователя не было в базе или статус пользователя "Неактивный"</br>
        /// </returns>
        public bool Disable(long? id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    if (users[i].IsActive)
                    {
                        users[i].IsActive = false;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return false;
        }

        public bool TryChangeSubgroup(long? id, out User user)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    if (users[i].IsActive)
                    {
                        users[i].Subgroup = users[i].Subgroup % 2 + 1;
                        user = users[i];
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            user = null;
            return false;
        }

        public bool Contains(long? id, bool onlyActive = true)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    if (onlyActive && !users[i].IsActive)
                        return false;
                    else
                        return true;
                }
            }
            return false;
        }

        public void RemoveLastMessageId(List<long> ids)
        {
            for (int i = 0; i < ids.Count; i++)
                RemoveLastMessageId(ids[i]);
        }

        public void RemoveLastMessageId(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users[i].LastMessageId = 0;
                    return;
                }
            }
        }

        public long GetAndRemoveLastMessageId(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    long lastMessageId = users[i].LastMessageId;
                    users[i].LastMessageId = 0;
                    return lastMessageId;
                }
            }
            return 0;
        }

        public long GetLastMessageId(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    return users[i].LastMessageId;
                }
            }
            return 0;
        }

        public MessageInfo GetAndRemoveLastMessageInfo(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    long lastMessageId = users[i].LastMessageId;
                    users[i].LastMessageId = 0;
                    return new MessageInfo(lastMessageId, users[i].LastMessageTime);
                }
            }
            return default;
        }

        public MessageInfo GetLastMessageInfo(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    return new MessageInfo(users[i].LastMessageId, users[i].LastMessageTime);
                }
            }
            return default;
        }
    }
}

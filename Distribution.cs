using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using Newtonsoft.Json;

namespace schedulebot
{
    public static class Distribution
    {
        public static void ToGroupSubgroup(string group, string subgroup, string message)
        {
            Random random = new Random();
            List<long> userIds = new List<long>();
            MessagesSendParams messagesSendParams;
            int count = 0;
            lock (Glob.locker)
            {
                int usersCount = Glob.users.Count;
                for (int i = 0; i < usersCount; ++i)
                {
                    if (Glob.users.ElementAt(i).Value.Group == group && Glob.users.ElementAt(i).Value.Subgroup == subgroup)
                    {
                        userIds.Add((long)Glob.users.ElementAt(i).Key);
                        ++count;
                        if (count == 100)
                        {
                            messagesSendParams = new MessagesSendParams()
                            {
                                UserIds = userIds,
                                Message = message,
                                RandomId = random.Next()
                            };
                            count = 0;
                            Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                            userIds.Clear();
                        }
                    }
                }
                if (count > 0)
                {
                    messagesSendParams = new MessagesSendParams()
                    {
                        UserIds = userIds,
                        Message = message,
                        RandomId = random.Next()
                    };
                    Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                    userIds.Clear();
                }
            }
        }
        public static void ToGroup(string group, string message)
        {
            Random random = new Random();
            List<long> userIds = new List<long>();
            MessagesSendParams messagesSendParams;
            int count = 0;
            lock (Glob.locker)
            {
                int usersCount = Glob.users.Count;
                for (int i = 0; i < usersCount; ++i)
                {
                    if (Glob.users.ElementAt(i).Value.Group == group)
                    {
                        userIds.Add((long)Glob.users.ElementAt(i).Key);
                        ++count;
                        if (count == 100)
                        {
                            messagesSendParams = new MessagesSendParams()
                            {
                                UserIds = userIds,
                                Message = message,
                                RandomId = random.Next()
                            };
                            count = 0;
                            Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                            userIds.Clear();
                        }
                    }
                }
                if (count > 0)
                {
                    messagesSendParams = new MessagesSendParams()
                    {
                        UserIds = userIds,
                        Message = message,
                        RandomId = random.Next()
                    };
                    Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                    userIds.Clear();
                }
            }
        }
        public static void ToCourse(int course, string message)
        {
            Random random = new Random();
            List<long> userIds = new List<long>();
            MessagesSendParams messagesSendParams;
            int count = 0;
            lock (Glob.locker)
            {
                int usersCount = Glob.users.Count;
                for (int i = 0; i < usersCount; ++i)
                {
                    if (Glob.schedule_mapping.ContainsKey(Glob.users.ElementAt(i).Value))
                    {
                        if (Glob.schedule_mapping[Glob.users.ElementAt(i).Value].Course == course)
                        {
                            userIds.Add((long)Glob.users.ElementAt(i).Key);
                            ++count;
                            if (count == 100)
                            {
                                messagesSendParams = new MessagesSendParams()
                                {
                                    UserIds = userIds,
                                    Message = message,
                                    RandomId = random.Next()
                                };
                                count = 0;
                                Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                                userIds.Clear();
                            }
                        }
                    }
                }
                if (count > 0)
                {
                    messagesSendParams = new MessagesSendParams()
                    {
                        UserIds = userIds,
                        Message = message,
                        RandomId = random.Next()
                    };
                    Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                    userIds.Clear(); // возможно убрать
                }
            }
        }     
        public static void ToAll(string message)
        {
            Random random = new Random();
            List<long> userIds = new List<long>();
            MessagesSendParams messagesSendParams;
            int count = 0;
            lock (Glob.locker)
            {
                int usersCount = Glob.users.Count;
                for (int i = 0; i < usersCount; ++i)
                {
                    userIds.Add((long)Glob.users.ElementAt(i).Key);
                    ++count;
                    if (count == 100)
                    {
                        messagesSendParams = new MessagesSendParams()
                        {
                            UserIds = userIds,
                            Message = message,
                            RandomId = random.Next()
                        };
                        count = 0;
                        Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                        userIds.Clear();
                    }
                }
            }
            if (count > 0)
            {
                messagesSendParams = new MessagesSendParams()
                {
                    UserIds = userIds,
                    Message = message,
                    RandomId = random.Next()
                };
                Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                userIds.Clear();
            }
        }
        public static void ScheduleUpdate(int course, int index)
        {
            Random random = new Random();
            List<long> userIds = new List<long>();
            MessagesSendParams messagesSendParams;
            int count = 0;
            string group;
            string subgroup;
            string message;
            lock (Glob.locker)
            {
                group = Glob.schedule[course, index, 0];
                subgroup = Glob.schedule[course, index, 1];
                message = "Новое расписание для " + group + " (" + subgroup + ") " + Glob.data[course];
                int usersCount = Glob.users.Count;
                for (int i = 0; i < usersCount; ++i)
                {
                    if (Glob.users.ElementAt(i).Value.Group == group && Glob.users.ElementAt(i).Value.Subgroup == subgroup)
                    {
                        userIds.Add((long)Glob.users.ElementAt(i).Key);
                        ++count;
                        if (count == 100)
                        {
                            messagesSendParams = new MessagesSendParams()
                            {
                                UserIds = userIds,
                                Message = message,
                                RandomId = random.Next(),
                                Attachments = new List<MediaAttachment>() {
                                    new Photo()
                                    {
                                        AlbumId = Const.mainAlbumId,
                                        OwnerId = -178155012,
                                        Id = (long?)Glob.schedule_uploaded[course, index]
                                    }
                                }
                            };
                            count = 0;
                            Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                            userIds.Clear();
                        }
                    }
                }
                if (count > 0)
                {
                    messagesSendParams = new MessagesSendParams()
                    {
                        UserIds = userIds,
                        Message = message,
                        RandomId = random.Next(),
                        Attachments = new List<MediaAttachment>() {
                            new Photo()
                            {
                                AlbumId = Const.mainAlbumId,
                                OwnerId = -178155012,
                                Id = (long?)Glob.schedule_uploaded[course, index]
                            }
                        }
                    };
                    Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                    userIds.Clear();
                }
            }
        }
    }
}
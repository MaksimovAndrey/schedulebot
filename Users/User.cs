using System;
using System.Text;

namespace Schedulebot.Users
{
    public class User
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public string Group { get; set; }
        public int Subgroup { get; set; }
        public long LastMessageId { get; set; }
        public DateTime LastMessageTime { get; set; }

        public User(long id, string group, int subgroup, long lastMessageId = 0, DateTime lastMessageTime = default, bool isActive = true)
        {
            Id = id;
            IsActive = isActive;
            Group = group;
            Subgroup = subgroup;
            LastMessageId = lastMessageId;
            LastMessageTime = lastMessageTime;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Id);
            stringBuilder.Append(':');
            stringBuilder.Append(Group);
            stringBuilder.Append(':');
            stringBuilder.Append(Subgroup);
            stringBuilder.Append(':');
            stringBuilder.Append(LastMessageId);
            stringBuilder.Append(':');
            stringBuilder.Append(LastMessageTime.ToString());
            stringBuilder.Append(':');
            stringBuilder.Append(IsActive);
            return stringBuilder.ToString();
        }

        public static bool TryParse(string rawUserLine, out User user)
        {
            try
            {
                int index = rawUserLine.IndexOf(':');
                long id = long.Parse(rawUserLine.Substring(0, index));
                rawUserLine = rawUserLine.Substring(index + 1);
                index = rawUserLine.IndexOf(':');
                string group = rawUserLine.Substring(0, index);
                rawUserLine = rawUserLine.Substring(index + 1);
                index = rawUserLine.IndexOf(':');
                int subgroup = int.Parse(rawUserLine.Substring(0, index));
                rawUserLine = rawUserLine.Substring(index + 1);
                index = rawUserLine.IndexOf(':');
                long lastMessageId = long.Parse(rawUserLine.Substring(0, index));
                rawUserLine = rawUserLine.Substring(index + 1);
                index = rawUserLine.LastIndexOf(':');
                DateTime lastMessageTime = DateTime.Parse(rawUserLine.Substring(0, index));
                rawUserLine = rawUserLine.Substring(index + 1);
                bool isActive = bool.Parse(rawUserLine);

                user = new User(id, group, subgroup, lastMessageId, lastMessageTime, isActive);
                return true;
            }
            catch
            {
                Console.WriteLine("\nЕсли ты это видишь, то ты, наверное, из дурки сбежал.\nСтруктура такая: long id \" \" string group \" \" int subgroup \"\\n\"");
                user = null;
                return false;
            }
        }
    }
}

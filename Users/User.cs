using System;
using System.Text;

namespace Schedulebot.Users
{
    public class User
    {
        public long Id { get; set; }
        public string Group { get; set; }
        public int Subgroup { get; set; }
        public int MessageId { get; set; }

        public User(long id, string group, int subgroup, int messageId = 0)
        {
            Id = id;
            Group = group;
            Subgroup = subgroup;
            MessageId = messageId;
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
            stringBuilder.Append(MessageId);
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
                int messageId = int.Parse(rawUserLine);

                user = new User(id, group, subgroup, messageId);
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
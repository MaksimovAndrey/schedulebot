using System;
using System.Text;

namespace Schedulebot.Users
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
                Console.WriteLine("\nЕсли ты это видишь, то ты, наверное, из дурки сбежал.\nСтруктура такая: long id \" \" string group \" \" int subgroup \"\\n\"");
                user = null;
                return false;
            }
        }
    }
}
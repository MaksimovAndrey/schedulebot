using Schedulebot.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Schedulebot.Departments
{
    public abstract class Department : IDepartment
    {
        public static List<Dictionary<string, long>> LoadGroupsDictionaries(string filename, int coursesCount = 4)
        {
            List<Dictionary<string, long>> dictionaries = new List<Dictionary<string, long>>();
            for (int i = 0; i < coursesCount; i++)
                dictionaries.Add(new Dictionary<string, long>());

            using StreamReader file = new StreamReader(filename, Encoding.Default);
            string str, group;
            int course;
            while ((str = file.ReadLine()) != null)
            {
                course = int.Parse(str.Substring(0, str.IndexOf(':'))) - 1;
                str = str.Substring(str.IndexOf(':') + 1);
                group = str.Substring(0, str.IndexOf(':'));
                str = str.Substring(str.IndexOf(':') + 1);
                if (long.TryParse(str, out long result))
                    dictionaries[course].Add(group, result);
                else
                    throw new System.IO.IOException("Uncorrect GROUPnLONG dictionary LONG value");
            }
            return dictionaries;
        }

        public abstract void SaveUsers();
    }
}

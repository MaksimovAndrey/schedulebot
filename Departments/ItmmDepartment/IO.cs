using Schedulebot.Mapping.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : IDepartment
    {
        private List<Dictionary<string, long>> LoadGroupsDictionaries(string filename, int coursesCount = 4)
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

        private void LoadSettings(string path)
        {
            using StreamReader file = new StreamReader(path, Encoding.Default);
            string str, value;
            while ((str = file.ReadLine()) != null)
            {
                if (str.Contains(':'))
                {
                    value = str.Substring(str.IndexOf(':') + 1);
                    str = str.Substring(0, str.IndexOf(':'));
                    switch (str)
                    {
                        case "startDay":
                        {
                            startDay = int.Parse(value);
                            break;
                        }
                        case "startWeek":
                        {
                            startWeek = int.Parse(value);
                            break;
                        }
                    }
                }
            }
        }

        //private void LoadUploadedSchedule(string path)
        //{
        //    using StreamReader file = new StreamReader(path, Encoding.Default);

        //    string rawLine;
        //    while (!file.EndOfStream)
        //    {
        //        rawLine = file.ReadLine();
        //        if (string.IsNullOrEmpty(rawLine))
        //            break;

        //        var rawSpan = rawLine.AsSpan();

        //        int spaceIndex = rawSpan.IndexOf(' ');
        //        int lastSpaceIndex = rawSpan.LastIndexOf(' ');

        //        long id = long.Parse(rawSpan.Slice(0, spaceIndex));
        //        string group = rawSpan.Slice(spaceIndex + 1, lastSpaceIndex - spaceIndex - 1).ToString();
        //        int subgroup = int.Parse(rawSpan.Slice(lastSpaceIndex + 1, 1));

        //        if (mapper.TryGetCourseAndGroupIndex(group, out UserMapping mapping))
        //            courses[mapping.Course].groups[mapping.GroupIndex].subgroups[subgroup - 1].PhotoId = id;
        //    }
        //}

        //private void SaveUploadedSchedule(string path)
        //{
        //    using StreamWriter file = new StreamWriter(path);

        //    StringBuilder stringBuilder = new StringBuilder();
        //    for (int currentCourse = 0; currentCourse < CoursesCount; currentCourse++)
        //    {
        //        int groupsCount = courses[currentCourse].groups.Count;
        //        for (int currentGroup = 0; currentGroup < groupsCount; currentGroup++)
        //        {
        //            stringBuilder.Append(courses[currentCourse].groups[currentGroup].subgroups[0].PhotoId);
        //            stringBuilder.Append(' ');
        //            stringBuilder.Append(courses[currentCourse].groups[currentGroup].name);
        //            stringBuilder.Append(" 1\n");
        //            stringBuilder.Append(courses[currentCourse].groups[currentGroup].subgroups[1].PhotoId);
        //            stringBuilder.Append(' ');
        //            stringBuilder.Append(courses[currentCourse].groups[currentGroup].name);
        //            stringBuilder.Append(" 2\n");
        //        }
        //    }
        //    if (stringBuilder.Length != 0)
        //        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        //    file.WriteLine(stringBuilder.ToString());
        //}
    }
}
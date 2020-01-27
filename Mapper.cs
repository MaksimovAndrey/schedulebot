using System.Collections.Generic;
using System;    

namespace Schedulebot
{
    public class Mapper
    {
        List<List<string>> coursesMap = new List<List<string>>();
        Dictionary<string, (int?, int)> groupsMap = new Dictionary<string, (int?, int)>();
        
        public void CreateMaps(Course[] courses)
        {
            int coursesAmount = courses.GetLength(0);
            for (int currentCourse = 0; currentCourse < coursesAmount; currentCourse++)
            {
                List<string> groupNames = new List<string>();
                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.Count; currentGroup++)
                {
                    groupNames.Add(courses[currentCourse].groups[currentGroup].name);
                    groupsMap.Add(courses[currentCourse].groups[currentGroup].name, (currentCourse, currentGroup));
                }
                coursesMap.Add(groupNames);
            }
        }

        public List<string> GetGroupNames(int course)
        {
            return coursesMap[course];
        }

        public (int?, int) GetCourseAndIndex(string group)
        {
            return groupsMap.GetValueOrDefault(group);
        }
    }
}
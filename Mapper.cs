using System.Collections.Generic;
using System;    

namespace Schedulebot
{
    public class Mapper
    {
        private List<List<string>> coursesMap = new List<List<string>>();
        private Dictionary<string, (int?, int)> groupsMap = new Dictionary<string, (int?, int)>();
        
        public void CreateMaps(Course[] courses)
        {
            coursesMap.Clear();
            groupsMap.Clear();
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

        public List<(string, int)> GetOldGroupSubgroupList(List<(string, int)> newGroupNames, List<int> updatingCourses)
        {
            List<(string, int)> groupSubgroupList = new List<(string, int)>();

            for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
                for (int currentGroupName = 0; currentGroupName < coursesMap[updatingCourses[currentUpdatingCourse]].Count; currentGroupName++)
                    for (int currentSubgroup = 1; currentSubgroup < 3; currentSubgroup++)
                        groupSubgroupList.Add((coursesMap[updatingCourses[currentUpdatingCourse]][currentGroupName], currentSubgroup));

            for (int currentNewGroupName = 0; currentNewGroupName < newGroupNames.Count; currentNewGroupName++)
                groupSubgroupList.Remove(newGroupNames[currentNewGroupName]);
            
            return groupSubgroupList;
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
using Schedulebot.Mapping.Utils;
using System.Collections.Generic;

namespace Schedulebot.Mapping
{
    public class Mapper
    {
        private readonly List<List<string>> coursesMap
            = new List<List<string>>();
        private readonly Dictionary<string, UserMapping> groupsMap
            = new Dictionary<string, UserMapping>();

        public Mapper(Course[] courses)
        {
            CreateMaps(courses);
        }

        public void CreateMaps(Course[] courses)
        {
            coursesMap.Clear();
            groupsMap.Clear();
            int coursesAmount = courses.GetLength(0);
            for (int currentCourse = 0; currentCourse < coursesAmount; currentCourse++)
            {
                if (courses[currentCourse].groups == null)
                {
                    coursesMap.Add(new List<string>());
                    continue;
                }
                List<string> groupNames = new List<string>();
                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.Count; currentGroup++)
                {
                    if (!groupsMap.ContainsKey(courses[currentCourse].groups[currentGroup].Name))
                    {
                        groupNames.Add(courses[currentCourse].groups[currentGroup].Name);
                        groupsMap.Add(courses[currentCourse].groups[currentGroup].Name,
                                      new UserMapping(currentCourse, currentGroup));
                    }
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

        public List<(string, int)> GetOldGroupSubgroupList(List<(string, int)> newGroupNames, int updatingCourse)
        {
            List<(string, int)> groupSubgroupList = new List<(string, int)>();

            for (int currentGroupName = 0; currentGroupName < coursesMap[updatingCourse].Count; currentGroupName++)
                for (int currentSubgroup = 1; currentSubgroup < 3; currentSubgroup++)
                    groupSubgroupList.Add((coursesMap[updatingCourse][currentGroupName], currentSubgroup));

            for (int currentNewGroupName = 0; currentNewGroupName < newGroupNames.Count; currentNewGroupName++)
                groupSubgroupList.Remove(newGroupNames[currentNewGroupName]);

            return groupSubgroupList;
        }

        public List<string> GetGroupNames(int course)
        {
            return coursesMap[course];
        }

        public bool TryGetCourseAndGroupIndex(string group, out UserMapping value)
            => groupsMap.TryGetValue(group, out value);
    }
}
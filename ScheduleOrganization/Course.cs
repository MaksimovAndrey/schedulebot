using System.Collections.Generic;

namespace Schedulebot
{
    public class Course
    {
        public List<Group> groups;

        public Course()
        {
            groups = new List<Group>();
        }
    }
}

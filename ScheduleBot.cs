using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Schedulebot.Departments;

namespace Schedulebot
{
    public class ScheduleBot
    {
#if DEBUG
        private const string с_path = @"C:/Main/Projects/Shared/sbinfo/";
#else
        private const string с_path = @"/media/projects/sbtest/";
#endif

        public static readonly HttpClient client = new HttpClient();
        public const int departmentsCount = 1;
        public IDepartment[] departments;

        public ScheduleBot(ref List<Task> _tasks)
        {
            //! Пока один департамент будет так
            // TODO: переписать с restart department
            List<Task> tasks = new List<Task>();

            departments = new IDepartment[departmentsCount];
            for (int currentDepartment = 0; currentDepartment < departmentsCount; currentDepartment++)
            {
                departments[currentDepartment] = new DepartmentItmm(с_path, ref tasks);
            }
            _tasks.Add(Task.WhenAny(tasks));
        }

        // TODO: restart department (при краше модуля(Task.WhenAny))
    }
}

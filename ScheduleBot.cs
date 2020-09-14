using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Schedulebot.Departments;

namespace Schedulebot
{
    public class ScheduleBot
    {
#if DEBUG
        public const string version = "v2.3 DEV";
#else
        public const string version = "v2.3";
#endif
        public const string delimiter = " · ";
        public const string lectureSign = "Л";
        public const string labSign = "Лаб";
        public const string seminarSign = "П";
        public const string remotelySign = "Д";
#if DEBUG
        private const string с_path = @"C:/Custom/Projects/Shared/sbtest/";
#else
        private const string с_path = @"/media/projects/sbtest/";
#endif

        public static readonly HttpClient client = new HttpClient();
        private const int departmentsAmount = 1;
        public IDepartment[] departments;

        public ScheduleBot(ref List<Task> _tasks)
        {
            //! Пока один департамент будет так
            // TODO: переписать с restart department
            List<Task> tasks = new List<Task>();

            departments = new IDepartment[departmentsAmount];
            for (int currentDepartment = 0; currentDepartment < departmentsAmount; currentDepartment++)
            {
                departments[currentDepartment] = new DepartmentItmm(с_path, ref tasks);
            }
            _tasks.Add(Task.WhenAny(tasks));
        }

        // TODO: restart department (при краше модуля(Task.WhenAny))
    }
}
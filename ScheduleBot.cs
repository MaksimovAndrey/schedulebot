using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

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
        public const string remotelySign = "Д";
        #if DEBUG 
            private const string path = @"C:/Custom/Projects/Shared/sbtest/";
        #else
            private const string path = @"/media/projects/sbtest/";
        #endif

        public static readonly HttpClient client = new HttpClient();
        private const int departmentsAmount = 1;
        public IDepartment[] departments;

        public ScheduleBot(ref List<Task> _tasks)
        {
            //! Пока один департамент будет так
            // todo: переписать с restart department
            List<Task> tasks = new List<Task>();

            departments = new IDepartment[departmentsAmount];
            for (int currentDepartment = 0; currentDepartment < departmentsAmount; currentDepartment++)
            {
                departments[currentDepartment] = new DepartmentItmm(path, ref tasks);
            }            
            _tasks.Add(Task.WhenAny(tasks));
        }

        // todo: restart department (при краше модуля(Task.WhenAny))
    }
}
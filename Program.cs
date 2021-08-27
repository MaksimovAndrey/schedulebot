using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schedulebot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = Constants.name + ' ' + Constants.version;
            List<Task> tasks = new List<Task>();
            ScheduleBot scheduleBot = new ScheduleBot(ref tasks);

            var mainTask = await Task.WhenAny(tasks);
            for (int curDepartment = 0; curDepartment < ScheduleBot.departmentsCount; curDepartment++)
            {
                scheduleBot.departments[curDepartment].SaveUsers();
            }
        }
    }
}
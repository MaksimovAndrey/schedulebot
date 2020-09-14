using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schedulebot
{
    class Program
    {
        
        static async Task Main(string[] args)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            Console.Title = "schedulebot v2.3";
            List<Task> tasks = new List<Task>();
            ScheduleBot scheduleBot = new ScheduleBot(ref tasks);

            var mainTask = await Task.WhenAny(tasks);
        }
    }
}
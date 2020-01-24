using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;
using System.IO;
// using VkNet.Model.RequestParams;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using GemBox.Spreadsheet;
using System.Net.Http;

namespace Schedulebot
{
    public static class Const
    {
        public const string version = "v1.0";
    }
    public static class Glob
    {

        //! ТОЛЬКО ДЛЯ ITMM
        // todo: подумать как пофиксить можно BibleThump
        public static Dictionary<string, string> acronymToPhrase = new Dictionary<string, string>();
        public static Dictionary<string, string> doubleOptionallySubject = new Dictionary<string, string>();
        public static List<string> fullName;
    }
    public class ScheduleBot
    {
        public static readonly HttpClient client = new HttpClient();
        private const string path = @"C:\Custom\Projects\Shared\sbtest\";
        private int departmentsAmount = 1;
        IDepartment[] departments;

        public ScheduleBot()
        {
            departments = new[] { new ItmmDepartment(path) };
        }

        // провреряем актуальность расписания для всех факультетов
        public void CheckRelevance()
        {
            Action action = () =>
            {
                while (true)
                {
                    // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Проверка актуальности");
                    for (int i = 0; i < departmentsAmount; ++i)
                        // new Task(departments[i].Action).Start();
                    // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Проверка актуальности");
                    Thread.Sleep(600000);
                }
            };
            Task task = new Task(action);
            task.Start();
        }
        
        private void UpdateSchedule(int departmentIndex, List<int> coursesToUpdate)
        {

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            ScheduleBot scheduleBot = new ScheduleBot();



            Console.WriteLine("Hello World!");
            
            
            
            // Parsing.Mapper(@"C:\Custom\Projects\Shared\schedulebot\downloads\1_course_schedule.xls");
        }
    }
}
// todo: создать карту людей и расписания

/*
check : https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md
        https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1
        https://habr.com/ru/post/137680/
        https://codernet.ru/books/c_sharp/yazyk_programmirovaniya_c_7_i_platformy_net_i_net_core/
file.Close() check

*/

/* todo
    unit-тесты Обязательно для ParseXLS
    Design a benchmark: https://benchmarkdotnet.org/articles/overview.html

*/
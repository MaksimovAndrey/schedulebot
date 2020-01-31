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
    public class ScheduleBot
    {
        public static readonly HttpClient client = new HttpClient();
        private const string path = @"C:\Custom\Projects\Shared\sbtest\";
        private int departmentsAmount = 1;
        public IDepartment[] departments;

        public ScheduleBot()
        {
            departments = new[] { new ItmmDepartment(path) };
        }

        public async Task CheckRelevanceAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    for (int i = 0; i < departmentsAmount; ++i)
                        departments[i].CheckRelevanceAsync();
                    await Task.Delay(600000);
                }
            });
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            ScheduleBot scheduleBot = new ScheduleBot();


            Console.WriteLine("1");
            var test0 = scheduleBot.departments[0].ExecuteMethodsAsync();
            Console.WriteLine("2");
            var test = scheduleBot.departments[0].GetMessagesAsync();
            Console.WriteLine("3");
            var test1 = scheduleBot.departments[0].UploadPhotosAsync();
            Console.WriteLine("4");
            var checkRelevanceTask = scheduleBot.CheckRelevanceAsync(); // active
            Console.WriteLine("5");

            while (true)
            {
                Console.Read();
                Console.WriteLine("000000000000000000000000000");
            }
            
            
            
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
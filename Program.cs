using System;
using GemBox.Spreadsheet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schedulebot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            ScheduleBot scheduleBot = new ScheduleBot();

            // Test.Test.Schedule(0, new int[4,101,101], false); // тест алгоритма parse

            List<Task> tasks = new List<Task>();
            tasks.Add(scheduleBot.departments[0].ExecuteMethodsAsync());
            tasks.Add(scheduleBot.departments[0].GetMessagesAsync());
            tasks.Add(scheduleBot.departments[0].UploadPhotosAsync());
            tasks.Add(scheduleBot.CheckRelevanceAsync());
            tasks.Add(scheduleBot.departments[0].SaveUsersAsync());

            await Task.WhenAny(tasks);
        }
    }
}

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
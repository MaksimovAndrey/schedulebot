using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net;
using System.IO;
// using VkNet.Model.RequestParams;
using Newtonsoft.Json;
using System.Threading;

namespace schedulebot
{
    public class Const
    {
        public const string version = "v1.0";
    }
    public static class Glob
    {
        //! ТОЛЬКО ДЛЯ ITMM
        // todo: подумать как пофиксить можно BibleThump
        public static Dictionary<string, string> acronymToPhrase = new Dictionary<string, string>();
        public static Dictionary<string, string> doubleOptionallySubject = new Dictionary<string, string>();
        public static string[] fullName;
    }
    public class User : IEquatable<User>
    {
        public string Group = null;
        public string Subgroup = null;

        public User() { }
        public User(string g, string s) { Group = g; Subgroup = s; }

        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            return other != null && Group == other.Group && Subgroup == other.Subgroup;
        }

        public override int GetHashCode()
        {
            var hashCode = 390074312;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Group);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Subgroup);
            return hashCode;
        }
    }
    public static class ScheduleBot
    {
        public static void DepartmentsStart()
        {
            departments[0] = new DepartmentITMM(path);
        }
        private const string path = @"C:\Custom\Projects\Shared\sbtest\";
        private static int departmentsCount = 1;
        static Department[] departments = { new DepartmentITMM(path) };
        // провреряем актуальность расписания для всех факультетов
        public static void CheckRelevance()
        {
            while (true)
            {
                Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Проверка актуальности");
                for (int i = 0; i < departmentsCount; ++i)
                {
                    // remake with tasks
                    List<int> coursesToUpdate = departments[i].CheckRelevance();
                    if (coursesToUpdate != null)
                    {
                        departments[i].UpdateSchedule(coursesToUpdate);
                    }
                    // todo: продолжение
                }
                Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Проверка актуальности");
                Thread.Sleep(600000);
            }
        }
        private static void UpdateSchedule(int departmentIndex, List<int> coursesToUpdate)
        {

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            ScheduleBot.DepartmentsStart();
            Console.WriteLine("Hello World!");
            Parsing.Mapper(@"C:\Custom\Projects\Shared\schedulebot\downloads\1_course_schedule.xls");
        }
    }
}


/*
check : https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1
        https://habr.com/ru/post/137680/
        https://codernet.ru/books/c_sharp/yazyk_programmirovaniya_c_7_i_platformy_net_i_net_core/
file.Close() check

List .Any( lec => lec.IsEmpty ) ;


todo: юнит тесты (xls обязательно)
*/
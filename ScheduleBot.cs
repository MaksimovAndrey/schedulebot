using System.Threading.Tasks;
using System.Net.Http;

namespace Schedulebot
{
    public class ScheduleBot
    {
        public const string version = "v2.2";
        public const string delimiter = " · ";
        public static readonly HttpClient client = new HttpClient();
        private const string path = @"C:/Custom/Projects/Shared/sbtest/";
        // private const string path = @"/media/projects/sbtest/";
        private int departmentsAmount = 1;
        public IDepartment[] departments;

        public ScheduleBot()
        {
            departments = new[] { new ItmmDepartment(path) };
        }

        public Task CheckRelevanceAsync()
        {
            return Task.Run(async () =>
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
}
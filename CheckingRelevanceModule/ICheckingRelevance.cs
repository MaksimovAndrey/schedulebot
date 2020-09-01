using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schedulebot.Schedule.Relevance
{
    public interface ICheckingRelevance
    {
        DatesAndUrls DatesAndUrls { get; }
        Task<(string, List<int>)> CheckRelevanceAsync();
    }
}
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schedulebot.Schedule.Relevance
{
    public interface IRelevance
    {
        DatesAndUrls DatesAndUrls { get; }

        public const string defaultFilenameBody = "course.xlsx";

        Task<HtmlDocument> DownloadHtmlDocument(string websiteUrl);
        Task<bool> DownloadScheduleFiles(int course, List<int> fileIndexes);

        string ParseInformation(HtmlDocument htmlDocument);

        // Возвращает список курс + индекс файлов, которые надо скачать
        List<(int, List<int>)> UpdateDatesAndUrls(HtmlDocument htmlDocument);
    }
}
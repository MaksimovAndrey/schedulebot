using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Schedulebot.Schedule.Relevance
{
    public interface IRelevance
    {
        public const string defaultFilenameBody = "course.xlsx";

        Task<HtmlDocument> DownloadHtmlDocument(string websiteUrl);

        string ParseInformation(HtmlDocument htmlDocument);
    }
}

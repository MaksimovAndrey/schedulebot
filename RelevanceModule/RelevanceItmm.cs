using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schedulebot.Schedule.Relevance
{
    public class RelevanceItmm : IRelevance
    {
        private string Path { get; }

        private const int с_tryDownloadDelay = 60000;

        public RelevanceItmm(string path)
        {
            Path = path;
        }

        public async Task<HtmlDocument> DownloadHtmlDocument(string websiteUrl)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            try
            {
                return await htmlWeb.LoadFromWebAsync(websiteUrl);
            }
            catch
            {
                //! ошибка загрузки страницы
                return null;
            }
        }

        public string ParseInformation(HtmlDocument htmlDocument)
        {
            try
            {
                //htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"[\u00A0\u00AD\s]+", "");
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"\u00ad", "");
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"&nbsp;", " ");
                HtmlNodeCollection info = htmlDocument.DocumentNode.SelectNodes("//main");
                string text = info[0].InnerText;
                int startIndex = text.IndexOf("Важная информация") + 17;
                int endIndex = text.IndexOf("Об Институте");
                if (startIndex != -1 && endIndex != -1)
                    return text.Substring(startIndex, endIndex - startIndex).Trim();
                else
                    return "Не удалось найти информацию на сайте";
            }
            catch
            {
                return "Произошла ошибка при поиске информации на сайте";
            }
        }
    }
}

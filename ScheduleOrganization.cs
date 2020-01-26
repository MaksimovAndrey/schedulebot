using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Schedulebot
{
    public class CheckRelevanceStuffITMM : ICheckRelevanceStuff
    {
        private const string url = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
        public async Task<DatesAndUrls> CheckRelevance()
        {
            HtmlDocument htmlDocument;
            HtmlWeb htmlWeb = new HtmlWeb();
            try
            {
                htmlDocument = await htmlWeb.LoadFromWebAsync(url);
            }
            catch 
            {
                //! ошибка загрузки страницы
                return null;
            }
            if (htmlDocument != null)
                return await Parse(htmlDocument);
            return null;
        }
        private async Task<DatesAndUrls> Parse(HtmlDocument htmlDocument)
        {
            await Task.Run(() =>
            {
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"\u00ad|", "");
                HtmlNodeCollection nodesWithDates = htmlDocument.DocumentNode.SelectNodes("//p[contains(text(), 'Расписание бакалавров')]");
                HtmlNodeCollection nodesWithUrls = htmlDocument.DocumentNode.SelectNodes("//a[contains(text(), 'скачать')]");
                if (nodesWithDates == null || nodesWithUrls == null)
                {
                    // todo: throw error
                    return null;
                }
                else if (nodesWithDates.Count > 0 && nodesWithUrls.Count > 0 && nodesWithUrls.Count == nodesWithUrls.Count)
                {
                    DatesAndUrls parseResult = new DatesAndUrls() { count = nodesWithDates.Count };
                    for (int i = 0; i < nodesWithDates.Count; ++i)
                    {
                        string nodeText = nodesWithDates[i].InnerText;
                        if (nodeText.Contains("(от "))
                        {
                            parseResult.dates[i] = nodeText.Substring(nodeText.LastIndexOf("(от") + 1, nodeText.LastIndexOf(')') - (nodeText.LastIndexOf("(от") + 1));
                        }
                    }
                    for (int i = 0; i < nodesWithUrls.Count; ++i)
                    {
                        parseResult.urls[i] = nodesWithUrls[i].Attributes["href"].Value.Trim();
                        if (parseResult.urls[i] == "")
                        {
                            parseResult.dates[i] = "";
                        }
                    }
                    return parseResult;
                }
                return null;
            });
            return null;
        }
    }
    
    public class DatesAndUrls
    {
        public int count;
        public string[] urls = new string[5];
        public string[] dates = new string[5];
    }

    public interface ICheckRelevanceStuff
    {
        Task<DatesAndUrls> CheckRelevance();
    }
}
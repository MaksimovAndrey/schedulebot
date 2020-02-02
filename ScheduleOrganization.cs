using System;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Schedulebot
{
    public class CheckRelevanceStuffITMM : ICheckRelevanceStuff
    {
        private const string url = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
        public async Task<DatesAndUrls> CheckRelevanceAsync()
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
            {
                return await ParseAsync(htmlDocument);
            }
            return null;
        }
        private async Task<DatesAndUrls> ParseAsync(HtmlDocument htmlDocument)
        {
            return await Task.Run(() =>
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
                    DatesAndUrls parseResult = new DatesAndUrls();
                    int count = 0;
                    for (int i = 0; i < nodesWithDates.Count; ++i)
                    {
                        string nodeText = nodesWithDates[i].InnerText;
                        if (nodeText.Contains("(от ") && nodeText.IndexOf(" курс") > 0)
                        {
                            if (Int32.TryParse(nodeText.Substring(nodeText.IndexOf(" курс") - 1, 1), out int course))
                            {
                                if (nodesWithUrls[i].Attributes["href"].Value.Trim() != "")
                                {
                                    parseResult.dates[count] = nodeText.Substring(nodeText.LastIndexOf("(от") + 1, nodeText.LastIndexOf(')') - (nodeText.LastIndexOf("(от") + 1));
                                    parseResult.courses[count] = course - 1;
                                    parseResult.urls[count] = nodesWithUrls[i].Attributes["href"].Value.Trim();
                                    count++;
                                }
                            }
                        }
                    }
                    parseResult.count = count;
                    return parseResult;
                }
                return null;
            });
        }
    }
    
    public class DatesAndUrls
    {
        public int count;
        public string[] urls = new string[5];
        public string[] dates = new string[5];
        public int[] courses = new int[5];
    }

    public interface ICheckRelevanceStuff
    {
        Task<DatesAndUrls> CheckRelevanceAsync();
    }
}
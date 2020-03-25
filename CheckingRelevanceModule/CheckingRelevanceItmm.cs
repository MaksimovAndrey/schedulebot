using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Schedulebot.Schedule.Relevance
{
    public class CheckingRelevanceItmm : ICheckingRelevance
    {
        private const string url = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
        
        public DatesAndUrls DatesAndUrls { get; }

        public CheckingRelevanceItmm(string path)
        {
            DatesAndUrls = new DatesAndUrls(path);
        }

        public async Task<List<int>> CheckRelevanceAsync()
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
                return Parse(htmlDocument);
            }
            return null;
        }

        private List<int> Parse(HtmlDocument htmlDocument)
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
                List<int> parseResult = new List<int>();
                for (int i = 0; i < nodesWithDates.Count; ++i)
                {
                    string nodeText = nodesWithDates[i].InnerText;
                    if (nodeText.Contains("(от ") && nodeText.IndexOf(" курс") > 0)
                    {
                        if (Int32.TryParse(nodeText.Substring(nodeText.IndexOf(" курс") - 1, 1), out int course))
                        {
                            course--;
                            if (nodesWithUrls[i].Attributes["href"].Value.Trim() != "")
                            {
                                string date = nodeText.Substring(nodeText.LastIndexOf("(от") + 1, nodeText.LastIndexOf(')') - (nodeText.LastIndexOf("(от") + 1));
                                if (DatesAndUrls.dates[course] != date)
                                {
                                    DatesAndUrls.dates[course] = date;
                                    DatesAndUrls.urls[course] = nodesWithUrls[i].Attributes["href"].Value.Trim();
                                    parseResult.Add(course);
                                }
                            }
                        }
                    }
                }
                return parseResult;
            }
            return null;
        }
    }
}
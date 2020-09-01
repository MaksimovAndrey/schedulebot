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

        public async Task<(string, List<int>)> CheckRelevanceAsync()
        {
            HtmlDocument htmlDocument = await DownloadHtmlDocument();
            string time = DateTime.Now.ToString();
            if (htmlDocument == null)
                return (null, null);

            return (ParseImportantInformation(htmlDocument, time), Parse(htmlDocument));
        }

        private async Task<HtmlDocument> DownloadHtmlDocument()
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            try
            {
                return await htmlWeb.LoadFromWebAsync(url);
            }
            catch 
            {
                //! ошибка загрузки страницы
                return null;
            }
        }

        private string ParseImportantInformation(HtmlDocument htmlDocument, string time)
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
                    return "От " + time + "\n" + text.Substring(startIndex, endIndex - startIndex).Trim();
                else
                    return "От " + time + "\nНе удалось найти информацию";
            }
            catch
            {
                return null;
            }
        }

        private List<int> Parse(HtmlDocument htmlDocument)
        {
            try
            {
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"\u00ad|", "");
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"&nbsp;", " ");

                HtmlNodeCollection nodesWithString = htmlDocument.DocumentNode.SelectNodes("//strong[contains(text(), 'Расписание бакалавров')]");
                HtmlNodeCollection nodesWithDate = htmlDocument.DocumentNode.SelectNodes("//strong[contains(text(), 'Расписание бакалавров')]/span");
                HtmlNodeCollection nodesWithUrl = htmlDocument.DocumentNode.SelectNodes("//strong[contains(text(), 'Расписание бакалавров')]/a");
                if (nodesWithDate == null || nodesWithUrl == null)
                {
                    // todo: throw error
                    return null;
                }
                else if (nodesWithDate.Count > 0 && nodesWithUrl.Count > 0 && nodesWithString.Count > 0 && nodesWithUrl.Count == nodesWithUrl.Count && nodesWithUrl.Count == nodesWithString.Count)
                {
                    List<int> parseResult = new List<int>();
                    for (int i = 0; i < nodesWithDate.Count; ++i)
                    {
                        string nodeText = nodesWithString[i].InnerText;
                        if (nodeText.Contains("(от ") && nodeText.IndexOf(" курс") > 0)
                        {
                            if (Int32.TryParse(nodeText.Substring(nodeText.IndexOf(" курс") - 1, 1), out int course))
                            {
                                course--;
                                if (nodesWithUrl[i].Attributes["href"].Value.Trim() != "")
                                {
                                    string date = nodeText.Substring(nodeText.LastIndexOf("(от") + 1, nodeText.LastIndexOf(')') - (nodeText.LastIndexOf("(от") + 1));
                                    if (DatesAndUrls.dates[course] != date)
                                    {
                                        DatesAndUrls.dates[course] = date;
                                        DatesAndUrls.urls[course] = nodesWithUrl[i].Attributes["href"].Value.Trim();
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
            catch
            {
                return null;
            }
        }
    }
}
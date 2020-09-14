using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Schedulebot.Schedule.Relevance
{
    public class RelevanceItmm : IRelevance
    {
        public DatesAndUrls DatesAndUrls { get; }

        private string Path { get; }


        public string DownloadFolderPath { get; }

        private const int с_tryDownloadDelay = 60000;

        public RelevanceItmm(string path, string downloadFolderPath)
        {
            Path = path;
            DownloadFolderPath = downloadFolderPath;
            DatesAndUrls = new DatesAndUrls(Path);
        }

        public List<(int, List<int>)> UpdateDatesAndUrls(HtmlDocument htmlDocument)
        {
            try
            {
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"\u00ad|", "");
                htmlDocument.DocumentNode.InnerHtml = Regex.Replace(htmlDocument.DocumentNode.InnerHtml, @"&nbsp;", " ");

                HtmlNodeCollection nodes = htmlDocument.DocumentNode.SelectNodes("//main/div/div/div");
                HtmlNode node = null;
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (!string.IsNullOrEmpty(nodes[i].InnerText.Trim()))
                    {
                        node = nodes[i];
                        break;
                    }
                }

                if (node == null)
                    return null;

                var matсhes = Regex.Matches(node.InnerHtml, @"[а-яА-Я]+ [а-яА-Я]+ \d [а-яА-Я]+");
                if (matсhes.Count == 0)
                    return null;

                string nodeInnerHtml = node.InnerHtml;


                List<(int, List<int>)> toUpdate = new List<(int, List<int>)>();

                for (int currentMatchResult = 0; currentMatchResult < matсhes.Count; currentMatchResult++)
                {
                    try
                    {
                        string dateAndUrlsHere;
                        if (currentMatchResult != matсhes.Count - 1)
                        {
                            int startIndex = nodeInnerHtml.IndexOf(matсhes[currentMatchResult].Value) + matсhes[currentMatchResult].Value.Length;
                            int endIndex = nodeInnerHtml.IndexOf(matсhes[currentMatchResult + 1].Value);
                            dateAndUrlsHere = nodeInnerHtml.Substring(startIndex, endIndex - startIndex);

                            nodeInnerHtml.Substring(endIndex);
                        }
                        else
                        {
                            int startIndex = nodeInnerHtml.IndexOf(matсhes[currentMatchResult].Value) + matсhes[currentMatchResult].Value.Length;
                            dateAndUrlsHere = nodeInnerHtml.Substring(startIndex);
                        }

                        int startDateIndex = dateAndUrlsHere.IndexOf('>', dateAndUrlsHere.IndexOf("<span") + 5) + 1;
                        int endDateIndex = dateAndUrlsHere.IndexOf("</span>", startDateIndex);

                        string date = dateAndUrlsHere.Substring(startDateIndex, endDateIndex - startDateIndex);
                        int indexOfOt = date.ToUpper().IndexOf("ОТ");
                        if (indexOfOt != -1)
                            date = date.Substring(indexOfOt + 2);
                        date = date.Trim();

                        int realCourse;
                        if (int.TryParse(Regex.Match(matсhes[currentMatchResult].Value, "\\d").Value, out realCourse))
                        {
                            realCourse--;
                        }
                        else
                        {
                            continue;
                        }

                        string urlsHere = dateAndUrlsHere.Substring(endDateIndex + 5);
                        var urlsMatchCollection = Regex.Matches(urlsHere, "href=\\\"[a-zA-Z0-9]{4,5}://[a-zA-Z0-9./_-]+\\\"");
                        List<string> urls = new List<string>();
                        for (int currentMatchedUrl = 0; currentMatchedUrl < urlsMatchCollection.Count; currentMatchedUrl++)
                            urls.Add(urlsMatchCollection[currentMatchedUrl].Value.Substring(6, urlsMatchCollection[currentMatchedUrl].Value.Length - 7));

                        List<int> fileIndexesToUpdate = new List<int>();

                        while (DatesAndUrls.urls[realCourse].Count > urls.Count)
                            DatesAndUrls.urls[realCourse].RemoveAt(DatesAndUrls.urls[realCourse].Count - 1);

                        if (date != DatesAndUrls.dates[realCourse])
                        {
                            for (int currentFoundUrlIndex = 0; currentFoundUrlIndex < urls.Count; currentFoundUrlIndex++)
                            {
                                fileIndexesToUpdate.Add(currentFoundUrlIndex);
                                if (DatesAndUrls.urls[realCourse].Count - 1 < currentFoundUrlIndex)
                                    DatesAndUrls.urls[realCourse].Add(urls[currentFoundUrlIndex]);
                                else
                                    DatesAndUrls.urls[realCourse][currentFoundUrlIndex] = urls[currentFoundUrlIndex];
                                DatesAndUrls.urls[realCourse][currentFoundUrlIndex] = urls[currentFoundUrlIndex];
                            }
                            DatesAndUrls.dates[realCourse] = date;
                        }
                        else
                        {
                            // Дата сверху не поменялась, но ссылки новые
                            for (int currentFoundUrlIndex = 0; currentFoundUrlIndex < urls.Count; currentFoundUrlIndex++)
                            {
                                if (DatesAndUrls.urls[realCourse].Count - 1 < currentFoundUrlIndex)
                                {
                                    fileIndexesToUpdate.Add(currentFoundUrlIndex);
                                    DatesAndUrls.urls[realCourse].Add(urls[currentFoundUrlIndex]);
                                }
                                else
                                {
                                    if (urls[currentFoundUrlIndex] != DatesAndUrls.urls[realCourse][currentFoundUrlIndex])
                                    {
                                        fileIndexesToUpdate.Add(currentFoundUrlIndex);
                                        DatesAndUrls.urls[realCourse][currentFoundUrlIndex] = urls[currentFoundUrlIndex];
                                    }
                                }
                            }
                        }
                        if (fileIndexesToUpdate.Count != 0)
                            toUpdate.Add((realCourse, fileIndexesToUpdate));
                    }
                    catch
                    {
                        continue;
                    }
                }
                return toUpdate;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DownloadScheduleFiles(int course, List<int> fileIndexes)
        {
            int triesAmount = 0;
            List<bool> successDownload = new List<bool>();

            for (int currentFileIndex = 0; currentFileIndex < fileIndexes.Count; currentFileIndex++)
            {
                if (triesAmount == 5)
                {
                    triesAmount = 0;
                    successDownload.Add(false);
                    continue;
                }
                HttpResponseMessage response = await ScheduleBot.client.GetAsync(DatesAndUrls.urls[course][fileIndexes[currentFileIndex]]);
                if (response.IsSuccessStatusCode)
                {
                    string filePath = DownloadFolderPath + fileIndexes[currentFileIndex].ToString() + '_' + course.ToString() + IRelevance.defaultFilenameBody;
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                        await response.Content.CopyToAsync(fileStream);
                    successDownload.Add(true);
                    continue;
                }
                else
                {
                    ++triesAmount;
                    --currentFileIndex;
                    await Task.Delay(с_tryDownloadDelay);
                }
            }

            // Если хотя бы одно не скачалось => загрузка не считается успешной
            for (int currentBool = 0; currentBool < successDownload.Count; currentBool++)
            {
                if (!successDownload[currentBool])
                    return false;
            }
            return true;
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
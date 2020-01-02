using System;
using System.Threading;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using VkNet.Model.RequestParams;
using Newtonsoft.Json;

namespace schedulebot
{
    public static class Schedule
    {  
        public static void СheckRelevance()
        {
            string url = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
            HtmlWeb web;
            HtmlAgilityPack.HtmlDocument doc;
            HtmlNodeCollection nodes;
            StreamWriter fileWrite;
            while (true)
            {
                Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Проверка актуальности"); // log
                web = new HtmlWeb();
                try
                {
                    doc = web.Load(url);
                }
                catch (WebException)
                {
                    Random random = new Random();
                    var messagesSendParams = new MessagesSendParams()
                    {
                        UserId = 135696841,
                        Message = "Не могу загрузить сайт",
                        RandomId = random.Next()
                    };
                    Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                    Thread.Sleep(600000);
                    continue;
                }
                catch
                {
                    Random random = new Random();
                    var messagesSendParams = new MessagesSendParams()
                    {
                        UserId = 135696841,
                        Message = "Не могу загрузить сайт. Ошибка не WebException",
                        RandomId = random.Next()
                    };
                    Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                    Thread.Sleep(600000);
                    continue;
                }
                string[] parse = new string[8] { "", "", "", "", "", "", "", "" };
                bool[] check = new bool[4] { true, true, true, true };
                int i = 0;
                nodes = doc.DocumentNode.SelectNodes("//p[contains(text(), 'Рас­пи­са­ние ба­ка­лав­ров')]");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        if (i == 4)
                            break;
                        string text = node.InnerText;
                        if (text.Contains("(от "))
                        {
                            int pos = text.LastIndexOf("(") + 1;
                            int length = text.LastIndexOf(")") - pos;
                            parse[i * 2] = text.Substring(pos, length);
                            if (parse[i * 2] != Glob.data[i])
                            {
                                check[i] = false;
                                lock (Glob.locker)
                                {
                                    Glob.data[i] = parse[i * 2];
                                }
                            }
                        }
                        ++i;
                    }
                    i = 0;
                    nodes = doc.DocumentNode.SelectNodes("//a[contains(text(), 'ска­чать')]");
                    if (nodes != null)
                    {
                        lock (Glob.locker)
                        {
                            foreach (var node in nodes)
                            {
                                if (i == 4)
                                    break;
                                parse[i * 2 + 1] = node.Attributes["href"].Value;
                                Glob.schedule_url[i] = parse[i * 2 + 1];
                                ++i;
                            }
                        }
                        if (!check[0] || !check[1] || !check[2] || !check[3])
                            Update(check);
                    }
                }
                Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Проверка актуальности"); // log
                lock (Glob.locker)
                {
                    if (Glob.subsChanges)
                    {
                        IO.SaveSubscribers();
                    }
                }
                fileWrite = new StreamWriter(
                    Const.path_config + "parse.txt",
                    false,
                    System.Text.Encoding.Default);
                for (i = 0; i < 4; ++i) // Запись новой информации в файл с ссылками и датами
                {
                    fileWrite.WriteLine(Glob.data[i]);
                    fileWrite.WriteLine(Glob.schedule_url[i]);
                }
                fileWrite.Close();
                web = null;
                doc = null;
                nodes = null;
                fileWrite = null;
                Thread.Sleep(600000);
            }
        }
        public static void Update(bool[] check)
        {
            int[,,] sendScheduleUpdateGroups = new int[4, 2, 101];
            int[,,] temp = new int[4, 2, 101];
            lock (Glob.lockerIsUpdating)
            {
                Glob.isUpdating = true;
            }
            for (int i = 0; i < 4; ++i)
            {
                if (!check[i])
                {
                    temp = UpdateCourse(i, sendScheduleUpdateGroups);
                    if (temp == null)
                    {
                        sendScheduleUpdateGroups[i, 0, 100] = 0;
                    }
                    else
                    {
                        sendScheduleUpdateGroups = temp;
                    }
                }
                else
                {
                    sendScheduleUpdateGroups[i, 0, 100] = 0;
                }
            }
            lock (Glob.locker)
                Glob.tomorrow_uploaded = new ulong[4, 40, 6, 2];
            Utils.ScheduleMapping();
            Utils.TomorrowStuding();
            Utils.СonstructingKeyboards();
            IO.SaveUploadedSchedule();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < sendScheduleUpdateGroups[i, 0, 100]; ++j)
                {
                    Distribution.ScheduleUpdate(sendScheduleUpdateGroups[i, 0, j], sendScheduleUpdateGroups[i, 1, j]);
                }
            }
            lock (Glob.lockerIsUpdating)
            {
                Glob.isUpdating = false;
            }
        }
        public static int[,,] UpdateCourse(int index, int[,,] sendScheduleUpdateGroups, bool sendUpdates = true)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Обновление расписания   " + index); // log
            //string url;
            string data;
            lock (Glob.locker)
            {
               //url = Glob.schedule_url[index];
                data = Glob.data[index];
            }
            if (sendUpdates)
                Distribution.ToCourse(index, "Вышло новое расписание " + data + ". Ожидайте результата обработки.");
            // Скачивание
            int downloadErrorsCount = 0;
            while (true)
            {
                try
                {
                    Download(index);
                    break;
                }
                catch (WebException)
                {
                    Random random = new Random();
                    if (downloadErrorsCount > 5)
                    {
                        var messagesSendParams = new MessagesSendParams()
                        {
                            UserId = 135696841,
                            Message = "Не могу скачать расписание " + (index + 1).ToString() + " курса. Попробую через минуту",
                            RandomId = random.Next()
                        };
                        Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                        Thread.Sleep(60000);
                    }
                    else if (downloadErrorsCount == 15)
                    {
                        // Рассылка, что не смог скачать расписание
                        string tempUrl, tempData;
                        lock (Glob.locker)
                        {
                            tempUrl = Glob.schedule_url[index];
                            tempData = Glob.data[index];
                        }
                        lock (Glob.lockerIsBroken)
                        {
                            Glob.isBroken[index] = true;
                        }
                        if (sendUpdates)
                            Distribution.ToCourse(index, "Не удалось скачать расписание " + tempData + ". Ссылка: " + tempUrl);
                        return null;
                    }
                    else
                    {
                        var messagesSendParams = new MessagesSendParams()
                        {
                            UserId = 135696841,
                            Message = "Не могу скачать расписание " + (index + 1).ToString() + " курса. Попробую через 10 секунд",
                            RandomId = random.Next()
                        };
                        Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
                        Thread.Sleep(10000);
                    }
                    ++downloadErrorsCount;
                }
            }
            // Обработка
            try
            {
                sendScheduleUpdateGroups = Parse.Schedule(index, sendScheduleUpdateGroups, sendUpdates);
            }
            catch
            {
                // не удалось обработать ;(
                string tempUrl, tempData;
                lock (Glob.locker)
                {
                    tempUrl = Glob.schedule_url[index];
                    tempData = Glob.data[index];
                }
                lock (Glob.lockerIsBroken)
                {
                    Glob.isBroken[index] = true;
                }
                if (sendUpdates)
                    Distribution.ToCourse(index, "Не удалось обработать расписание " + tempData + ". Ссылка: " + tempUrl);
                return null;
            }
            lock (Glob.lockerIsBroken)
            {
                Glob.isBroken[index] = false;
            }
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Обновление расписания   " + index); // log
            return sendScheduleUpdateGroups;
        }
        public static void Download(int index)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S]  -> Скачивание расписания"); // log
            WebClient webClient = new WebClient();
            string local_name = (index + 1).ToString() + "_course_schedule.xls"; // название файла
            webClient.DownloadFile(Glob.schedule_url[index], Const.path_downloads + local_name);
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E]  -> Скачивание расписания"); // log
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Exception;
using VkNet.Enums.SafetyEnums;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;


using Schedulebot.Vk;

namespace Schedulebot
{
    public class ItmmDepartment : IDepartment
    {
        private readonly string path;
        private VkStuff vkStuff = new VkStuff();
        private ICheckRelevanceStuff checkRelevanceStuffITMM = new CheckRelevanceStuffITMM();
        // private Dictionary<string, string> acronymToPhrase;
        // private Dictionary<string, string> doubleOptionallySubject;
        // private List<string> fullName;
        public int CoursesAmount { get; set; }
        private Course[] courses = new Course[4]; // 4 курса всегда ЫЫЫЫ

        private UserRepository userRepository = new UserRepository();
        private int startDay;
        private int startWeek;
        public ItmmDepartment(string _path)
        {
            path = _path + @"itmm\"; // todo: вынести в LoadSettings()
            vkStuff.MainMenuKeyboards = new MessageKeyboard[5]
            {
                // main
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Расписание",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Неделя",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Настройки",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Информация",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // schedule
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "На неделю",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "На сегодня",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "На завтра",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Ссылка",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // settings
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Negative,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Отписаться",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Positive,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Подписаться",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Изменить подгруппу",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // выбор курса
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Выберите курс",
                                    Payload = "{\"menu\": \"3\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "1",
                                    Payload = "{\"menu\": \"3\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "2",
                                    Payload = "{\"menu\": \"3\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "3",
                                    Payload = "{\"menu\": \"3\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "4",
                                    Payload = "{\"menu\": \"3\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"3\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // выбор подгруппы
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "1",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "2",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                }
            };
            LoadSettings();
            LoadUsers();
            LoadAcronymToPhrase();
            LoadDoubleOptionallySubject();
            LoadFullName();
            for (int i = 0; i < 4; ++i)
                courses[i] = new Course(path + @"downloads\" + i + "_course.xls");
        }
        
        private static class ConstructKeyboardsProperties
        {
            public const int buttonsInLine = 2; // 1..4
            public const int linesInKeyboard = 4; // 1..9 
        }

        private void СonstructKeyboards()
        {
            for (int currentCourse = 0; currentCourse < CoursesAmount; ++currentCourse)
            {
                int groupsAmount = courses[currentCourse].groups.GetLength(0);
                int pagesAmount = (int)Math.Ceiling((double)groupsAmount
                    / (double)(ConstructKeyboardsProperties.linesInKeyboard * ConstructKeyboardsProperties.buttonsInLine));
                int currentPage = 0;
                courses[currentCourse].keyboards = new List<MessageKeyboard>();
                List<MessageKeyboardButton> line = new List<MessageKeyboardButton>();
                List<List<MessageKeyboardButton>> buttons = new List<List<MessageKeyboardButton>>();
                List<MessageKeyboardButton> serviceLine = new List<MessageKeyboardButton>();
                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.GetLength(0); currentGroup++)
                {
                    line.Add(new MessageKeyboardButton()
                    {
                        Color = KeyboardButtonColor.Primary,
                        Action = new MessageKeyboardButtonAction
                        {
                            Type = KeyboardButtonActionType.Text,
                            Label = courses[currentCourse].groups[currentGroup].name,
                            Payload = "{\"menu\": \"30\", \"index\": \"" + currentGroup + "\", \"course\": \"" + currentCourse + "\"}"
                        }
                    });
                    if (line.Count == ConstructKeyboardsProperties.buttonsInLine
                        || (currentGroup + 1 == groupsAmount && line.Count != 0))
                    {
                        buttons.Add(new List<MessageKeyboardButton>(line));
                        line.Clear();
                    }
                    if (buttons.Count == ConstructKeyboardsProperties.linesInKeyboard
                        || (currentGroup + 1 == groupsAmount && buttons.Count != 0))
                    {
                        string payloadService = "{\"menu\": \"30\", \"page\": \"" + currentPage + "\", \"course\": \"" + currentCourse + "\"}";
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = "Назад",
                                Payload = payloadService
                            }
                        });
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = (currentPage + 1) + " из " + pagesAmount,
                                Payload = payloadService
                            }
                        });
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = "Вперед",
                                Payload = payloadService
                            }
                        });
                        buttons.Add(new List<MessageKeyboardButton>(serviceLine));
                        serviceLine.Clear();
                        courses[currentCourse].keyboards.Add(new MessageKeyboard
                        {
                            Buttons = new List<List<MessageKeyboardButton>>(buttons),
                            OneTime = false
                        });
                        buttons.Clear();
                        ++currentPage;
                    }
                }
            }
        }

        public string CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            if ((DateTime.Now.DayOfYear - startDay) / 7 % 2 != startWeek)
            {
                return "Нижняя";
            }
            return "Верхняя";
        }
        
        public void LoadSettings()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка настроек");
            using (StreamReader file = new StreamReader(
                path + "settings.txt",
                System.Text.Encoding.Default))
            {
                string str, value;
                while ((str = file.ReadLine()) != null)
                {
                    if (str.Contains(':'))
                    {
                        value = str.Substring(str.IndexOf(':') + 1);
                        str = str.Substring(0, str.IndexOf(':'));
                        switch (str)
                        {
                            case "key":
                            {
                                vkStuff.api.Authorize(new ApiAuthParams() { AccessToken = value });
                                break;
                            }
                            case "keyPhotos":
                            {
                                vkStuff.apiPhotos.Authorize(new ApiAuthParams() { AccessToken = value });
                                break;
                            }
                            case "groupId":
                            {
                                vkStuff.GroupId = long.Parse(value);
                                break;
                            }
                            case "mainAlbumId":
                            {
                                vkStuff.MainAlbumId = Int64.Parse(value);
                                break;
                            }
                            // case "tomorrowAlbumId":
                            // {
                            //     vkStuff.TomorrowAlbumId = Int64.Parse(value);
                            //     break;
                            // }
                            case "startDay":
                            {
                                startDay = Int32.Parse(value);
                                break;
                            }
                            case "startWeek":
                            {
                                startWeek = Int32.Parse(value);
                                break;
                            }
                            case "groupUrl":
                            {
                                // todo
                                break;
                            }
                            case "path":
                            {
                                // todo примерно itmm/
                                break;
                            }
                        }
                    }
                }
            }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка настроек");
        }

        private void LoadAcronymToPhrase()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка ManualAcronymToPhrase");
            Glob.acronymToPhrase = new Dictionary<string,string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/acronymToPhrase.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                Glob.acronymToPhrase.Add(file.ReadLine(), file.ReadLine());
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка ManualAcronymToPhrase");
        }

        private void LoadDoubleOptionallySubject()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка DoubleOptionallySubject");
            Glob.doubleOptionallySubject = new Dictionary<string,string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/doubleOptionallySubject.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                Glob.doubleOptionallySubject.Add(file.ReadLine(), file.ReadLine());
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка DoubleOptionallySubject");
        }

        private void LoadFullName()
        {
            Glob.fullName = new List<string>();
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/fullName.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                Glob.fullName.Add(file.ReadLine());
        }

        public void LoadUsers()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка подписанных");
            using (StreamReader file = new StreamReader(
                path + "users.txt",
                System.Text.Encoding.Default))
            {
                while (!file.EndOfStream)
                {
                    if (User.TryParseUser(file.ReadLine(), out var user))
                        userRepository.AddUser(user);
                }
            }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка подписанных");
        }

        public async void SaveUsers()
        {
            using (StreamWriter file = new StreamWriter(path + "users.txt"))
            await file.WriteLineAsync(userRepository.ToString());
        }
        
        public async Task GetMessagesAsync()
        {
            await Task.Run(() =>
            {
                LongPollServerResponse serverResponse = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
                BotsLongPollHistoryResponse historyResponse = null;
                BotsLongPollHistoryParams botsLongPollHistoryParams = new BotsLongPollHistoryParams()
                {
                    Server = serverResponse.Server,
                    Ts = serverResponse.Ts,
                    Key = serverResponse.Key,
                    Wait = 25
                };
                while (true)
                {
                    // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " Получаю сообщения");
                    try
                    {
                        historyResponse = vkStuff.api.Groups.GetBotsLongPollHistory(botsLongPollHistoryParams);
                        if (historyResponse == null)
                            continue;
                        botsLongPollHistoryParams.Ts = historyResponse.Ts;
                        if (!historyResponse.Updates.Any())
                            continue;
                        foreach (var update in historyResponse.Updates)
                        {
                            if (update.Type == GroupUpdateType.MessageNew)
                            {
                                MessageResponseAsync(update.Message);
                            }
                        }
                        historyResponse = null;
                    }
                    catch (LongPollException exception)
                    {
                        if (exception is LongPollOutdateException outdateException)
                            botsLongPollHistoryParams.Ts = outdateException.Ts;
                        else
                        {
                            LongPollServerResponse server = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
                            botsLongPollHistoryParams.Ts = server.Ts;
                            botsLongPollHistoryParams.Key = server.Key;
                            botsLongPollHistoryParams.Server = server.Server;
                        }
                    }
                    catch (Exception exception)
                    {
                        // todo: long poll error
                        LongPollServerResponse server = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
                        botsLongPollHistoryParams.Ts = server.Ts;
                        botsLongPollHistoryParams.Key = server.Key;
                        botsLongPollHistoryParams.Server = server.Server;
                        // Console.WriteLine("Long poll error = " + e);
                    }
                }
            });
        }

        public class PayloadStuff
        {
            public string Command { get; set; }
            public int? Menu { get; set; }
            public int? Course { get; set; }
            public int? Index { get; set; }
        }

        public async void MessageResponseAsync(Message message)
        {
            await Task.Run(() =>
            {
                if (message.Payload == null)
                {
                    // todo: Переписать админку
                    if (message.PeerId == vkStuff.AdminId)
                    {
                        /*
                        if (message.Text.IndexOf("Помощь") == 0 || message.Text.IndexOf("Help") == 0)
                        {
                            string help = "Команды:\n\nРассылка <всем,*КУРС*,*ГРУППА*>\n--отправляет расписание на неделю выбранным юзерам\nОбновить <все,*КУРС*> [нет]\n--обновляет расписание для выбранных курсов, отправлять ли обновление юзерам (по умолчанию - да)\nПерезагрузка\n--перезагружает бота(для применения обновления версии бота)\n\nCommands:\n\nDistribution <all,*COURSE*,*GROUP*>\n--отправляет расписание на неделю выбранным юзерам\nUpdate <all,*COURSE*> [false]\n--обновляет расписание для выбранных курсов, отправлять ли обновление юзерам (по умолчанию - да)\nReboot\n--перезагружает бота(для применения обновления версии бота)\n";
                            SendMessage(userId: message.PeerId, message: help);
                        }
                        else if (message.Text.IndexOf("Рассылка") == 0 || message.Text.IndexOf("Distribution") == 0)
                        {
                            string temp = message.Text.Substring(message.Text.IndexOf(' ') + 1);
                            string toWhom = temp.Substring(0, temp.IndexOf(' '));
                            temp = temp.Substring(temp.IndexOf(' ') + 1); // сообщение
                            if (toWhom == "всем" || toWhom == "all")
                            {
                                Distribution.ToAll(temp);
                                SendMessage(userId: message.PeerId, message: "Выполнено");
                            }
                            else if (toWhom.Length == 1)
                            {
                                int toCourse = 0;
                                int.TryParse(toWhom, out toCourse);
                                --toCourse;
                                if (toCourse != -1 && toCourse >= 0 && toCourse < 4)
                                {
                                    Distribution.ToCourse(toCourse, temp);
                                    SendMessage(userId: message.PeerId, message: "Выполнено");
                                }
                                else
                                {
                                    SendMessage(userId: message.PeerId, message: "Ошибка рассылки:\nневерный курс: " + toWhom + "\nВведите значение от 1 до 4");
                                }
                            }
                            else
                            {
                                Distribution.ToGroup(toWhom, temp);
                                SendMessage(userId: message.PeerId, message: "Выполнено");
                            }
                        }
                        else if (message.Text.IndexOf("Обновить") == 0 || message.Text.IndexOf("Update") == 0)
                        {
                            string temp = message.Text.Substring(message.Text.IndexOf(' ') + 1);
                            bool sendUpdates = true;
                            string course = temp.Substring(0, temp.IndexOf(' '));
                            temp = temp.Substring(temp.IndexOf(' ') + 1);
                            if (temp == "нет" || temp == "false")
                                sendUpdates = false;
                            if (course == "все" || course == "all")
                            {
                                int[,,] sendScheduleUpdateGroups = new int[4, 2, 101];
                                lock (Glob.lockerIsUpdating)
                                {
                                    Glob.isUpdating = true;
                                }
                                for (int i = 0; i < 4; ++i)
                                {
                                    int[,,] tempM = new int[4, 2, 101];
                                    tempM = Schedule.UpdateCourse(i, sendScheduleUpdateGroups, sendUpdates);
                                    if (tempM == null)
                                    {
                                        sendScheduleUpdateGroups[i, 0, 100] = 0;
                                    }
                                    else
                                    {
                                        sendScheduleUpdateGroups = tempM;
                                    }
                                }
                                if (sendScheduleUpdateGroups[0, 0, 100] == 0 && sendScheduleUpdateGroups[1, 0, 100] == 0
                                    && sendScheduleUpdateGroups[2, 0, 100] == 0 && sendScheduleUpdateGroups[3, 0, 100] == 0)
                                {
                                    lock (Glob.lockerIsUpdating)
                                    {
                                        Glob.isUpdating = false;
                                    }
                                }
                                else
                                {
                                    lock (Glob.locker)
                                        Glob.tomorrow_uploaded = new ulong[4, 40, 6, 2];
                                    Utils.ScheduleMapping();
                                    Utils.TomorrowStudying();
                                    Utils.СonstructingKeyboards();
                                    IO.SaveUploadedSchedule();
                                    if (sendUpdates)
                                    {
                                        for (int l = 0; l < 4; ++l)
                                        {
                                            for (int j = 0; j < sendScheduleUpdateGroups[l, 0, 100]; ++j)
                                            {
                                                Distribution.ScheduleUpdate(sendScheduleUpdateGroups[l, 0, j], sendScheduleUpdateGroups[l, 1, j]);
                                            }
                                        }
                                    }
                                    lock (Glob.lockerIsUpdating)
                                    {
                                        Glob.isUpdating = false;
                                    }
                                }
                            }
                            else if (course.Length == 1)
                            {
                                int courseI = -1;
                                int.TryParse(course, out courseI);
                                if (courseI >= 0 && courseI <= 3)
                                {
                                    int[,,] sendScheduleUpdateGroups = new int[4, 2, 101];
                                    lock (Glob.lockerIsUpdating)
                                    {
                                        Glob.isUpdating = true;
                                    }
                                    sendScheduleUpdateGroups = Schedule.UpdateCourse(courseI, sendScheduleUpdateGroups, sendUpdates);
                                    if (sendScheduleUpdateGroups == null)
                                    {
                                        lock (Glob.lockerIsUpdating)
                                        {
                                            Glob.isUpdating = false;
                                        }
                                    }
                                    else if (sendScheduleUpdateGroups[courseI, 0, 100] != 0)
                                    {
                                        lock (Glob.locker)
                                            Glob.tomorrow_uploaded = new ulong[4, 40, 6, 2];
                                        Utils.ScheduleMapping();
                                        Utils.TomorrowStudying();
                                        Utils.СonstructingKeyboards();
                                        IO.SaveUploadedSchedule();
                                        if (sendUpdates)
                                        {
                                            for (int j = 0; j < sendScheduleUpdateGroups[courseI, 0, 100]; ++j)
                                            {
                                                Distribution.ScheduleUpdate(sendScheduleUpdateGroups[courseI, 0, j], sendScheduleUpdateGroups[courseI, 1, j]);
                                            }
                                        }
                                        lock (Glob.lockerIsUpdating)
                                        {
                                            Glob.isUpdating = false;
                                        }
                                    }
                                    else
                                    {
                                        lock (Glob.lockerIsUpdating)
                                        {
                                            Glob.isUpdating = false;
                                        }
                                    }
                                }
                            }
                            SendMessage(userId: message.PeerId, message: "Выполнено");
                        }
                        else if (message.Text.IndexOf("Перезагрузка") == 0 || message.Text.IndexOf("Reboot") == 0)
                        {
                            while (Glob.isUpdating)
                                Thread.Sleep(60000);
                            Glob.relevanceCheck.Interrupt();
                            while (!Glob.commandsQueue.IsEmpty)
                                Thread.Sleep(5000); 
                            lock (Glob.locker)
                            {
                                if (Glob.subsChanges)
                                {
                                    IO.SaveSubscribers();
                                }
                            }                               
                            Environment.Exit(0);
                        }
                        */
                    }
                    else if (message.Attachments.Count != 0)
                    {
                        // todo: fix
                        if (message.Attachments.Single().ToString() == "Sticker")
                        {
                            
                            SendMessage(userId: message.PeerId,
                                        message: "🤡");
                            return;
                        }
                    }
                    else
                    {
                        SendMessage(userId: message.PeerId,
                                    message: "Нажмите на кнопку");
                        return;
                    }
                    return;
                }
                PayloadStuff payloadStuff = Newtonsoft.Json.JsonConvert.DeserializeObject<PayloadStuff>(message.Payload);
                if (payloadStuff.Command == "start")
                {
                    SendMessage(userId: message.PeerId,
                                message: "Здравствуйтe, я буду присылать актуальное расписание, если Вы подпишитесь в настройках.\nКнопка \"Информация\" для получения подробностей",
                                keyboardId: 0);
                    return;
                }
                // По idшникам меню сортируем сообщения
                switch (payloadStuff.Menu)
                {
                    case null:
                    {
                        SendMessage(userId: message.PeerId,
                                    message: "Что-то пошло не так",
                                    keyboardId: 0);
                        return;
                    }
                    case 0:
                    {
                        switch (message.Text)
                        {
                            case "Расписание":
                            {
                                SendMessage(userId: message.PeerId,
                                            keyboardId: 1);
                                return;
                            }
                            case "Неделя":
                            {
                                SendMessage(userId: message.PeerId,
                                            message: CurrentWeek());
                                return;
                            }
                            case "Настройки":
                            {
                                MessageKeyboard keyboardCustom;
                                keyboardCustom = vkStuff.MainMenuKeyboards[2];
                                //!
                                if (!userRepository.ContainsUser(message.PeerId))
                                {
                                    keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                }
                                else
                                {
                                    // keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "Информация":
                            {
                                SendMessage(userId: message.PeerId,
                                            message: "Текущая версия - v2.2\n\nПри обновлении расписания на сайте Вам придёт сообщение. Далее Вы получите одно из трех сообщений:\n 1) Новое расписание *картинка*\n 2) Для Вас изменений нет\n 3) Не удалось скачать/обработать расписание *ссылка*\n Если не придёт никакого сообщения, Ваша группа скорее всего изменилась/не найдена. Настройте заново.\n\nВ расписании могут встретиться верхние индексы, предупреждающие о возможных ошибках. Советую ознакомиться со статьёй: vk.com/@itmmschedulebot-raspisanie");
                                return;
                            }
                            default:
                            {
                                SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 0, что-то с message.Text", keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 1:
                    {

                        return;
                    }
                    case 2:
                    {

                        return;
                    }
                    case 3:
                    {

                        return;
                    }
                    case 4:
                    {

                        return;
                    }
                    case 30:
                    {


                        return;
                    }
                }
                


                /*
                Regex regex = new Regex("[0-9]+");
                int[] args = new int[4] { -1, -1, -1, -1 };
                MatchCollection matches = regex.Matches(message.Payload);
                if (matches.Count != 0)
                {
                    for (int i = 0; i < matches.Count; ++i)
                    {
                        args[i] = int.Parse(matches[i].Value);
                    }
                }
                else
                {
                    SendMessage(userId: message.PeerId, message: "Здравствуйтe, я буду присылать актуальное расписание, если Вы подпишитесь в настройках.\nКнопка \"Информация\" для получения подробностей", keyboardId: 0);
                    return;
                }
                */


                /*
                switch (args[0])
                {
                    case -1: // в случае ошибки
                    {
                        SendMessage(userId: message.PeerId, message: "Что-то пошло не так", keyboardId: 0);
                        return;
                    }
                    case 0: // сделать информацию
                    {
                        switch (message.Text)
                        {
                            case "Расписание":
                            {
                                SendMessage(userId: message.PeerId, keyboardId: 1);
                                return;
                            }
                            case "Неделя":
                            {
                                SendMessage(userId: message.PeerId, message: CurrentWeek(), keyboardId: 0);
                                return;
                            }
                            case "Настройки":
                            {
                                MessageKeyboard keyboardCustom;
                                keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                lock (Glob.locker)
                                {
                                    if (!Glob.users.Keys.Contains(message.PeerId))
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                    }
                                    else
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                    }
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "Информация":
                            {
                                SendMessage(userId: message.PeerId, message: "Текущая версия - v2.2\n\nПри обновлении расписания на сайте Вам придёт сообщение. Далее Вы получите одно из трех сообщений:\n 1) Новое расписание *картинка*\n 2) Для Вас изменений нет\n 3) Не удалось скачать/обработать расписание *ссылка*\n Если не придёт никакого сообщения, Ваша группа скорее всего изменилась/не найдена. Настройте заново.\n\nВ расписании могут встретиться верхние индексы, предупреждающие о возможных ошибках. Советую ознакомиться со статьёй: vk.com/@itmmschedulebot-raspisanie", keyboardId: 0);
                                return;
                            }
                            default:
                            {
                                SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 0, что-то с message.Text", keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 1:
                    {
                        bool isUpdating;
                        lock (Glob.lockerIsUpdating)
                        {
                            isUpdating = Glob.isUpdating;
                        }
                        if (message.Text == "Назад")
                        {
                            SendMessage(userId: message.PeerId, keyboardId: 0);
                            return;
                        }
                        else if (message.Text == "Ссылка")
                        {
                            MessageKeyboard keyboardCustom;
                            bool contains;
                            lock (Glob.lockerKeyboards)
                            {
                                keyboardCustom = vkStuff.mainMenuKeyboards[2];
                            }
                            lock (Glob.locker)
                            {
                                contains = Glob.users.Keys.Contains(message.PeerId);
                            }
                            if (!contains)
                            {
                                keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            else
                            {
                                lock (Glob.locker)
                                {
                                    contains = Glob.schedule_mapping.ContainsKey(Glob.users[message.PeerId]);
                                }
                                if (contains)
                                {
                                    int course;
                                    string url;
                                    lock (Glob.locker)
                                    {
                                        course = Glob.schedule_mapping[Glob.users[message.PeerId]].Course;
                                        url = Glob.schedule_url[course];
                                    }
                                    SendMessage(
                                        userId: message.PeerId,
                                        message: "Расписание для " + (course + 1) + " курса: " + url,
                                        keyboardId: 1,
                                        customKeyboard: keyboardCustom,
                                        onlyKeyboard: true);
                                    return;
                                }
                                else
                                {
                                    keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                    SendMessage(
                                        userId: message.PeerId,
                                        message: "Ваша группа не существует, настройте заново",
                                        keyboardId: -1,
                                        customKeyboard: keyboardCustom,
                                        onlyKeyboard: true);
                                    return;
                                }
                            }
                        }
                        else if (isUpdating)
                        {
                            SendMessage(
                                userId: message.PeerId,
                                message: "Происходит обновление расписания, повторите попытку через несколько минут",
                                keyboardId: 1);
                            return;
                        }
                        else
                        {
                            switch (message.Text)
                            {
                                case "На неделю":
                                {
                                    MessageKeyboard keyboardCustom;
                                    string messageTemp;
                                    long? photoId = null;
                                    bool contains;
                                    lock (Glob.lockerKeyboards)
                                    {
                                        keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                    }
                                    lock (Glob.locker)
                                    {
                                        contains = Glob.users.Keys.Contains(message.PeerId);
                                    }
                                    if (!contains)
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                        SendMessage(
                                            userId: message.PeerId,
                                            message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                                            keyboardId: -1,
                                            customKeyboard: keyboardCustom);
                                        return;
                                    }
                                    else
                                    {
                                        lock (Glob.locker)
                                        {
                                            contains = Glob.schedule_mapping.ContainsKey(Glob.users[message.PeerId]);
                                        }
                                        if (contains)
                                        {
                                            bool isBroken;
                                            lock (Glob.lockerIsBroken)
                                            {
                                                isBroken = Glob.isBroken[Glob.schedule_mapping[Glob.users[message.PeerId]].Course];
                                            }
                                            if (isBroken)
                                            {
                                                SendMessage(
                                                    userId: message.PeerId,
                                                    message: "Расписание Вашего курса не обработано",
                                                    keyboardId: 1);
                                                return;
                                            }
                                            else
                                            {
                                                lock (Glob.locker)
                                                {
                                                    messageTemp = "Расписание для " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                                    photoId = (long)Glob.schedule_uploaded[Glob.schedule_mapping[Glob.users[message.PeerId]].Course, Glob.schedule_mapping[Glob.users[message.PeerId]].Index];
                                                }
                                                SendMessage(
                                                    userId: message.PeerId,
                                                    message: messageTemp,
                                                    keyboardId: 1,
                                                    attachments: new List<MediaAttachment>
                                                    {
                                                        new Photo()
                                                        {
                                                            AlbumId = Const.mainAlbumId,
                                                            OwnerId = -178155012,
                                                            Id = photoId
                                                        }
                                                    });
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                            SendMessage(
                                                userId: message.PeerId,
                                                message: "Ваша группа не существует, настройте заново",
                                                keyboardId: -1,
                                                customKeyboard: keyboardCustom,
                                                onlyKeyboard: true);
                                            return;
                                        }
                                    }
                                }
                                case "На сегодня":
                                {
                                    MessageKeyboard keyboardCustom;
                                    bool contains;
                                    lock (Glob.lockerKeyboards)
                                    {
                                        keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                    }
                                    lock (Glob.locker)
                                    {
                                        contains = Glob.users.Keys.Contains(message.PeerId);
                                    }
                                    if (!contains)
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                        SendMessage(
                                            userId: message.PeerId,
                                            message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                                            keyboardId: -1,
                                            customKeyboard: keyboardCustom);
                                        return;
                                    }
                                    else
                                    {
                                        int week = 0;
                                        if ((DateTime.Now.DayOfYear - Glob.startDay) / 7 % 2 == 0)
                                        {
                                            week = 1;
                                        }
                                        int today = (int)DateTime.Now.DayOfWeek;
                                        if (today == 0)
                                        {
                                            SendMessage(
                                                userId: message.PeerId,
                                                message: "Сегодня воскресенье",
                                                keyboardId: 1);
                                            return;
                                        }
                                        else
                                            --today;
                                        lock (Glob.locker)
                                        {
                                            contains = Glob.schedule_mapping.ContainsKey(Glob.users[message.PeerId]);
                                        }
                                        if (contains)
                                        {
                                            bool isBroken;
                                            lock (Glob.lockerIsBroken)
                                            {
                                                isBroken = Glob.isBroken[Glob.schedule_mapping[Glob.users[message.PeerId]].Course];
                                            }
                                            if (isBroken)
                                            {
                                                SendMessage(
                                                    userId: message.PeerId,
                                                    message: "Расписание Вашего курса не обработано",
                                                    keyboardId: 1);
                                                return;
                                            }
                                            else
                                            {
                                                Mapping mapping;
                                                ulong photoId;
                                                bool study;
                                                lock (Glob.locker)
                                                {
                                                    mapping = Glob.schedule_mapping[Glob.users[message.PeerId]];
                                                    study = Glob.tomorrow_studying[mapping.Course, mapping.Index, today, week];
                                                }
                                                if (study)
                                                {
                                                    lock (Glob.locker)
                                                    {   
                                                        photoId = Glob.tomorrow_uploaded[mapping.Course, mapping.Index, today, week];
                                                    }
                                                    if (photoId == 0)
                                                    {
                                                        Process.TomorrowSchedule(mapping.Course, mapping.Index, today, week);
                                                        lock (Glob.locker)
                                                        {
                                                            photoId = Glob.tomorrow_uploaded[mapping.Course, mapping.Index, today, week];
                                                        }
                                                    }
                                                    SendMessage(
                                                        userId: message.PeerId,
                                                        message: "Расписание на сегодня",
                                                        keyboardId: 1,
                                                        attachments: new List<MediaAttachment>
                                                        {
                                                            new Photo()
                                                            {
                                                                AlbumId = Const.tomorrowAlbumId,
                                                                OwnerId = -178155012,
                                                                Id = (long?)photoId
                                                            }
                                                        });
                                                    return;
                                                }
                                                else
                                                {
                                                    SendMessage(
                                                        userId: message.PeerId,
                                                        message: "Сегодня Вы не учитесь",
                                                        keyboardId: 1);
                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                            SendMessage(
                                                userId: message.PeerId,
                                                message: "Ваша группа не существует, настройте заново",
                                                keyboardId: -1,
                                                customKeyboard: keyboardCustom,
                                                onlyKeyboard: true);
                                            return;
                                        }
                                    }
                                }
                                case "На завтра":
                                {
                                    MessageKeyboard keyboardCustom;
                                    bool contains;
                                    lock (Glob.lockerKeyboards)
                                    {
                                        keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                    }
                                    lock (Glob.locker)
                                    {
                                        contains = Glob.users.Keys.Contains(message.PeerId);
                                    }
                                    if (!contains)
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                        SendMessage(
                                            userId: message.PeerId,
                                            message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                                            keyboardId: -1,
                                            customKeyboard: keyboardCustom,
                                            onlyKeyboard: true);
                                        return;
                                    }
                                    else
                                    {
                                        int week = 0;
                                        if ((DateTime.Now.DayOfYear - Glob.startDay) / 7 % 2 == 0)
                                        {
                                            week = 1;
                                        }
                                        int today = (int)DateTime.Now.DayOfWeek;
                                        Console.WriteLine(today + " " + week);
                                        lock (Glob.locker)
                                        {
                                            contains = Glob.schedule_mapping.ContainsKey(Glob.users[message.PeerId]);
                                        }
                                        if (contains)
                                        {
                                            bool isBroken;
                                            lock (Glob.lockerIsBroken)
                                            {
                                                isBroken = Glob.isBroken[Glob.schedule_mapping[Glob.users[message.PeerId]].Course];
                                            }
                                            if (isBroken)
                                            {
                                                SendMessage(
                                                    userId: message.PeerId,
                                                    message: "Расписание Вашего курса не обработано",
                                                    keyboardId: 1);
                                                return;
                                            }
                                            else
                                            {
                                                Mapping mapping;
                                                lock (Glob.locker)
                                                {
                                                    mapping = Glob.schedule_mapping[Glob.users[message.PeerId]];
                                                }
                                                if (today == 6)
                                                {
                                                    week = (week + 1) % 2;
                                                    int day = 0;
                                                    ulong photoId;
                                                    lock (Glob.locker)
                                                    {
                                                        while (!Glob.tomorrow_studying[mapping.Course, mapping.Index, day, week])
                                                        {
                                                            ++day;
                                                            if (day == 6)
                                                            {
                                                                day = 0;
                                                                week = (week + 1) % 2;
                                                            }
                                                        }
                                                        photoId = Glob.tomorrow_uploaded[mapping.Course, mapping.Index, day, week];
                                                    }
                                                    if (photoId == 0)
                                                    {
                                                        Process.TomorrowSchedule(mapping.Course, mapping.Index, day, week);
                                                        lock (Glob.locker)
                                                        {
                                                            photoId = Glob.tomorrow_uploaded[mapping.Course, mapping.Index, day, week];
                                                        }
                                                    }
                                                    SendMessage(
                                                        userId: message.PeerId,
                                                        message: "Завтра воскресенье, вот расписание на ближайший учебный день",
                                                        keyboardId: 1,
                                                        attachments: new List<MediaAttachment>
                                                        {
                                                            new Photo()
                                                            {
                                                                AlbumId = Const.tomorrowAlbumId,
                                                                OwnerId = -178155012,
                                                                Id = (long?)photoId
                                                            }
                                                        });
                                                    return;
                                                }
                                                else
                                                {
                                                    int day = today;
                                                    if (today == 0)
                                                        week = (week + 1) % 2;
                                                    int weekTemp = week;
                                                    ulong photoId;
                                                    lock (Glob.locker)
                                                    {
                                                        while (!Glob.tomorrow_studying[mapping.Course, mapping.Index, day, week])
                                                        {
                                                            ++day;
                                                            if (day == 6)
                                                            {
                                                                day = 0;
                                                                week = (week + 1) % 2;
                                                            }
                                                        }
                                                        photoId = Glob.tomorrow_uploaded[mapping.Course, mapping.Index, day, week];
                                                    }
                                                    if (photoId == 0)
                                                    {
                                                        Process.TomorrowSchedule(mapping.Course, mapping.Index, day, week);
                                                        lock (Glob.locker)
                                                        {
                                                            photoId = Glob.tomorrow_uploaded[mapping.Course, mapping.Index, day, week];
                                                        }
                                                    }
                                                    string messageTemp = "Завтра Вы не учитесь, вот расписание на ближайший учебный день";
                                                    if (day == today && weekTemp == week)
                                                    {
                                                        messageTemp = "Расписание на завтра";
                                                    }
                                                    SendMessage(
                                                        userId: message.PeerId,
                                                        message: messageTemp,
                                                        keyboardId: 1,
                                                        attachments: new List<MediaAttachment>
                                                        {
                                                            new Photo()
                                                            {
                                                                AlbumId = Const.tomorrowAlbumId,
                                                                OwnerId = -178155012,
                                                                Id = (long?)photoId
                                                            }
                                                        });
                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                            SendMessage(
                                                userId: message.PeerId,
                                                message: "Ваша группа не существует, настройте заново",
                                                keyboardId: -1,
                                                customKeyboard: keyboardCustom);
                                            return;
                                        }
                                    }
                                }
                                default:
                                {
                                    SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 1, что-то с message.Text", keyboardId: 0);
                                    return;
                                }
                            }
                        }
                    }
                    case 2:
                    {
                        if (message.Text.Contains("Вы подписаны") || message.Text.Contains("Вы не подписаны"))
                        {
                            MessageKeyboard keyboardCustom;
                            lock (Glob.lockerKeyboards)
                            {
                                keyboardCustom = vkStuff.mainMenuKeyboards[2];
                            }
                            lock (Glob.locker)
                            {
                                if (!Glob.users.Keys.Contains(message.PeerId))
                                {
                                    keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                }
                                else
                                {
                                    keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                }
                            }
                            SendMessage(
                                userId: message.PeerId,
                                message: "Попробуйте нажать на другую кнопку",
                                keyboardId: -1,
                                customKeyboard: keyboardCustom);
                            return;
                        }
                        switch (message.Text)
                        {
                            case "Отписаться":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                }
                                keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                string messageTemp;
                                lock (Glob.locker)
                                {

                                    if (!Glob.users.ContainsKey(message.PeerId))
                                    {
                                        messageTemp = "Вы не подписаны";
                                    }
                                    else
                                    {
                                        messageTemp = "Отменена подписка на " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                        Glob.users.Remove(message.PeerId);
                                        if (!Glob.subsChanges)
                                            Glob.subsChanges = true;
                                    }
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: messageTemp,
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "Подписаться":
                            {
                                SendMessage(userId: message.PeerId, keyboardId: 3);
                                return;
                            }
                            case "Изменить подгруппу":
                            {
                                bool contains = false;
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                }
                                lock (Glob.locker)
                                {
                                    if (!Glob.users.Keys.Contains(message.PeerId))
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                    }
                                    else
                                    {
                                        contains = true;
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group;
                                    }
                                }
                                if (contains)
                                {
                                    User temp;
                                    lock (Glob.locker)
                                    {
                                        temp = Glob.users[message.PeerId];
                                        Glob.users.Remove(message.PeerId);
                                        if (temp.Subgroup == "1")
                                        {
                                            temp.Subgroup = "2";
                                            keyboardCustom.Buttons.First().First().Action.Label += " (2)";
                                        }
                                        else
                                        {
                                            temp.Subgroup = "1";
                                            keyboardCustom.Buttons.First().First().Action.Label += " (1)";
                                        }
                                        Glob.users.Add(message.PeerId, temp);
                                        if (!Glob.subsChanges)
                                            Glob.subsChanges = true;
                                    }
                                    SendMessage(
                                        userId: message.PeerId,
                                        message: "Ваша подгруппа: " + temp.Subgroup,
                                        keyboardId: -1,
                                        customKeyboard: keyboardCustom);
                                    return;
                                }
                                else
                                {
                                    SendMessage(
                                        userId: message.PeerId,
                                        message: "Невозможно изменить подгруппу, Вы не подписаны",
                                        keyboardId: -1,
                                        customKeyboard: keyboardCustom);
                                    return;
                                }
                            }
                            case "Назад":
                            {
                                SendMessage(userId: message.PeerId, keyboardId: 0);
                                return;
                            }
                            default:
                            {
                                SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 2, что-то с message.Text", keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 3:
                    {
                        switch (message.Text)
                        {
                            case "Выберите курс":
                            {
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Попробуйте нажать на другую кнопку",
                                    keyboardId: 3);
                                return;
                            }
                            case "1":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Glob.keyboardsNewSub[0, 0];
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Выберите группу",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "2":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Glob.keyboardsNewSub[1, 0];
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Выберите группу",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "3":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Glob.keyboardsNewSub[2, 0];
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Выберите группу",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "4":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Glob.keyboardsNewSub[3, 0];
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Выберите группу",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "Назад":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = vkStuff.mainMenuKeyboards[2];
                                }
                                lock (Glob.locker)
                                {
                                    if (!Glob.users.Keys.Contains(message.PeerId))
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы не подписаны";
                                    }
                                    else
                                    {
                                        keyboardCustom.Buttons.First().First().Action.Label = "Вы подписаны: " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                    }
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            default:
                            {
                                SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 3, что-то с message.Text", keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 4:
                    {
                        switch (message.Text)
                        {
                            case "1":
                            {
                                string messageTemp;
                                lock (Glob.locker)
                                {
                                    if (Glob.users.ContainsKey(message.PeerId))
                                    {
                                        Glob.users.Remove(message.PeerId);
                                    }
                                    Glob.users.Add(message.PeerId, new User()
                                    {
                                        Group = Glob.schedule[args[2], args[1], 0],
                                        Subgroup = "1"
                                    });
                                    if (!Glob.subsChanges)
                                        Glob.subsChanges = true;
                                    messageTemp = "Вы подписались на " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: messageTemp,
                                    keyboardId: 0);
                                return;
                            }
                            case "2":
                            {
                                string messageTemp;
                                lock (Glob.locker)
                                {
                                    if (Glob.users.ContainsKey(message.PeerId))
                                    {
                                        Glob.users.Remove(message.PeerId);
                                    }
                                    Glob.users.Add(message.PeerId, new User()
                                    {
                                        Group = Glob.schedule[args[2], args[1], 0],
                                        Subgroup = "2"
                                    });
                                    if (!Glob.subsChanges)
                                        Glob.subsChanges = true;
                                    messageTemp = "Вы подписались на " + Glob.users[message.PeerId].Group + " (" + Glob.users[message.PeerId].Subgroup + ")";
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: messageTemp,
                                    keyboardId: 0);
                                return;
                            }
                            case "Назад":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Glob.keyboardsNewSub[args[2], 0];
                                }
                                SendMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: -1,
                                    customKeyboard: keyboardCustom,
                                    onlyKeyboard: true);
                                return;
                            }
                            default:
                            {
                                SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 4, что-то с message.Text", keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 30:
                    {
                        if (message.Payload.Contains("page"))
                        {
                            switch (message.Text)
                            {
                                case "Назад":
                                {
                                    if (args[1] == 0)
                                    {
                                        SendMessage(
                                            userId: message.PeerId,
                                            message: "Отправляю клавиатуру",
                                            keyboardId: 3,
                                            onlyKeyboard: true);
                                        return;
                                    }
                                    else
                                    {
                                        MessageKeyboard keyboardCustom;
                                        lock (Glob.lockerKeyboards)
                                        {
                                            keyboardCustom = Glob.keyboardsNewSub[args[2], args[1] - 1];
                                        }
                                        SendMessage(
                                            userId: message.PeerId,
                                            message: "Отправляю клавиатуру",
                                            onlyKeyboard: true,
                                            keyboardId: -1,
                                            customKeyboard: keyboardCustom);
                                        return;
                                    }
                                }
                                case "Вперед":
                                {
                                    MessageKeyboard keyboardCustom;
                                    lock (Glob.lockerKeyboards)
                                    {
                                        if (args[1] == Glob.keyboardsNewSubCount[args[2]] - 1)
                                        {
                                            keyboardCustom = Glob.keyboardsNewSub[args[2], 0];
                                        }
                                        else
                                        {
                                            keyboardCustom = Glob.keyboardsNewSub[args[2], args[1] + 1];
                                        }
                                    }
                                    SendMessage(
                                        userId: message.PeerId,
                                        message: "Отправляю клавиатуру",
                                        onlyKeyboard: true,
                                        keyboardId: -1,
                                        customKeyboard: keyboardCustom);
                                    return;
                                }
                                default:
                                {
                                    if (message.Text.Contains(" из "))
                                    {
                                        MessageKeyboard keyboardCustom;
                                        lock (Glob.lockerKeyboards)
                                        {
                                            keyboardCustom = Glob.keyboardsNewSub[args[2], args[1]];
                                        }
                                        SendMessage(
                                            userId: message.PeerId,
                                            message: "Меню страниц не реализовано",
                                            keyboardId: -1,
                                            customKeyboard: keyboardCustom);
                                        return;
                                    }
                                    SendMessage(userId: message.PeerId, message: "Произошла ошибка в меню 30, что-то с message.Text", keyboardId: 0);
                                    return;
                                }
                            }
                        }
                        if (message.Payload.Contains("index"))
                        {
                            MessageKeyboard customKeyboard;
                            lock (Glob.lockerKeyboards)
                            {
                                customKeyboard = vkStuff.mainMenuKeyboards[4];
                            }
                            string payload = "{\"menu\": \"4\", \"index\": \"" + args[1] + "\", \"course\": \"" + args[2] + "\"}";
                            customKeyboard.Buttons.First().First().Action.Payload = payload;
                            customKeyboard.Buttons.First().ElementAt(1).Action.Payload = payload;
                            customKeyboard.Buttons.ElementAt(1).First().Action.Payload = payload;
                            SendMessage(
                                userId: message.PeerId,
                                message: "Выберите подгруппу, если нет - 1",
                                keyboardId: -1,
                                customKeyboard: customKeyboard);
                            return;
                        }
                        break;
                    }
                    default:
                    {
                        SendMessage(userId: message.PeerId, message: "Если Вы видите это сообщение, пожалуйста, напишите разработчику vk.com/id133040900");
                        return;
                    }
                }
                */
            });
            return;
        }
        
        public void SendMessage(long? userId,
                                // bool oneTime = false,
                                string message = "Отправляю клавиатуру",
                                List<MediaAttachment> attachments = null,
                                int? keyboardId = null,
                                string keyboardSpecial = "",
                                MessageKeyboard customKeyboard = null)
        {
            Random random = new Random();
            Int32 randomId;
            randomId = (Int32)((2 * random.NextDouble() - 1) * Int32.MaxValue);
            MessagesSendParams messagesSendParams = new MessagesSendParams()
            {
                PeerId = userId,
                Message = message,
                RandomId = randomId
            };
            switch (keyboardId)
            {
                case null:
                {
                    break;
                }
                case -1:
                {
                    messagesSendParams.Keyboard = customKeyboard;
                    break;
                }
                case 0:
                {
                    messagesSendParams.Keyboard = vkStuff.MainMenuKeyboards[0];
                    break;
                }
                case 1:
                {
                    messagesSendParams.Keyboard = vkStuff.MainMenuKeyboards[1];
                    messagesSendParams.Attachments = attachments;
                    break;
                }
                case 3:
                {
                    messagesSendParams.Keyboard = vkStuff.MainMenuKeyboards[3];
                    break;
                }
            }
            // if (oneTime)
            //     messagesSendParams.Keyboard.OneTime = true;
            vkStuff.commandsQueue.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
            Console.WriteLine(messagesSendParams.Message); // test
        }

        
        public async Task ExecuteMethodsAsync()
        {
            await Task.Run(async () => 
            {
                int queueCommandsAmount;
                int commandsInRequestAmount = 0;
                int timer = 0;
                StringBuilder stringBuilder = new StringBuilder();
                while (true)
                {
                    queueCommandsAmount = vkStuff.commandsQueue.Count();
                    // if (queueCommandsAmount > 25 - commandsInRequestAmount)
                    // {
                    //     queueCommandsAmount = 25 - commandsInRequestAmount;
                    // }
                    for (int i = 0; i < queueCommandsAmount; ++i)
                    {
                        if (vkStuff.commandsQueue.TryDequeue(out string command))
                        {
                            stringBuilder.Append(command);
                            ++commandsInRequestAmount;
                        }
                        else
                        {
                            --i;
                            timer += 1;
                            await Task.Delay(1);
                        }
                    }
                    if ((commandsInRequestAmount == 25 && timer >= 56) || timer >= 200)
                    {
                        if (commandsInRequestAmount == 0)
                        {
                            timer = 0;
                        }
                        else
                        {
                            vkStuff.api.Execute.Execute(stringBuilder.ToString());
                            timer = 0;
                            commandsInRequestAmount = 0;
                            stringBuilder = stringBuilder.Clear();
                        }
                    }
                    timer += 8;
                    await Task.Delay(8);
                }
            });
        }

        public async void CheckRelevanceAsync()
        {
            await Task.Run(async () => 
            {
                // SaveUsers();
                DatesAndUrls newDatesAndUrls = await checkRelevanceStuffITMM.CheckRelevance();
                if (newDatesAndUrls != null)
                {
                    for (int currentCourse = 0; currentCourse < 4; ++currentCourse)
                    {
                        if (newDatesAndUrls.dates[currentCourse] != courses[currentCourse].date)
                        {
                            courses[currentCourse].urlToFile = newDatesAndUrls.urls[currentCourse];
                            courses[currentCourse].date = newDatesAndUrls.dates[currentCourse];
                            UpdateProperties updateProperties = new UpdateProperties();
                            updateProperties.drawingStandartScheduleInfo = new Drawing.DrawingStandartScheduleInfo();
                            updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
                            updateProperties.photoUploadProperties.AlbumId = vkStuff.MainAlbumId;
                            courses[currentCourse].UpdateAsync(vkStuff.GroupUrl, updateProperties);
                        }
                    }
                }
            });
        }
        
        public async Task UploadPhotosAsync()
        {
            await Task.Run(async () => 
            {
                int queuePhotosAmount;
                int photosInRequestAmount = 0;
                int timer = 0;
                MultipartFormDataContent form = new MultipartFormDataContent();
                while (true)
                {
                    queuePhotosAmount = vkStuff.commandsQueue.Count();
                    if (queuePhotosAmount > 5 - photosInRequestAmount)
                    {
                        queuePhotosAmount = 5 - photosInRequestAmount;
                    }
                    for (int i = 0; i < queuePhotosAmount; ++i)
                    {
                        if (vkStuff.uploadPhotosQueue.TryDequeue(out PhotoUploadProperties photoUploadProperties))
                        {
                            form.Add(new ByteArrayContent(photoUploadProperties.Photo), "file" + i.ToString(), i.ToString() + ".png");
                            ++photosInRequestAmount;
                        }
                        else
                        {
                            --i;
                            timer += 1;
                            await Task.Delay(1);
                        }
                    }
                    if (photosInRequestAmount == 5 || timer >= 333)
                    {
                        if (photosInRequestAmount == 0)
                        {
                            timer = 0;
                        }
                        else
                        {
                            bool success = false;
                            HttpResponseMessage response;
                            while (!success)
                            {
                                try
                                {
                                    var uploadServer = vkStuff.apiPhotos.Photo.GetUploadServer(vkStuff.MainAlbumId, vkStuff.GroupId);
                                    response = null;
                                    response = (ScheduleBot.client.PostAsync(new Uri(uploadServer.UploadUrl), form)).Result;
                                    if (response != null)
                                    {
                                        IReadOnlyCollection<Photo> photos = vkStuff.apiPhotos.Photo.Save(new PhotoSaveParams
                                        {
                                            SaveFileResponse = Encoding.ASCII.GetString(response.Content.ReadAsByteArrayAsync().Result),
                                            AlbumId = vkStuff.MainAlbumId,
                                            GroupId = vkStuff.GroupId
                                        });
                                        if (photos.Count() == photosInRequestAmount)
                                        {
                                            // todo: сохранение id фоток
                                            // ! error empty photos_list nado testit' 
                                            // for (int i = 0; i < count; ++i)
                                            //     Glob.schedule_uploaded[photosToUpload[i].course, photosToUpload[i].index] = (ulong)photos.ElementAt(i).Id;
                                            success = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    await Task.Delay(1000);
                                }
                            }
                            timer = 0;
                            photosInRequestAmount = 0;
                            form.Dispose();
                        }
                    }
                    timer += 333;
                    await Task.Delay(333);
                }
            });
        }
        
        private List<int> AreScheduleRelevant(DatesAndUrls newDatesAndUrls)
        {
            List<int> notRelevantCourses = new List<int>();
            CoursesAmount = newDatesAndUrls.count;
            for (int i = 0; i < newDatesAndUrls.count; ++i)
            {
                if (newDatesAndUrls.dates[i] != null && courses[i].date != newDatesAndUrls.dates[i])
                {
                    notRelevantCourses.Add(i);
                }
            }
            return notRelevantCourses;
        }
    }
    





    public interface IDepartment
    {
        int CoursesAmount { get; set; }

        void CheckRelevanceAsync();

        Task GetMessagesAsync();

        Task UploadPhotosAsync();
    }
}
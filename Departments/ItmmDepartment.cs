using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        private Mapper mapper = new Mapper();
        private ICheckRelevanceStuff checkRelevanceStuffITMM = new CheckRelevanceStuffITMM();
        private int CoursesAmount { get; } = 4;
        private Course[] courses = new Course[4]; // 4 курса
        private UserRepository userRepository = new UserRepository();
        private Dictionaries dictionaries = new Dictionaries();
        private int startDay;
        private int startWeek;
        public ItmmDepartment(string _path)
        {
            path = _path + @"itmm\";
            vkStuff.MenuKeyboards = new MessageKeyboard[6]
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
                // настройки когда НЕ подписан
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Вы не подписаны",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
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
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                 // настройки когда подписан
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
                                    Label = "Переподписаться",
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
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            }
                        },
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
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "3",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "4",
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
                                    Payload = "{\"menu\": \"5\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "2",
                                    Payload = "{\"menu\": \"5\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"5\"}"
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
            for (int currentCourse = 0; currentCourse < 4; ++currentCourse)
                courses[currentCourse] = new Course(path + @"downloads\" + currentCourse + "_course.xls", dictionaries);
            LoadDatesAndUrls();
            mapper.CreateMaps(courses);
            ConstructKeyboards();
            LoadUploadedSchedule();
        }
        
        private static class ConstructKeyboardsProperties
        {
            public const int buttonsInLine = 2; // 1..4
            public const int linesInKeyboard = 4; // 1..9 
        }

        private void ConstructKeyboards()
        {
            for (int currentCourse = 0; currentCourse < CoursesAmount; ++currentCourse)
            {
                List<string> groupNames = mapper.GetGroupNames(currentCourse);
                int pagesAmount = (int)Math.Ceiling((double)groupNames.Count
                    / (double)(ConstructKeyboardsProperties.linesInKeyboard * ConstructKeyboardsProperties.buttonsInLine));
                int currentPage = 0;
                courses[currentCourse].keyboards = new List<MessageKeyboard>();
                List<MessageKeyboardButton> line = new List<MessageKeyboardButton>();
                List<List<MessageKeyboardButton>> buttons = new List<List<MessageKeyboardButton>>();
                List<MessageKeyboardButton> serviceLine = new List<MessageKeyboardButton>();
                for (int currentName = 0; currentName < groupNames.Count; currentName++)
                {
                    line.Add(new MessageKeyboardButton()
                    {
                        Color = KeyboardButtonColor.Primary,
                        Action = new MessageKeyboardButtonAction
                        {
                            Type = KeyboardButtonActionType.Text,
                            Label = groupNames[currentName],
                            Payload = "{\"menu\": \"40\", \"course\": \"" + currentCourse + "\"}"
                        }
                    });
                    if (line.Count == ConstructKeyboardsProperties.buttonsInLine
                        || (currentName + 1 == groupNames.Count && line.Count != 0))
                    {
                        buttons.Add(new List<MessageKeyboardButton>(line));
                        line.Clear();
                    }
                    if (buttons.Count == ConstructKeyboardsProperties.linesInKeyboard
                        || (currentName + 1 == groupNames.Count && buttons.Count != 0))
                    {
                        string payloadService = "{\"menu\": \"40\", \"page\": \"" + currentPage + "\", \"course\": \"" + currentCourse + "\"}";
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

        private string CurrentWeekStr() // Определение недели (верхняя или нижняя)
        {
            if (CurrentWeek() == 0)
            {
                return "Верхняя";
            }
            return "Нижняя";
        }

        private int CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            return ((DateTime.Now.DayOfYear - startDay) / 7 + startWeek) % 2;
        }
        
        private void LoadSettings()
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
                                vkStuff.GroupUrl = value;
                                break;
                            }
                            case "adminId":
                            {
                                vkStuff.AdminId = Int64.Parse(value);
                                break;
                            }
                        }
                    }
                }
            }
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка настроек");
        }

        private void LoadDatesAndUrls()
        {
            using (StreamReader file = new StreamReader(
                path + "datesAndUrls.txt",
                System.Text.Encoding.Default))
            {
                for (int currentCourse = 0; currentCourse < 4; currentCourse++)
                {
                    string str = file.ReadLine();
                    courses[currentCourse].urlToFile = str.Substring(0, str.IndexOf(' '));
                    courses[currentCourse].date = str.Substring(str.IndexOf(' ') + 1);
                }
            }
        }

        private void SaveDatesAndUrls()
        {
            using (StreamWriter file = new StreamWriter(path + "datesAndUrls.txt"))
            {
                for (int currentCourse = 0; currentCourse < 4; currentCourse++)
                {
                    file.WriteLine(courses[currentCourse].urlToFile + " " + courses[currentCourse].date);
                }
            }
        }

        private void LoadAcronymToPhrase()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка ManualAcronymToPhrase");
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/acronymToPhrase.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                dictionaries.acronymToPhrase.Add(file.ReadLine(), file.ReadLine());
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка ManualAcronymToPhrase");
        }

        private void LoadDoubleOptionallySubject()
        {
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка DoubleOptionallySubject");
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/doubleOptionallySubject.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                dictionaries.doubleOptionallySubject.Add(file.ReadLine(), file.ReadLine());
            // Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка DoubleOptionallySubject");
        }

        private void LoadFullName()
        {
            using StreamReader file = new StreamReader(
                path + @"/manualProcessing/fullName.txt",
                System.Text.Encoding.Default);
            while (!file.EndOfStream)
                dictionaries.fullName.Add(file.ReadLine());
        }

        private void LoadUploadedSchedule()
        {
            using (StreamReader file = new StreamReader(
                path + "uploadedSchedule.txt",
                System.Text.Encoding.Default))
            {
                string rawLine;
                while (!file.EndOfStream)
                {
                    rawLine = file.ReadLine();
                    var rawSpan = rawLine.AsSpan();

                    int spaceIndex = rawSpan.IndexOf(' ');
                    int lastSpaceIndex = rawSpan.LastIndexOf(' ');

                    long id = long.Parse(rawSpan.Slice(0, spaceIndex));
                    string group = rawSpan.Slice(spaceIndex + 1, lastSpaceIndex - spaceIndex - 1).ToString();
                    int subgroup = int.Parse(rawSpan.Slice(lastSpaceIndex + 1, 1));

                    for (int currentCourse = 0; currentCourse < CoursesAmount; currentCourse++)
                    {
                        int groupsAmount = courses[currentCourse].groups.Count;
                        for (int currentGroup = 0; currentGroup < groupsAmount; currentGroup++)
                        {
                            if (courses[currentCourse].groups[currentGroup].name == group)
                            {
                                courses[currentCourse].groups[currentGroup].scheduleSubgroups[subgroup - 1].PhotoId = id;
                                currentCourse = CoursesAmount;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void SaveUploadedSchedule()
        {
            using (StreamWriter file = new StreamWriter(path + "uploadedSchedule.txt"))
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int currentCourse = 0; currentCourse < CoursesAmount; currentCourse++)
                {
                    int groupsAmount = courses[currentCourse].groups.Count;
                    for (int currentGroup = 0; currentGroup < groupsAmount; currentGroup++)
                    {
                        stringBuilder.Append(courses[currentCourse].groups[currentGroup].scheduleSubgroups[0].PhotoId);
                        stringBuilder.Append(' ');
                        stringBuilder.Append(courses[currentCourse].groups[currentGroup].name);
                        stringBuilder.Append(" 1\n");
                        stringBuilder.Append(courses[currentCourse].groups[currentGroup].scheduleSubgroups[1].PhotoId);
                        stringBuilder.Append(' ');
                        stringBuilder.Append(courses[currentCourse].groups[currentGroup].name);
                        stringBuilder.Append(" 2\n");
                    }
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                file.WriteLine(stringBuilder.ToString());
            }
        }

        private void LoadUsers()
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

        private async void SaveUsers()
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
                    Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " Получаю сообщения");
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
                            Console.WriteLine(update.Message.Text);
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
                    catch
                    {
                        LongPollServerResponse server = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
                        botsLongPollHistoryParams.Ts = server.Ts;
                        botsLongPollHistoryParams.Key = server.Key;
                        botsLongPollHistoryParams.Server = server.Server;
                    }
                }
            });
        }

        public async void MessageResponseAsync(Message message)
        {
            await Task.Run(() =>
            {
                if (message.Payload == null)
                {
                    // todo: UPdate переписать
                    if (message.PeerId == vkStuff.AdminId)
                    {
                        if (message.Text.IndexOf("Помощь") == 0 || message.Text.IndexOf("Help") == 0)
                        {
                            string help = "Команды:\n\nРассылка <всем,*КУРС*,*ГРУППА*>\n--отправляет расписание на неделю выбранным юзерам\nОбновить <все,*КУРС*> [нет]\n--обновляет расписание для выбранных курсов, отправлять ли обновление юзерам (по умолчанию - да)\nПерезагрузка\n--перезагружает бота(для применения обновления версии бота)\n\nCommands:\n\nDistribution <all,*COURSE*,*GROUP*>\n--отправляет расписание на неделю выбранным юзерам\nUpdate <all,*COURSE*> [false]\n--обновляет расписание для выбранных курсов, отправлять ли обновление юзерам (по умолчанию - да)\nReboot\n--перезагружает бота(для применения обновления версии бота)\n";
                            EnqueueMessageAsync(userId: message.PeerId, message: help);
                        }
                        else if (message.Text.IndexOf("Рассылка") == 0 || message.Text.IndexOf("Distribution") == 0)
                        {
                            string temp = message.Text.Substring(message.Text.IndexOf(' ') + 1);
                            string toWhom = temp.Substring(0, temp.IndexOf(' '));
                            string messageStr = temp.Substring(temp.IndexOf(' ') + 1); // сообщение
                            if (toWhom == "всем" || toWhom == "all")
                            {
                                EnqueueMessageAsync(
                                    userIds: userRepository.GetIds(),
                                    message: messageStr);
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Выполнено");
                            }
                            else if (toWhom.Length == 1)
                            {
                                int toCourse = 0;
                                int.TryParse(toWhom, out toCourse);
                                --toCourse;
                                if (toCourse != -1 && toCourse >= 0 && toCourse < 4)
                                {
                                    EnqueueMessageAsync(
                                        userIds: userRepository.GetIds(toCourse, mapper),
                                        message: messageStr);
                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Выполнено");
                                }
                                else
                                {
                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Ошибка рассылки:\nневерный курс: " + toWhom + "\nВведите значение от 1 до 4");
                                }
                            }
                            else
                            {
                                EnqueueMessageAsync(
                                    userIds: userRepository.GetIds(toWhom),
                                    message: messageStr);
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Выполнено");
                            }
                        }
                        else if (message.Text.IndexOf("Обновить") == 0 || message.Text.IndexOf("Update") == 0)
                        {
                            /*
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
                            SendMessageAsync(userId: message.PeerId, message: "Выполнено");
                            */
                        }
                        else if (message.Text.IndexOf("Перезагрузка") == 0 || message.Text.IndexOf("Reboot") == 0)
                        {
                            while (courses[0].isUpdating || courses[1].isUpdating || courses[2].isUpdating || courses[3].isUpdating)
                                Thread.Sleep(60000);
                            while (!vkStuff.commandsQueue.IsEmpty && !vkStuff.photosQueue.IsEmpty)
                                Thread.Sleep(5000); 
                            SaveUsers();                         
                            Environment.Exit(0);
                        }
                    }
                    else if (message.Attachments.Count != 0)
                    {
                        // todo: fix
                        if (message.Attachments.Single().ToString() == "Sticker")
                        {
                            
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "🤡");
                            return;
                        }
                    }
                    else
                    {
                        EnqueueMessageAsync(
                            userId: message.PeerId,
                            message: "Нажмите на кнопку",
                            keyboardId: 0);
                        return;
                    }
                    return;
                }
                PayloadStuff payloadStuff = Newtonsoft.Json.JsonConvert.DeserializeObject<PayloadStuff>(message.Payload);
                if (payloadStuff.Command == "start")
                {
                    EnqueueMessageAsync(
                        userId: message.PeerId,
                        message: "Здравствуйтe, я буду присылать актуальное расписание, если Вы подпишитесь в настройках.\nКнопка \"Информация\" для получения подробностей",
                        keyboardId: 0);
                    return;
                }
                // По idшникам меню сортируем сообщения
                switch (payloadStuff.Menu)
                {
                    case null:
                    {
                        EnqueueMessageAsync(
                            userId: message.PeerId,
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
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    keyboardId: 1);
                                return;
                            }
                            case "Неделя":
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: CurrentWeekStr());
                                return;
                            }
                            case "Настройки":
                            {
                                if (userRepository.ContainsUser(message.PeerId))
                                {
                                    User user = userRepository.GetUser(message.PeerId);

                                    StringBuilder stringBuilder = new StringBuilder();
                                    stringBuilder.Append("Вы подписаны: ");
                                    stringBuilder.Append(user.Group);
                                    stringBuilder.Append(" (");
                                    stringBuilder.Append(user.Subgroup);
                                    stringBuilder.Append(')');

                                    MessageKeyboard keyboardCustom = vkStuff.MenuKeyboards[3];
                                    keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();
                                    
                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Отправляю клавиатуру",
                                        customKeyboard: keyboardCustom);
                                }
                                else
                                {
                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Отправляю клавиатуру",
                                        keyboardId: 2);
                                }
                                return;
                            }
                            case "Информация":
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Текущая версия - v2.2\n\nПри обновлении расписания на сайте Вам придёт сообщение. Далее Вы получите одно из трех сообщений:\n 1) Новое расписание *картинка*\n 2) Для Вас изменений нет\n 3) Не удалось скачать/обработать расписание *ссылка*\n Если не придёт никакого сообщения, Ваша группа скорее всего изменилась/не найдена. Настройте заново.\n\nВ расписании могут встретиться верхние индексы, предупреждающие о возможных ошибках. Советую ознакомиться со статьёй: vk.com/@itmmschedulebot-raspisanie");
                                return;
                            }
                            default:
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Произошла ошибка в меню 0, что-то с message.Text",
                                    keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 1:
                    {
                        if (message.Text == "Назад")
                        {
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                keyboardId: 0);
                            return;
                        }
                        else
                        {
                            User user = null;
                            (int?, int) userMapping = default((int?, int));
                            if (userRepository.ContainsUser(message.PeerId))
                            {
                                user = userRepository.GetUser(message.PeerId);
                                userMapping = mapper.GetCourseAndIndex(user.Group);
                            }
                            if (user != null)
                            {
                                if (userMapping.Item1 != null)
                                {
                                    if (courses[(int)userMapping.Item1].isUpdating)
                                    {
                                        EnqueueMessageAsync(
                                            userId: message.PeerId,
                                            message: "Происходит обновление расписания, повторите попытку через несколько минут");
                                        return;
                                    }
                                    else if (message.Text == "Ссылка")
                                    {
                                        StringBuilder stringBuilder = new StringBuilder();
                                        stringBuilder.Append("Расписание для ");
                                        stringBuilder.Append(userMapping.Item1 + 1);
                                        stringBuilder.Append(" курса: ");
                                        stringBuilder.Append(courses[(int)userMapping.Item1].urlToFile);

                                        EnqueueMessageAsync(
                                            userId: message.PeerId,
                                            message: stringBuilder.ToString());
                                        return;
                                    }
                                    else if (courses[(int)userMapping.Item1].isBroken)
                                    {
                                        EnqueueMessageAsync(
                                            userId: message.PeerId,
                                            message: "Расписание Вашего курса не обработано");
                                        return;
                                    }
                                    else
                                    {
                                        switch (message.Text)
                                        {
                                            case "На неделю":
                                            {
                                                if (courses[(int)userMapping.Item1].isBroken)
                                                {
                                                    EnqueueMessageAsync(
                                                        userId: message.PeerId,
                                                        message: "Расписание Вашего курса не обработано",
                                                        keyboardId: 1);
                                                    return;
                                                }
                                                else
                                                {
                                                    StringBuilder stringBuilder = new StringBuilder();
                                                    stringBuilder.Append("Расписание для ");
                                                    stringBuilder.Append(user.Group);
                                                    stringBuilder.Append(" (");
                                                    stringBuilder.Append(user.Subgroup);
                                                    stringBuilder.Append(')');    

                                                    EnqueueMessageAsync(
                                                        userId: message.PeerId,
                                                        message: stringBuilder.ToString(),
                                                        attachments: new List<MediaAttachment>
                                                        {
                                                            new Photo()
                                                            {
                                                                AlbumId = vkStuff.MainAlbumId,
                                                                OwnerId = -vkStuff.GroupId,
                                                                Id = courses[(int)userMapping.Item1].groups[userMapping.Item2].scheduleSubgroups[user.Subgroup - 1].PhotoId
                                                            }
                                                        });
                                                    return;
                                                }
                                            }
                                            case "На сегодня":
                                            {
                                                int week = CurrentWeek();
                                                int today = (int)DateTime.Now.DayOfWeek;
                                                if (today == 0)
                                                {
                                                    EnqueueMessageAsync(
                                                        userId: message.PeerId,
                                                        message: "Сегодня воскресенье");
                                                    return;
                                                }
                                                else
                                                {
                                                    --today;
                                                    if (courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                        .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                                                        .days[today].isStudying)
                                                    {
                                                        long photoId = courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                            .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                                                            .days[today].PhotoId;
                                                        if (photoId == 0)
                                                        {
                                                            Drawing.DrawingDayScheduleInfo drawingDayScheduleInfo = new Drawing.DrawingDayScheduleInfo();
                                                            drawingDayScheduleInfo.date = courses[(int)userMapping.Item1].date;
                                                            drawingDayScheduleInfo.day = courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                                .scheduleSubgroups[user.Subgroup - 1].weeks[week].days[today];
                                                            drawingDayScheduleInfo.dayOfWeek = today;
                                                            drawingDayScheduleInfo.group = user.Group;
                                                            drawingDayScheduleInfo.subgroup = user.Subgroup.ToString();
                                                            drawingDayScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
                                                            drawingDayScheduleInfo.weekProperties = week;

                                                            PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
                                                            photoUploadProperties.AlbumId = vkStuff.MainAlbumId;
                                                            photoUploadProperties.Day = today;
                                                            photoUploadProperties.Group = user.Group;
                                                            photoUploadProperties.Subgroup = user.Subgroup - 1;
                                                            photoUploadProperties.Week = week;
                                                            photoUploadProperties.PeerId = (long)message.PeerId;
                                                            photoUploadProperties.Message = "Расписание на сегодня";
                                                            photoUploadProperties.Photo = Drawing.DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                                                        
                                                            vkStuff.photosQueue.Enqueue(photoUploadProperties);
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            EnqueueMessageAsync(
                                                                userId: message.PeerId,
                                                                message: "Расписание на сегодня",
                                                                attachments: new List<MediaAttachment>
                                                                {
                                                                    new Photo()
                                                                    {
                                                                        AlbumId = vkStuff.MainAlbumId,
                                                                        OwnerId = -vkStuff.GroupId,
                                                                        Id = photoId
                                                                    }
                                                                });
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        EnqueueMessageAsync(
                                                            userId: message.PeerId,
                                                            message: "Сегодня Вы не учитесь");
                                                        return;
                                                    }
                                                }
                                            }
                                            case "На завтра":
                                            {
                                                int week = CurrentWeek();
                                                int today = (int)DateTime.Now.DayOfWeek;
                                                if (today == 6)
                                                {
                                                    week = (week + 1) % 2;
                                                    int day = 0;
                                                    while (!courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                        .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                                                        .days[day].isStudying)
                                                    {
                                                        ++day;
                                                        if (day == 6)
                                                        {
                                                            day = 0;
                                                            week = (week + 1) % 2;
                                                        }
                                                    }
                                                    long photoId = courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                        .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                                                        .days[day].PhotoId;
                                                    if (photoId == 0)
                                                    {
                                                        Drawing.DrawingDayScheduleInfo drawingDayScheduleInfo = new Drawing.DrawingDayScheduleInfo();
                                                        drawingDayScheduleInfo.date = courses[(int)userMapping.Item1].date;
                                                        drawingDayScheduleInfo.day = courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                            .scheduleSubgroups[user.Subgroup - 1].weeks[week].days[day];
                                                        drawingDayScheduleInfo.dayOfWeek = day;
                                                        drawingDayScheduleInfo.group = user.Group;
                                                        drawingDayScheduleInfo.subgroup = user.Subgroup.ToString();
                                                        drawingDayScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
                                                        drawingDayScheduleInfo.weekProperties = week;

                                                        PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
                                                        photoUploadProperties.AlbumId = vkStuff.MainAlbumId;
                                                        photoUploadProperties.Day = day;
                                                        photoUploadProperties.Group = user.Group;
                                                        photoUploadProperties.Subgroup = user.Subgroup - 1;
                                                        photoUploadProperties.Week = week;
                                                        photoUploadProperties.PeerId = (long)message.PeerId;
                                                        photoUploadProperties.Message = "Завтра воскресенье, вот расписание на ближайший учебный день";
                                                        photoUploadProperties.Photo = Drawing.DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                                                    
                                                        vkStuff.photosQueue.Enqueue(photoUploadProperties);
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        EnqueueMessageAsync(
                                                            userId: message.PeerId,
                                                            message: "Завтра воскресенье, вот расписание на ближайший учебный день",
                                                            attachments: new List<MediaAttachment>
                                                            {
                                                                new Photo()
                                                                {
                                                                    AlbumId = vkStuff.MainAlbumId,
                                                                    OwnerId = -vkStuff.GroupId,
                                                                    Id = photoId
                                                                }
                                                            });
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    // в связи с тем, что DateTime.Now.DayOfWeek == 0 это воскресенье
                                                    int day = today;
                                                    if (today == 0)
                                                        week = (week + 1) % 2;
                                                    int weekTemp = week;
                                                    while (!courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                        .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                                                        .days[day].isStudying)
                                                    {
                                                        ++day;
                                                        if (day == 6)
                                                        {
                                                            day = 0;
                                                            week = (week + 1) % 2;
                                                        }
                                                    }
                                                    string messageTemp = "Завтра Вы не учитесь, вот расписание на ближайший учебный день";
                                                    if (day == today && week == weekTemp)
                                                        messageTemp = "Расписание на завтра";
                                                    long photoId = courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                        .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                                                        .days[day].PhotoId;
                                                    if (photoId == 0)
                                                    {
                                                        Drawing.DrawingDayScheduleInfo drawingDayScheduleInfo = new Drawing.DrawingDayScheduleInfo();
                                                        drawingDayScheduleInfo.date = courses[(int)userMapping.Item1].date;
                                                        drawingDayScheduleInfo.day = courses[(int)userMapping.Item1].groups[userMapping.Item2]
                                                            .scheduleSubgroups[user.Subgroup - 1].weeks[week].days[day];
                                                        drawingDayScheduleInfo.dayOfWeek = day;
                                                        drawingDayScheduleInfo.group = user.Group;
                                                        drawingDayScheduleInfo.subgroup = user.Subgroup.ToString();
                                                        drawingDayScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
                                                        drawingDayScheduleInfo.weekProperties = week;

                                                        PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
                                                        photoUploadProperties.AlbumId = vkStuff.MainAlbumId;
                                                        photoUploadProperties.Day = day;
                                                        photoUploadProperties.Group = user.Group;
                                                        photoUploadProperties.Subgroup = user.Subgroup - 1;
                                                        photoUploadProperties.Week = week;
                                                        photoUploadProperties.PeerId = (long)message.PeerId;
                                                        photoUploadProperties.Message = messageTemp;
                                                        photoUploadProperties.Photo = Drawing.DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                                                    
                                                        vkStuff.photosQueue.Enqueue(photoUploadProperties);
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        EnqueueMessageAsync(
                                                            userId: message.PeerId,
                                                            message: messageTemp,
                                                            attachments: new List<MediaAttachment>
                                                            {
                                                                new Photo()
                                                                {
                                                                    AlbumId = vkStuff.MainAlbumId,
                                                                    OwnerId = -vkStuff.GroupId,
                                                                    Id = photoId
                                                                }
                                                            });
                                                        return;
                                                    }
                                                }
                                            }
                                            default:
                                            {
                                                EnqueueMessageAsync(
                                                    userId: message.PeerId,
                                                    message: "Произошла ошибка в меню 1, что-то с message.Text",
                                                    keyboardId: 0);
                                                return;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MessageKeyboard keyboardCustom = vkStuff.MenuKeyboards[3];

                                    StringBuilder stringBuilder = new StringBuilder();
                                    stringBuilder.Append("Вы подписаны: ");
                                    stringBuilder.Append(user.Group);
                                    stringBuilder.Append(" (");
                                    stringBuilder.Append(user.Subgroup);
                                    stringBuilder.Append(")");

                                    keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();

                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Ваша группа не существует, настройте заново",
                                        customKeyboard: keyboardCustom);
                                    return;
                                }
                            }
                            else
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                                    keyboardId: 2);
                                return;
                            }
                        }
                    }
                    case 2: // 2 и 3 тут
                    {
                        if (message.Text.Contains("Вы подписаны") || message.Text.Contains("Вы не подписаны"))
                        {
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "Попробуйте нажать на другую кнопку");
                            return;
                        }
                        switch (message.Text)
                        {
                            case "Отписаться":
                            {
                                User user = userRepository.GetUser(message.PeerId);

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("Отменена подписка на ");
                                stringBuilder.Append(user.Group);
                                stringBuilder.Append(" (");
                                stringBuilder.Append(user.Subgroup);
                                stringBuilder.Append(')');

                                userRepository.DeleteUser((long)message.PeerId);

                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: stringBuilder.ToString(),
                                    keyboardId: 2);
                                return;
                            }
                            case "Подписаться":
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    keyboardId: 4);
                                return;
                            }
                            case "Переподписаться":
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    keyboardId: 4);
                                return;
                            }
                            case "Изменить подгруппу":
                            {
                                User user = userRepository.ChangeSubgroup(message.PeerId);

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("Вы подписаны: ");
                                stringBuilder.Append(user.Group);
                                stringBuilder.Append(" (");
                                stringBuilder.Append(user.Subgroup);
                                stringBuilder.Append(')');

                                MessageKeyboard keyboardCustom;
                                keyboardCustom = vkStuff.MenuKeyboards[3];
                                keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();
                            
                                stringBuilder.Clear();
                                stringBuilder.Append("Ваша подгруппа: ");
                                stringBuilder.Append(user.Subgroup);

                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: stringBuilder.ToString(),
                                    customKeyboard: keyboardCustom);
                                return;
                            }
                            case "Назад":
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    keyboardId: 0);
                                return;
                            }
                            default:
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Произошла ошибка в меню 2, что-то с message.Text",
                                    keyboardId: 0);
                                return;
                            }
                        }
                    }
                    case 4:
                    {
                        if (message.Text == "Выберите курс")
                        {
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "Попробуйте нажать на другую кнопку");
                            return;
                        }
                        else if (message.Text == "Назад")
                        {
                            if (userRepository.ContainsUser(message.PeerId))
                            {
                                User user = userRepository.GetUser(message.PeerId);

                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("Вы подписаны: ");
                                stringBuilder.Append(user.Group);
                                stringBuilder.Append(" (");
                                stringBuilder.Append(user.Subgroup);
                                stringBuilder.Append(')');

                                MessageKeyboard keyboardCustom = vkStuff.MenuKeyboards[3];
                                keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();
                                
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    customKeyboard: keyboardCustom);
                            }
                            else
                            {
                                EnqueueMessageAsync(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: 2);
                            }
                            return;
                        }
                        else if (message.Text.Length == 1)
                        {
                            Int32.TryParse(message.Text, out int course);
                            course--;
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "Выберите группу",
                                customKeyboard: courses[course].keyboards[0]);
                            return;
                        }
                        else
                        {
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "Произошла ошибка в меню 4, что-то с message.Text", 
                                keyboardId: 0);
                            return;
                        }
                    }
                    case 5:
                    {
                        if (message.Text == "Назад")
                        {
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                customKeyboard: courses[payloadStuff.Course].keyboards[0]);
                            return;
                        }
                        else if (message.Text.Length == 1)
                        {
                            StringBuilder stringBuilder = new StringBuilder();

                            Int32.TryParse(message.Text, out int subgroup);
                            if (userRepository.ContainsUser(message.PeerId))
                            {
                                userRepository.EditUser(
                                    id: (long)message.PeerId,
                                    newGroup: payloadStuff.Group,
                                    newSubgroup: subgroup);

                                stringBuilder.Append("Вы изменили настройки на ");
                            }
                            else
                            {
                                userRepository.AddUser(new User((long)message.PeerId, payloadStuff.Group, subgroup));

                                stringBuilder.Append("Вы подписались на ");
                            }

                            stringBuilder.Append(payloadStuff.Group);
                            stringBuilder.Append(" (");
                            stringBuilder.Append(message.Text);
                            stringBuilder.Append(')');

                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: stringBuilder.ToString(),
                                keyboardId: 0);
                            return;
                        }
                        else
                        {
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "Произошла ошибка в меню 5, что-то с message.Text",
                                keyboardId: 0);
                            return;
                        }
                    }
                    case 40:
                    {
                        if (payloadStuff.Page != -1)
                        {
                            switch (message.Text)
                            {
                                case "Назад":
                                {
                                    if (payloadStuff.Page == 0)
                                    {
                                        EnqueueMessageAsync(
                                            userId: message.PeerId,
                                            message: "Отправляю клавиатуру",
                                            keyboardId: 4);
                                        return;
                                    }
                                    else
                                    {
                                        EnqueueMessageAsync(
                                            userId: message.PeerId,
                                            message: "Отправляю клавиатуру",
                                            customKeyboard: courses[payloadStuff.Course].keyboards[payloadStuff.Page - 1]);
                                        return;
                                    }
                                }
                                case "Вперед":
                                {
                                    MessageKeyboard keyboardCustom;
                                    if (payloadStuff.Page == courses[payloadStuff.Course].keyboards.Count - 1)
                                    {
                                        keyboardCustom = courses[payloadStuff.Course].keyboards[0];
                                    }
                                    else
                                    {
                                        keyboardCustom = courses[payloadStuff.Course].keyboards[payloadStuff.Page + 1];
                                    }
                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Отправляю клавиатуру",
                                        customKeyboard: keyboardCustom);
                                    return;
                                }
                                default:
                                {
                                    if (message.Text.Contains(" из "))
                                    {
                                        EnqueueMessageAsync(
                                            userId: message.PeerId,
                                            message: "Меню страниц не реализовано");
                                        return;
                                    }
                                    EnqueueMessageAsync(
                                        userId: message.PeerId,
                                        message: "Произошла ошибка в меню 40, что-то с message.Text",
                                        keyboardId: 0);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            MessageKeyboard customKeyboard;
                            customKeyboard = vkStuff.MenuKeyboards[5];
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("{\"menu\": \"5\", \"group\": \"");
                            stringBuilder.Append(message.Text);
                            stringBuilder.Append("\", \"course\": \"");
                            stringBuilder.Append(payloadStuff.Course);
                            stringBuilder.Append("\"}");
                            customKeyboard.Buttons.First().First().Action.Payload = stringBuilder.ToString();
                            customKeyboard.Buttons.First().ElementAt(1).Action.Payload = customKeyboard.Buttons.First().First().Action.Payload;
                            customKeyboard.Buttons.ElementAt(1).First().Action.Payload = customKeyboard.Buttons.First().First().Action.Payload;
                            EnqueueMessageAsync(
                                userId: message.PeerId,
                                message: "Выберите подгруппу, если нет - 1",
                                customKeyboard: customKeyboard);
                            return;
                        }
                    }
                }
            });
            return;
        }

        public async void EnqueueMessageAsync(
            long? userId = null,
            List<long> userIds = null,
            string message = "Отправляю клавиатуру",
            List<MediaAttachment> attachments = null,
            int? keyboardId = null,
            MessageKeyboard customKeyboard = null)
        {
            await Task.Run(() => 
            {
                MessagesSendParams messageSendParams = new MessagesSendParams()
                {
                    Message = message,
                    RandomId = (int)DateTime.Now.Ticks
                };
                if (userId == null)
                {
                    if (userIds.Count == 0)
                        return;
                    else if (userIds.Count > 100)
                    {
                        messageSendParams.UserIds = userIds.GetRange(0, 100);
                        userIds.RemoveRange(0, 100);
                        EnqueueMessageAsync(
                            userIds: userIds,
                            message: message,
                            attachments: attachments,
                            keyboardId: keyboardId,
                            customKeyboard: customKeyboard);
                    }
                    else
                        messageSendParams.UserIds = userIds;
                }
                else
                    messageSendParams.PeerId = userId;
                if (attachments != null)
                    messageSendParams.Attachments = attachments;
                if (customKeyboard != null)
                {
                    messageSendParams.Keyboard = customKeyboard;
                }
                else
                {
                    switch (keyboardId)
                    {
                        case null:
                        {
                            break;
                        }
                        case 0:
                        {
                            messageSendParams.Keyboard = vkStuff.MenuKeyboards[0];
                            break;
                        }
                        case 1:
                        {
                            messageSendParams.Keyboard = vkStuff.MenuKeyboards[1];
                            break;
                        }
                        case 2:
                        {
                            messageSendParams.Keyboard = vkStuff.MenuKeyboards[2];
                            break;
                        }
                        case 4:
                        {
                            messageSendParams.Keyboard = vkStuff.MenuKeyboards[4];
                            break;
                        }
                    }
                }
                vkStuff.commandsQueue.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messageSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
            });
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
                    queueCommandsAmount = vkStuff.commandsQueue.Count;
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
                            Console.WriteLine(stringBuilder.ToString());
                            var response = vkStuff.api.Execute.Execute(stringBuilder.ToString());
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
                DatesAndUrls newDatesAndUrls = await checkRelevanceStuffITMM.CheckRelevanceAsync();
                if (newDatesAndUrls != null)
                {
                    List<int> updatingCourses = new List<int>();
                    List<Schedulebot.Vk.PhotoUploadProperties> photosUploadProperties = new List<PhotoUploadProperties>();
                    for (int currentDateAndUrl = 0; currentDateAndUrl < newDatesAndUrls.count; ++currentDateAndUrl)
                    {
                        if (newDatesAndUrls.dates[currentDateAndUrl] != courses[newDatesAndUrls.courses[currentDateAndUrl]].date)
                        {
                            courses[newDatesAndUrls.courses[currentDateAndUrl]].urlToFile = newDatesAndUrls.urls[currentDateAndUrl];
                            courses[newDatesAndUrls.courses[currentDateAndUrl]].date = newDatesAndUrls.dates[currentDateAndUrl];
                            
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("Вышло новое расписание ");
                            stringBuilder.Append(newDatesAndUrls.dates[currentDateAndUrl]);
                            stringBuilder.Append(". Ожидайте результата обработки.");

                            EnqueueMessageAsync(
                                userIds: userRepository.GetIds(newDatesAndUrls.courses[currentDateAndUrl], mapper),
                                message: stringBuilder.ToString());

                            UpdateProperties updateProperties = new UpdateProperties();
                            updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
                            updateProperties.photoUploadProperties.AlbumId = vkStuff.MainAlbumId;

                            var tempPhotosList = await courses[newDatesAndUrls.courses[currentDateAndUrl]].UpdateAsync(updateProperties, dictionaries);
                            if (tempPhotosList != null)
                            {
                                photosUploadProperties.AddRange(tempPhotosList);
                                updatingCourses.Add(newDatesAndUrls.courses[currentDateAndUrl]);
                            }
                        }
                    }
                    SaveDatesAndUrls();
                    if (updatingCourses.Count != 0)
                    {
                        mapper.CreateMaps(courses);
                        ConstructKeyboards();

                        for (int i = 0; i < photosUploadProperties.Count; i++)
                            vkStuff.photosQueue.Enqueue(photosUploadProperties[i]);

                        List<(string, int)> newGroupSubgroupList = new List<(string, int)>();
                        for (int currentPhoto = 0; currentPhoto < photosUploadProperties.Count; currentPhoto++)
                            newGroupSubgroupList.Add((photosUploadProperties[currentPhoto].Group, photosUploadProperties[currentPhoto].Subgroup));

                        EnqueueMessageAsync(
                            message: "Для Вас изменений нет",
                            userIds: userRepository.GetIds(mapper.GetOldGroupSubgroupList(newGroupSubgroupList, updatingCourses)));
                        
                        while (true)
                        {
                            if (vkStuff.photosQueue.IsEmpty)
                            {
                                for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
                                    courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;
                                break;
                            }
                            await Task.Delay(2000);
                        }

                        SaveUploadedSchedule();

                        for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
                        {
                            if (courses[updatingCourses[currentUpdatingCourse]].isBroken)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("Не удалось обработать расписание ");
                                stringBuilder.Append(courses[updatingCourses[currentUpdatingCourse]].date);
                                stringBuilder.Append(". Ссылка: ");
                                stringBuilder.Append(courses[updatingCourses[currentUpdatingCourse]].urlToFile);

                                EnqueueMessageAsync(
                                    userIds: userRepository.GetIds(updatingCourses[currentUpdatingCourse], mapper),
                                    message: stringBuilder.ToString());
                            }
                            courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;
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
                List<PhotoUploadProperties> photosUploadProperties = new List<PhotoUploadProperties>();
                while (true)
                {
                    queuePhotosAmount = vkStuff.photosQueue.Count;
                    if (queuePhotosAmount > 5 - photosInRequestAmount)
                    {
                        queuePhotosAmount = 5 - photosInRequestAmount;
                    }
                    for (int i = 0; i < queuePhotosAmount; ++i)
                    {
                        if (vkStuff.photosQueue.TryDequeue(out PhotoUploadProperties photoUploadProperties))
                        {
                            photosUploadProperties.Add(photoUploadProperties);
                            form.Add(new ByteArrayContent(photosUploadProperties[i].Photo), "file" + i.ToString(), i.ToString() + ".png");
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
                                    response = await ScheduleBot.client.PostAsync(new Uri(uploadServer.UploadUrl), form);
                                    if (response != null)
                                    {
                                        IReadOnlyCollection<Photo> photos = vkStuff.apiPhotos.Photo.Save(new PhotoSaveParams
                                        {
                                            SaveFileResponse = Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync()),
                                            AlbumId = vkStuff.MainAlbumId,
                                            GroupId = vkStuff.GroupId
                                        });
                                        if (photos.Count == photosInRequestAmount)
                                        {
                                            for (int currentPhoto = 0; currentPhoto < photosInRequestAmount; currentPhoto++)
                                            {
                                                (int?, int) CourseIndexAndGroupIndex = mapper.GetCourseAndIndex(photosUploadProperties[currentPhoto].Group);
                                                if (photosUploadProperties[currentPhoto].Week == -1)
                                                {
                                                    // на неделю
                                                    courses[(int)CourseIndexAndGroupIndex.Item1].groups[CourseIndexAndGroupIndex.Item2].scheduleSubgroups[photosUploadProperties[currentPhoto].Subgroup].PhotoId
                                                        = (long)photos.ElementAt(currentPhoto).Id;
                                                    List<long> ids = userRepository.GetIds((int)CourseIndexAndGroupIndex.Item1, mapper);
                                                    EnqueueMessageAsync(
                                                        userIds: ids,
                                                        message: photosUploadProperties[currentPhoto].Message,
                                                        attachments: new List<MediaAttachment>
                                                        {
                                                            new Photo()
                                                            {
                                                                AlbumId = photosUploadProperties[currentPhoto].AlbumId,
                                                                OwnerId = -vkStuff.GroupId,
                                                                Id = photos.ElementAt(currentPhoto).Id
                                                            }
                                                        });
                                                }
                                                else
                                                {
                                                    courses[(int)CourseIndexAndGroupIndex.Item1]
                                                        .groups[CourseIndexAndGroupIndex.Item2]
                                                        .scheduleSubgroups[photosUploadProperties[currentPhoto].Subgroup]
                                                        .weeks[photosUploadProperties[currentPhoto].Week]
                                                        .days[photosUploadProperties[currentPhoto].Day]
                                                        .PhotoId
                                                        = (long)photos.ElementAt(currentPhoto).Id;
                                                    if (photosUploadProperties[currentPhoto].PeerId != 0)
                                                    {
                                                        EnqueueMessageAsync(
                                                            userId: photosUploadProperties[currentPhoto].PeerId,
                                                            message: photosUploadProperties[currentPhoto].Message,
                                                            attachments: new List<MediaAttachment>
                                                            {
                                                                new Photo()
                                                                {
                                                                    AlbumId = photosUploadProperties[currentPhoto].AlbumId,
                                                                    OwnerId = -vkStuff.GroupId,
                                                                    Id = photos.ElementAt(currentPhoto).Id
                                                                }
                                                            });
                                                    }
                                                }
                                            }
                                            success = true;
                                        }
                                        else
                                        {
                                            await Task.Delay(1000);
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
                            form = new MultipartFormDataContent();
                            photosUploadProperties.Clear();
                        }
                    }
                    timer += 333;
                    await Task.Delay(333);
                }
            });
        }
    
        private class PayloadStuff
        {
            public string Command { get; set; } = "";
            public int? Menu { get; set; } = null;
            public int Course { get; set; } = -1;
            public string Group { get; set; } = "";
            public int Page { get; set; } = -1;
        }
    }
}
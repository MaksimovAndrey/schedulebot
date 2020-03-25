// #define SOME_MESSAGES_TEST

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
using System.Collections.Concurrent;

using Schedulebot.Vk;
using Schedulebot.Users;
using Schedulebot.Schedule.Relevance;

namespace Schedulebot
{
    public class DepartmentItmm : IDepartment
    {
        private readonly string path;

        private readonly ConcurrentQueue<string> commandsQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<PhotoUploadProperties> photosQueue = new ConcurrentQueue<PhotoUploadProperties>();
    
        private int CoursesAmount { get; } = 4;
        private readonly Course[] courses = new Course[4]; // 4 курса
        
        private readonly VkStuff vkStuff;
        private readonly Mapper mapper;
        private readonly ICheckingRelevance checkingRelevance;
        private readonly UserRepository userRepository;
        private readonly Dictionaries dictionaries;
        
        private int startDay;
        private int startWeek;
        
        public DepartmentItmm(string _path, ref List<Task> tasks)
        {
            path = _path + @"itmm/";

            #if DEBUG
                vkStuff = new VkStuff(path + "settings-.txt");
            #else
                vkStuff = new VkStuff(path + "settings.txt");
            #endif
            
            userRepository = new UserRepository(path + "users.txt");
            dictionaries = new Dictionaries(path + @"manualProcessing/");
            for (int currentCourse = 0; currentCourse < 4; ++currentCourse)
                courses[currentCourse] = new Course(path + @"downloads/" + currentCourse + "_course.xls", dictionaries);
            mapper = new Mapper(courses);
            checkingRelevance = new CheckingRelevanceItmm(path);
            
            LoadSettings();
            
            ConstructKeyboards();

            LoadUploadedSchedule();

            #if DEBUG
                tasks.Add(ExecuteMethodsAsync());
                tasks.Add(GetMessagesAsync());
                tasks.Add(UploadPhotosAsync());
                tasks.Add(SaveUsersAsync());
            #else
                tasks.Add(ExecuteMethodsAsync());
                tasks.Add(GetMessagesAsync());
                tasks.Add(UploadPhotosAsync());
                tasks.Add(SaveUsersAsync());
            #endif

            EnqueueMessage(
                userId: vkStuff.adminId,
                message: DateTime.Now.ToString() + " | Запустился"
            );

            bool changes = UploadWeekSchedule();
            while (!commandsQueue.IsEmpty || !photosQueue.IsEmpty)
                Thread.Sleep(5000);
            if (changes)
                SaveUploadedSchedule();

            #if DEBUG
                tasks.Add(CheckRelevanceAsync());
            #else
                tasks.Add(CheckRelevanceAsync());
            #endif

            EnqueueMessage(
                userId: vkStuff.adminId,
                message: DateTime.Now.ToString() + " | Запустил CheckRelevance"
            );
        }

        private bool UploadWeekSchedule()
        {
            bool result = false;
            for (int currentCourse = 0; currentCourse < CoursesAmount; currentCourse++)
            {
                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.Count; currentGroup++)
                {
                    for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
                    {
                        if (courses[currentCourse].groups[currentGroup].scheduleSubgroups[currentSubgroup].PhotoId == 0)
                        {
                            UpdateProperties updateProperties = new UpdateProperties();

                            updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                            updateProperties.drawingStandartScheduleInfo.date
                                = checkingRelevance.DatesAndUrls.dates[currentCourse];
                            updateProperties.drawingStandartScheduleInfo.weeks
                                = courses[currentCourse].groups[currentGroup].scheduleSubgroups[currentSubgroup].weeks;
                            updateProperties.drawingStandartScheduleInfo.group
                                = courses[currentCourse].groups[currentGroup].name;
            
                            PhotoUploadProperties photoUploadProperties
                                = courses[currentCourse].groups[currentGroup].UpdateSubgroup(currentSubgroup, updateProperties);

                            photoUploadProperties.GroupName = courses[currentCourse].groups[currentGroup].name;
                            photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                            photoUploadProperties.Course = currentCourse;
                            photoUploadProperties.GroupIndex = currentGroup;
                            photoUploadProperties.ToSend = false;
                            
                            photosQueue.Enqueue(new PhotoUploadProperties(photoUploadProperties));
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        private async Task SaveUsersAsync()
        {
            while (true)
            {
                await Task.Delay(3600000);
                userRepository.SaveUsers(path);
            }
        }

        private static class ConstructKeyboardsProperties
        {
            public const int buttonsInLine = 2; // 1..4 ограничения vk
            public const int linesInKeyboard = 4; // 1..9 ограничения vk
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
                return "Верхняя";
            return "Нижняя";
        }

        private int CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            return ((DateTime.Now.DayOfYear - startDay) / 7 + startWeek) % 2;
        }
        
        private void LoadSettings()
        {
            using (StreamReader file = new StreamReader(
                #if DEBUG
                    path + "settings-.txt",
                #else
                    path + "settings.txt",
                #endif
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
                        }
                    }
                }
            }
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

                    var indexes = mapper.GetCourseAndIndex(group);
                    if (indexes.Item1 != null)
                        courses[(int)indexes.Item1].groups[indexes.Item2].scheduleSubgroups[subgroup - 1].PhotoId = id;
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
        
        public async Task GetMessagesAsync()
        {
            await Task.Run(() =>
            {
                LongPollServerResponse serverResponse = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.groupId);
                BotsLongPollHistoryResponse historyResponse = null;
                BotsLongPollHistoryParams botsLongPollHistoryParams = new BotsLongPollHistoryParams()
                {
                    Server = serverResponse.Server,
                    Ts = serverResponse.Ts,
                    Key = serverResponse.Key,
                    Wait = 25
                };
                #if (DEBUG && SOME_MESSAGES_TEST)
                    Message[] messages = new Message[11];
                    List<Attachment> attachments = new List<Attachment>();
                    System.Collections.ObjectModel.ReadOnlyCollection<VkNet.Model.Attachments.Attachment> test = new System.Collections.ObjectModel.ReadOnlyCollection<Attachment>(attachments);
                    for (int i = 0; i < 11; ++i)
                    {
                        long l = i + 1;
                        messages[i] = new Message();
                        messages[i].Text = "На завтра";
                        messages[i].PeerId = l;
                        messages[i].Attachments = test;
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        int t = i;
                        Task.Run(() => MessageResponse(messages[t]));
                        Console.WriteLine(t);
                    }
                #endif
                while (true)
                {
                    try
                    {
                        Console.WriteLine( DateTime.Now.ToString() + " Получаю сообщения");
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
                                Task.Run(() => MessageResponse(update.Message));
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
                            LongPollServerResponse server = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.groupId);
                            botsLongPollHistoryParams.Ts = server.Ts;
                            botsLongPollHistoryParams.Key = server.Key;
                            botsLongPollHistoryParams.Server = server.Server;
                        }
                    }
                    catch
                    {
                        LongPollServerResponse server = vkStuff.api.Groups.GetLongPollServer((ulong)vkStuff.groupId);
                        botsLongPollHistoryParams.Ts = server.Ts;
                        botsLongPollHistoryParams.Key = server.Key;
                        botsLongPollHistoryParams.Server = server.Server;
                    }
                }
            });
        }

        public void MessageResponse(Message message)
        {
            if (message.Payload == null)
            {
                if (message.PeerId == vkStuff.adminId)
                {
                    if (message.Text.IndexOf("Помощь") == 0 || message.Text.IndexOf("Help") == 0)
                    {
                        string help = "Команды:\n\nРассылка <всем,*КУРС*,*ГРУППА*>\n--отправляет расписание на неделю выбранным юзерам\nОбновить <все,*КУРС*> [нет]\n--обновляет расписание для выбранных курсов, отправлять ли обновление юзерам (по умолчанию - да)\nПерезагрузка\n--перезагружает бота(для применения обновления версии бота)\n\nCommands:\n\nDistribution <all,*COURSE*,*GROUP*>\n--отправляет расписание на неделю выбранным юзерам\nUpdate <all,*COURSE*> [false]\n--обновляет расписание для выбранных курсов, отправлять ли обновление юзерам (по умолчанию - да)\nReboot\n--перезагружает бота(для применения обновления версии бота)\n";
                        EnqueueMessage(userId: message.PeerId, message: help);
                    }
                    else if (message.Text.IndexOf("Рассылка") == 0 || message.Text.IndexOf("Distribution") == 0)
                    {
                        string temp = message.Text.Substring(message.Text.IndexOf(' ') + 1);
                        string toWhom = temp.Substring(0, temp.IndexOf(' '));
                        string messageStr = temp.Substring(temp.IndexOf(' ') + 1); // сообщение
                        if (toWhom == "всем" || toWhom == "all")
                        {
                            EnqueueMessage(
                                userIds: userRepository.GetIds(),
                                message: messageStr);
                            EnqueueMessage(
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
                                EnqueueMessage(
                                    userIds: userRepository.GetIds(toCourse, mapper),
                                    message: messageStr);
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Выполнено");
                            }
                            else
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Ошибка рассылки:\nневерный курс: " + toWhom + "\nВведите значение от 1 до 4");
                            }
                        }
                        else
                        {
                            EnqueueMessage(
                                userIds: userRepository.GetIds(toWhom),
                                message: messageStr);
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Выполнено");
                        }
                    }
                    else if (message.Text.IndexOf("Обновить") == 0 || message.Text.IndexOf("Update") == 0)
                    {
                        // todo
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "todo");
                        return;
                    }
                    else if (message.Text.IndexOf("Перезагрузка") == 0 || message.Text.IndexOf("Reboot") == 0)
                    {
                        while (courses[0].isUpdating || courses[1].isUpdating || courses[2].isUpdating || courses[3].isUpdating)
                            Thread.Sleep(60000);
                        while (!commandsQueue.IsEmpty || !photosQueue.IsEmpty)
                            Thread.Sleep(5000); 
                        userRepository.SaveUsers(path);                   
                        Environment.Exit(0);
                    }
                }
                else if (message.Attachments.Count != 0)
                {
                    if (message.Attachments.Single().ToString() == "Sticker")
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "🤡");
                        return;
                    }
                    else
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Я не умею читать файлы");
                        return;
                    }
                }

                string messageTemp = message.Text;
                if (messageTemp.ToUpper().Contains("ПОДПИСАТЬСЯ "))
                {
                    messageTemp = messageTemp.Substring(messageTemp.IndexOf(' ') + 1);
                    string group;
                    int subgroup;
                    if (messageTemp.Contains(' ') && messageTemp.Length > messageTemp.IndexOf(' ') + 1)
                    {
                        if (messageTemp.Length != messageTemp.IndexOf(' ') + 2
                            || !Int32.TryParse(messageTemp.Substring(messageTemp.IndexOf(' ') + 1, 1), out subgroup)
                            || (subgroup != 1 && subgroup != 2))
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                attachments: new List<MediaAttachment>()
                                {
                                    vkStuff.subscribeInfo
                                },
                                message: "Некорректный ввод настроек"
                            );
                            return;
                        }
                        group = messageTemp.Substring(0, messageTemp.IndexOf(' '));
                    }
                    else
                    {
                        group = messageTemp;
                        subgroup = 1;
                    }
                    StringBuilder messageBuilder = new StringBuilder();
                    if (userRepository.ContainsUser(message.PeerId))
                    {
                        userRepository.EditUser(
                            id: (long)message.PeerId,
                            newGroup: group,
                            newSubgroup: subgroup);
                        messageBuilder.Append("Вы изменили настройки на ");
                    }
                    else
                    {
                        userRepository.AddUser((long)message.PeerId, group, subgroup);
                        messageBuilder.Append("Вы подписались на ");
                    }
                    messageBuilder.Append(group);
                    messageBuilder.Append(" (");
                    messageBuilder.Append(subgroup);
                    messageBuilder.Append(')');
                    EnqueueMessage(
                        userId: message.PeerId,
                        message: messageBuilder.ToString()
                    );
                    return;
                }
                message.Text = message.Text.ToUpper();
                if (message.Text == "НА НЕДЕЛЮ"
                    || message.Text == "НА ЗАВТРА"
                    || message.Text == "НА СЕГОДНЯ"
                    || message.Text == "ССЫЛКА")
                {
                    if (!userRepository.GetUser(message.PeerId, out Users.User user))
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            attachments: new List<MediaAttachment>()
                            {
                                vkStuff.subscribeInfo
                            },
                            message: "Вы не настроили свою группу");
                        return;
                    }
                    (int?, int) userMapping = mapper.GetCourseAndIndex(user.Group);
                    if (userMapping.Item1 == null)
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            attachments: new List<MediaAttachment>()
                            {
                                vkStuff.subscribeInfo
                            },
                            message: "Ваша группа не существует, настройте заново."
                        );
                        return;
                    }
                    if (courses[(int)userMapping.Item1].isUpdating)
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Происходит обновление расписания, повторите попытку через несколько минут");
                        return;
                    }
                    else if (message.Text == "ССЫЛКА")
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("Расписание для ");
                        stringBuilder.Append(userMapping.Item1 + 1);
                        stringBuilder.Append(" курса: ");
                        stringBuilder.Append(checkingRelevance.DatesAndUrls.urls[(int)userMapping.Item1]);

                        EnqueueMessage(
                            userId: message.PeerId,
                            message: stringBuilder.ToString());
                        return;
                    }
                    else if (courses[(int)userMapping.Item1].isBroken)
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Расписание Вашего курса не обработано");
                        return;
                    }
                    switch (message.Text)
                    {
                        case "НА НЕДЕЛЮ":
                        {
                            ForWeek((int)userMapping.Item1, userMapping.Item2, user);
                            return;
                        }
                        case "НА ЗАВТРА":
                        {
                            ForTomorrow((int)userMapping.Item1, userMapping.Item2, user);
                            return;
                        }
                        case "НА СЕГОДНЯ":
                        {
                            ForToday((int)userMapping.Item1, userMapping.Item2, user);
                            return;
                        }
                    }
                    return;
                }
                switch (message.Text)
                {
                    case "ОТПИСАТЬСЯ":
                    {
                        string messageText;
                        if (userRepository.ContainsUser(message.PeerId))
                        {
                            userRepository.DeleteUser((long)message.PeerId);
                            messageText = "Вы отписались";
                        }
                        else
                        {
                            messageText = "Вы не подписаны";
                        }
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: messageText
                        );
                        return;
                    }
                    case "ПОДПИСАТЬСЯ":
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            attachments: new List<MediaAttachment>()
                            {
                                vkStuff.subscribeInfo
                            },
                            message: null
                        );
                        return;
                    }
                    case "НЕДЕЛЯ":
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: CurrentWeekStr()
                        );
                        return;
                    }
                    default:
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            attachments: new List<MediaAttachment>()
                            {
                                vkStuff.textCommandsInfo
                            },
                            message: null,
                            keyboardId: 0
                        );
                        return;
                    }
                }
            }

            PayloadStuff payloadStuff = null;
            try
            {
                payloadStuff = Newtonsoft.Json.JsonConvert.DeserializeObject<PayloadStuff>(message.Payload);
            }
            catch
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: "У Вас устаревшая клавиатура, отправляю новую",
                    keyboardId: 0);
                return;
            }
            if (payloadStuff.Command == "start")
            {
                EnqueueMessage(
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
                    EnqueueMessage(
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
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 1);
                            return;
                        }
                        case "Неделя":
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: CurrentWeekStr());
                            return;
                        }
                        case "Настройки":
                        {
                            if (userRepository.GetUser(message.PeerId, out Users.User user))
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("Вы подписаны: ");
                                stringBuilder.Append(user.Group);
                                stringBuilder.Append(" (");
                                stringBuilder.Append(user.Subgroup);
                                stringBuilder.Append(')');

                                MessageKeyboard keyboardCustom = vkStuff.menuKeyboards[3];
                                keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();
                                
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    customKeyboard: keyboardCustom
                                );
                                return;
                            }
                            else
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: 2
                                );
                                return;
                            }
                        }
                        case "Информация":
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Текущая версия - v2.3\n\nПри обновлении расписания на сайте Вам придёт сообщение. Далее Вы получите одно из трех сообщений:\n 1) Новое расписание *картинка*\n 2) Для Вас изменений нет\n 3) Не удалось скачать/обработать расписание *ссылка*\n Если не придёт никакого сообщения, Ваша группа скорее всего изменилась/не найдена. Настройте заново.\n\nВ расписании могут встретиться верхние индексы, предупреждающие о возможных ошибках. Советую ознакомиться со статьёй: vk.com/@itmmschedulebot-raspisanie");
                            return;
                        }
                        default:
                        {
                            EnqueueMessage(
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
                        EnqueueMessage(
                            userId: message.PeerId,
                            keyboardId: 0);
                        return;
                    }
                    if (!userRepository.GetUser(message.PeerId, out Users.User user))
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                            keyboardId: 2);
                        return;
                    }
                    (int?, int) userMapping = mapper.GetCourseAndIndex(user.Group);
                    if (userMapping.Item1 == null)
                    {
                        MessageKeyboard keyboardCustom = vkStuff.menuKeyboards[3];

                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("Вы подписаны: ");
                        stringBuilder.Append(user.Group);
                        stringBuilder.Append(" (");
                        stringBuilder.Append(user.Subgroup);
                        stringBuilder.Append(")");

                        keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();

                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Ваша группа не существует, настройте заново",
                            customKeyboard: keyboardCustom);
                        return;
                    }
                    if (courses[(int)userMapping.Item1].isUpdating)
                    {
                        EnqueueMessage(
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
                        stringBuilder.Append(checkingRelevance.DatesAndUrls.urls[(int)userMapping.Item1]);

                        EnqueueMessage(
                            userId: message.PeerId,
                            message: stringBuilder.ToString());
                        return;
                    }
                    else if (courses[(int)userMapping.Item1].isBroken)
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Расписание Вашего курса не обработано");
                        return;
                    }
                    switch (message.Text)
                    {
                        case "На неделю":
                        {
                            ForWeek((int)userMapping.Item1, userMapping.Item2, user);
                            return;
                        }
                        case "На сегодня":
                        {
                            ForToday((int)userMapping.Item1, userMapping.Item2, user);
                            return;
                        }
                        case "На завтра":
                        {
                            ForTomorrow((int)userMapping.Item1, userMapping.Item2, user);
                            return;
                        }
                        default:
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Произошла ошибка в меню 1, что-то с message.Text",
                                keyboardId: 0);
                            return;
                        }
                    }
                }
                case 2: // 2 и 3 тут
                {
                    if (message.Text == "Вы не подписаны" || message.Text.Contains("Вы подписаны"))
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Попробуйте нажать на другую кнопку");
                        return;
                    }
                    switch (message.Text)
                    {
                        case "Отписаться":
                        {
                            if (!userRepository.GetUser(message.PeerId, out Users.User user))
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Вы не можете отписаться, так как Вы не подписаны");
                                return;
                            }
                            StringBuilder messageBuilder = new StringBuilder();
                            messageBuilder.Append("Отменена подписка на ");
                            messageBuilder.Append(user.Group);
                            messageBuilder.Append(" (");
                            messageBuilder.Append(user.Subgroup);
                            messageBuilder.Append(')');

                            userRepository.DeleteUser((long)message.PeerId);

                            EnqueueMessage(
                                userId: message.PeerId,
                                message: messageBuilder.ToString(),
                                keyboardId: 2);
                            return;
                        }
                        case "Подписаться":
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 4);
                            return;
                        }
                        case "Переподписаться":
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 4);
                            return;
                        }
                        case "Изменить подгруппу":
                        {
                            Users.User user = userRepository.ChangeSubgroup(message.PeerId);
                            if (user == null)
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться",
                                    keyboardId: 2);
                            }
                            else
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append("Вы подписаны: ");
                                stringBuilder.Append(user.Group);
                                stringBuilder.Append(" (");
                                stringBuilder.Append(user.Subgroup);
                                stringBuilder.Append(')');

                                MessageKeyboard keyboardCustom;
                                keyboardCustom = vkStuff.menuKeyboards[3];
                                keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();
                            
                                stringBuilder.Clear();
                                stringBuilder.Append("Ваша подгруппа: ");
                                stringBuilder.Append(user.Subgroup);

                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: stringBuilder.ToString(),
                                    customKeyboard: keyboardCustom);
                            }
                            return;
                        }
                        case "Назад":
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 0);
                            return;
                        }
                        default:
                        {
                            EnqueueMessage(
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
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Попробуйте нажать на другую кнопку");
                        return;
                    }
                    else if (message.Text == "Назад")
                    {
                        if (!userRepository.GetUser(message.PeerId, out Users.User user))
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Отправляю клавиатуру",
                                keyboardId: 2);
                            return;
                        }
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("Вы подписаны: ");
                        stringBuilder.Append(user.Group);
                        stringBuilder.Append(" (");
                        stringBuilder.Append(user.Subgroup);
                        stringBuilder.Append(')');

                        MessageKeyboard keyboardCustom = vkStuff.menuKeyboards[3];
                        keyboardCustom.Buttons.First().First().Action.Label = stringBuilder.ToString();
                        
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Отправляю клавиатуру",
                            customKeyboard: keyboardCustom);
                        return;
                    }
                    else if (message.Text.Length == 1)
                    {
                        Int32.TryParse(message.Text, out int course);
                        course--;
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Выберите группу",
                            customKeyboard: courses[course].keyboards[0]);
                        return;
                    }
                    else
                    {
                        EnqueueMessage(
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
                        EnqueueMessage(
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
                            userRepository.AddUser(new Users.User((long)message.PeerId, payloadStuff.Group, subgroup));
                            stringBuilder.Append("Вы подписались на ");
                        }

                        stringBuilder.Append(payloadStuff.Group);
                        stringBuilder.Append(" (");
                        stringBuilder.Append(message.Text);
                        stringBuilder.Append(')');

                        EnqueueMessage(
                            userId: message.PeerId,
                            message: stringBuilder.ToString(),
                            keyboardId: 0);
                        return;
                    }
                    else
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Произошла ошибка в меню 5, что-то с message.Text",
                            keyboardId: 0);
                        return;
                    }
                }
                case 40:
                {
                    if (payloadStuff.Page == -1)
                    {
                        MessageKeyboard customKeyboard;
                        customKeyboard = vkStuff.menuKeyboards[5];
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("{\"menu\": \"5\", \"group\": \"");
                        stringBuilder.Append(message.Text);
                        stringBuilder.Append("\", \"course\": \"");
                        stringBuilder.Append(payloadStuff.Course);
                        stringBuilder.Append("\"}");
                        customKeyboard.Buttons.First().First().Action.Payload = stringBuilder.ToString();
                        customKeyboard.Buttons.First().ElementAt(1).Action.Payload = customKeyboard.Buttons.First().First().Action.Payload;
                        customKeyboard.Buttons.ElementAt(1).First().Action.Payload = customKeyboard.Buttons.First().First().Action.Payload;
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Выберите подгруппу, если нет - 1",
                            customKeyboard: customKeyboard);
                        return;
                    }
                    switch (message.Text)
                    {
                        case "Назад":
                        {
                            if (payloadStuff.Page == 0)
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Отправляю клавиатуру",
                                    keyboardId: 4);
                                return;
                            }
                            else
                            {
                                EnqueueMessage(
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
                                keyboardCustom = courses[payloadStuff.Course].keyboards[0];
                            else
                                keyboardCustom = courses[payloadStuff.Course].keyboards[payloadStuff.Page + 1];
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Отправляю клавиатуру",
                                customKeyboard: keyboardCustom);
                            return;
                        }
                        default:
                        {
                            if (message.Text.Contains(" из "))
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Меню страниц не реализовано");
                                return;
                            }
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Произошла ошибка в меню 40, что-то с message.Text",
                                keyboardId: 0);
                            return;
                        }
                    }
                }
            }
        }

        private void ForWeek(int course, int groupIndex, Users.User user)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Расписание для ");
            stringBuilder.Append(user.Group);
            stringBuilder.Append(" (");
            stringBuilder.Append(user.Subgroup);
            stringBuilder.Append(')');    

            EnqueueMessage(
                userId: user.Id,
                message: stringBuilder.ToString(),
                attachments: new List<MediaAttachment>
                {
                    new Photo()
                    {
                        AlbumId = vkStuff.mainAlbumId,
                        OwnerId = -vkStuff.groupId,
                        Id = courses[course].groups[groupIndex].scheduleSubgroups[user.Subgroup - 1].PhotoId
                    }
                });
            return;
        }

        private void ForTomorrow(int course, int groupIndex, Users.User user)
        {
            int week = CurrentWeek();
            int today = (int)DateTime.Now.DayOfWeek;
            if (today == 6)
            {
                week = (week + 1) % 2;
                int day = 0;
                while (!courses[course].groups[groupIndex]
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
                long photoId = courses[course].groups[groupIndex]
                    .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                    .days[day].PhotoId;
                if (photoId == 0)
                {
                    Drawing.DrawingDayScheduleInfo drawingDayScheduleInfo = new Drawing.DrawingDayScheduleInfo();
                    drawingDayScheduleInfo.date = checkingRelevance.DatesAndUrls.dates[course];
                    drawingDayScheduleInfo.day = courses[course].groups[groupIndex]
                        .scheduleSubgroups[user.Subgroup - 1].weeks[week].days[day];
                    drawingDayScheduleInfo.dayOfWeek = day;
                    drawingDayScheduleInfo.group = user.Group;
                    drawingDayScheduleInfo.subgroup = user.Subgroup.ToString();
                    drawingDayScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                    drawingDayScheduleInfo.weekProperties = week;

                    PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
                    photoUploadProperties.UploadingSchedule = UploadingSchedule.Day;
                    photoUploadProperties.ToSend = true;
                    photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                    photoUploadProperties.Course = course;
                    photoUploadProperties.GroupIndex = groupIndex;
                    photoUploadProperties.Day = day;
                    photoUploadProperties.GroupName = user.Group;
                    photoUploadProperties.Subgroup = user.Subgroup - 1;
                    photoUploadProperties.Week = week;
                    photoUploadProperties.PeerId = user.Id;
                    photoUploadProperties.Message = "Завтра воскресенье, вот расписание на ближайший учебный день";
                    try
                    {
                        photoUploadProperties.Photo = Drawing.DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                    }
                    catch
                    {
                        EnqueueMessage(
                            userId: user.Id,
                            message: "Не удалось нарисовать картинку, попробуйте позже"
                        );
                        return;
                    }
                    photosQueue.Enqueue(photoUploadProperties);
                    return;
                }
                else
                {
                    EnqueueMessage(
                        userId: user.Id,
                        message: "Завтра воскресенье, вот расписание на ближайший учебный день",
                        attachments: new List<MediaAttachment>
                        {
                            new Photo()
                            {
                                AlbumId = vkStuff.mainAlbumId,
                                OwnerId = -vkStuff.groupId,
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
                while (!courses[course].groups[groupIndex]
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
                long photoId
                    = courses[course].groups[groupIndex].scheduleSubgroups[user.Subgroup - 1].weeks[week].days[day].PhotoId;
                if (photoId == 0)
                {
                    Drawing.DrawingDayScheduleInfo drawingDayScheduleInfo = new Drawing.DrawingDayScheduleInfo();
                    drawingDayScheduleInfo.date = checkingRelevance.DatesAndUrls.dates[course];
                    drawingDayScheduleInfo.day = courses[course].groups[groupIndex]
                        .scheduleSubgroups[user.Subgroup - 1].weeks[week].days[day];
                    drawingDayScheduleInfo.dayOfWeek = day;
                    drawingDayScheduleInfo.group = user.Group;
                    drawingDayScheduleInfo.subgroup = user.Subgroup.ToString();
                    drawingDayScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                    drawingDayScheduleInfo.weekProperties = week;

                    PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
                    photoUploadProperties.UploadingSchedule = UploadingSchedule.Day;
                    photoUploadProperties.ToSend = true;
                    photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                    photoUploadProperties.Course = course;
                    photoUploadProperties.GroupIndex = groupIndex;
                    photoUploadProperties.Day = day;
                    photoUploadProperties.GroupName = user.Group;
                    photoUploadProperties.Subgroup = user.Subgroup - 1;
                    photoUploadProperties.Week = week;
                    photoUploadProperties.PeerId = user.Id;
                    photoUploadProperties.Message = messageTemp;
                    try
                    {
                        photoUploadProperties.Photo = Drawing.DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                    }
                    catch
                    {
                        EnqueueMessage(
                            userId: user.Id,
                            message: "Не удалось нарисовать картинку, попробуйте позже"
                        );
                        return;
                    }
                    photosQueue.Enqueue(photoUploadProperties);
                    return;
                }
                else
                {   
                    EnqueueMessage(
                        userId: user.Id,
                        message: messageTemp,
                        attachments: new List<MediaAttachment>
                        {
                            new Photo()
                            {
                                AlbumId = vkStuff.mainAlbumId,
                                OwnerId = -vkStuff.groupId,
                                Id = photoId
                            }
                        });
                    return;
                }
            }
        }

        private void ForToday(int course, int groupIndex, Users.User user)
        {
            int week = CurrentWeek();
            int today = (int)DateTime.Now.DayOfWeek;
            if (today == 0)
            {
                EnqueueMessage(
                    userId: user.Id,
                    message: "Сегодня воскресенье");
                return;
            }
            else
            {
                --today;
                if (courses[course].groups[groupIndex]
                    .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                    .days[today].isStudying)
                {
                    long photoId = courses[course].groups[groupIndex]
                        .scheduleSubgroups[user.Subgroup - 1].weeks[week]
                        .days[today].PhotoId;
                    if (photoId == 0)
                    {
                        Drawing.DrawingDayScheduleInfo drawingDayScheduleInfo = new Drawing.DrawingDayScheduleInfo();
                        drawingDayScheduleInfo.date = checkingRelevance.DatesAndUrls.dates[course];
                        drawingDayScheduleInfo.day = courses[course].groups[groupIndex]
                            .scheduleSubgroups[user.Subgroup - 1].weeks[week].days[today];
                        drawingDayScheduleInfo.dayOfWeek = today;
                        drawingDayScheduleInfo.group = user.Group;
                        drawingDayScheduleInfo.subgroup = user.Subgroup.ToString();
                        drawingDayScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                        drawingDayScheduleInfo.weekProperties = week;

                        PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
                        photoUploadProperties.UploadingSchedule = UploadingSchedule.Day;
                        photoUploadProperties.ToSend = true;
                        photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                        photoUploadProperties.Course = course;
                        photoUploadProperties.GroupIndex = groupIndex;
                        photoUploadProperties.Day = today;
                        photoUploadProperties.GroupName = user.Group;
                        photoUploadProperties.Subgroup = user.Subgroup - 1;
                        photoUploadProperties.Week = week;
                        photoUploadProperties.PeerId = user.Id;
                        photoUploadProperties.Message = "Расписание на сегодня";
                        try
                        {
                            photoUploadProperties.Photo = Drawing.DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                        }
                        catch
                        {
                            EnqueueMessage(
                                userId: user.Id,
                                message: "Не удалось нарисовать картинку, попробуйте позже"
                            );
                            return;
                        }
                        photosQueue.Enqueue(photoUploadProperties);
                        return;
                    }
                    else
                    {
                        EnqueueMessage(
                            userId: user.Id,
                            message: "Расписание на сегодня",
                            attachments: new List<MediaAttachment>
                            {
                                new Photo()
                                {
                                    AlbumId = vkStuff.mainAlbumId,
                                    OwnerId = -vkStuff.groupId,
                                    Id = photoId
                                }
                            });
                        return;
                    }
                }
                else
                {
                    EnqueueMessage(
                        userId: user.Id,
                        message: "Сегодня Вы не учитесь");
                    return;
                }
            }
        }

        public void EnqueueMessage(
            long? userId = null,
            List<long> userIds = null,
            string message = "Отправляю клавиатуру",
            List<MediaAttachment> attachments = null,
            int? keyboardId = null,
            MessageKeyboard customKeyboard = null)
        {
            MessagesSendParams messageSendParams = new MessagesSendParams()
            {
                Message = message,
                RandomId = (int)DateTime.Now.Ticks
            };
            if (userIds != null)
            {
                if (userIds.Count == 0)
                    return;
                else if (userIds.Count > 100)
                {
                    messageSendParams.UserIds = userIds.GetRange(0, 100);
                    userIds.RemoveRange(0, 100);
                    EnqueueMessage(
                        userIds: userIds,
                        message: message,
                        attachments: attachments,
                        keyboardId: keyboardId,
                        customKeyboard: customKeyboard);
                }
                else
                    messageSendParams.UserIds = userIds;
            }
            
            messageSendParams.PeerId = userId;

            messageSendParams.Attachments = attachments;

            messageSendParams.Keyboard = customKeyboard;
            switch (keyboardId)
            {
                case null:
                {
                    break;
                }
                case 0:
                {
                    messageSendParams.Keyboard = vkStuff.menuKeyboards[0];
                    break;
                }
                case 1:
                {
                    messageSendParams.Keyboard = vkStuff.menuKeyboards[1];
                    break;
                }
                case 2:
                {
                    messageSendParams.Keyboard = vkStuff.menuKeyboards[2];
                    break;
                }
                case 4:
                {
                    messageSendParams.Keyboard = vkStuff.menuKeyboards[4];
                    break;
                }
            }
            commandsQueue.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messageSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
        }

        public async Task ExecuteMethodsAsync()
        {
            int queueCommandsAmount;
            int commandsInRequestAmount = 0;
            int timer = 0;
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                queueCommandsAmount
                    = commandsQueue.Count <= 25 - commandsInRequestAmount
                    ? commandsQueue.Count : 25 - commandsInRequestAmount;
                for (int i = 0; i < queueCommandsAmount; ++i)
                {
                    if (commandsQueue.TryDequeue(out string command))
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
                if ((commandsInRequestAmount == 25 && timer >= 60) || timer >= 200)
                {
                    if (commandsInRequestAmount == 0)
                    {
                        timer = 0;
                    }
                    else
                    {
                        #if DEBUG
                            Console.WriteLine(stringBuilder.ToString());
                        #endif

                        var response = vkStuff.api.Execute.Execute(stringBuilder.ToString());
                        timer = 0;
                        commandsInRequestAmount = 0;
                        stringBuilder.Clear();
                    }
                }
                timer += 20;
                await Task.Delay(20);
            }
        }

        public async Task CheckRelevanceAsync()
        {
            List<int> coursesToUpdate;
            while (true)
            {
                coursesToUpdate = await checkingRelevance.CheckRelevanceAsync();
                if (coursesToUpdate != null)
                {
                    if (coursesToUpdate.Count != 0)
                    {
                        List<Schedulebot.Vk.PhotoUploadProperties> photosUploadProperties = new List<PhotoUploadProperties>();
                        List<int> updatingCourses = new List<int>();
                        for (int i = 0; i < coursesToUpdate.Count; ++i)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("Вышло новое расписание ");
                            stringBuilder.Append(checkingRelevance.DatesAndUrls.dates[coursesToUpdate[i]]);
                            stringBuilder.Append(". Ожидайте результата обработки.");

                            Task enqueueMessageTask = Task.Run(() => EnqueueMessage(
                                userIds: userRepository.GetIds(coursesToUpdate[i], mapper),
                                message: stringBuilder.ToString()));

                            UpdateProperties updateProperties = new UpdateProperties();
                            updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                            updateProperties.photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                            updateProperties.photoUploadProperties.ToSend = true;
                            updateProperties.photoUploadProperties.UploadingSchedule = UploadingSchedule.Week;

                            var tempPhotosList
                                = await courses[coursesToUpdate[i]]
                                    .UpdateAsync(
                                        checkingRelevance.DatesAndUrls.urls[coursesToUpdate[i]],
                                        checkingRelevance.DatesAndUrls.dates[coursesToUpdate[i]],
                                        updateProperties,
                                        dictionaries);

                            if (tempPhotosList != null)
                            {
                                photosUploadProperties.AddRange(tempPhotosList);
                                updatingCourses.Add(coursesToUpdate[i]);
                            }
                        }
                        if (updatingCourses.Count != 0)
                        {
                            mapper.CreateMaps(courses);
                            ConstructKeyboards();

                            for (int i = 0; i < photosUploadProperties.Count; i++)
                                photosQueue.Enqueue(photosUploadProperties[i]);

                            List<(string, int)> newGroupSubgroupList = new List<(string, int)>();
                            for (int currentPhoto = 0; currentPhoto < photosUploadProperties.Count; currentPhoto++)
                                newGroupSubgroupList.Add((photosUploadProperties[currentPhoto].GroupName, photosUploadProperties[currentPhoto].Subgroup + 1));

                            Task enqueueMessageTask = Task.Run(() => EnqueueMessage(
                                message: "Для Вас изменений нет",
                                userIds: userRepository.GetIds(mapper.GetOldGroupSubgroupList(newGroupSubgroupList, updatingCourses))));
                            
                            while (true)
                            {
                                if (photosQueue.IsEmpty)
                                {
                                    await Task.Delay(5000);
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
                                    stringBuilder.Append(checkingRelevance.DatesAndUrls.dates[currentUpdatingCourse]);
                                    stringBuilder.Append(". Ссылка: ");
                                    stringBuilder.Append(checkingRelevance.DatesAndUrls.urls[currentUpdatingCourse]);

                                    Task enqueueMessageTask2 = Task.Run(() => EnqueueMessage(
                                        userIds: userRepository.GetIds(updatingCourses[currentUpdatingCourse], mapper),
                                        message: stringBuilder.ToString()));
                                }
                                courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;
                            }
                            checkingRelevance.DatesAndUrls.Save();
                        }
                    }
                }
                await Task.Delay(600000);
            }
        }
        
        public async void UploadedPhotoResponse(PhotoUploadProperties photo)
        {
            await Task.Run(() => 
            {
                switch (photo.UploadingSchedule)
                {
                    case UploadingSchedule.Day:
                    {
                        courses[photo.Course].groups[photo.GroupIndex].scheduleSubgroups[photo.Subgroup]
                        .weeks[photo.Week].days[photo.Day].PhotoId
                            = photo.Id;
                        if (photo.ToSend && photo.PeerId != 0)
                        {
                            EnqueueMessage(
                                userId: photo.PeerId,
                                message: photo.Message,
                                attachments: new List<MediaAttachment>
                                {
                                    new Photo()
                                    {
                                        AlbumId = photo.AlbumId,
                                        OwnerId = -vkStuff.groupId,
                                        Id = photo.Id
                                    }
                                });
                        }
                        break;
                    }
                    case UploadingSchedule.Week:
                    {
                        courses[photo.Course].groups[photo.GroupIndex].scheduleSubgroups[photo.Subgroup].PhotoId
                            = photo.Id;
                        if (photo.ToSend)
                        {
                            List<long> ids = userRepository.GetIds(photo.GroupName, photo.Subgroup + 1);
                            EnqueueMessage(
                                userIds: ids,
                                message: photo.Message,
                                attachments: new List<MediaAttachment>
                                {
                                    new Photo()
                                    {
                                        AlbumId = photo.AlbumId,
                                        OwnerId = -vkStuff.groupId,
                                        Id = photo.Id
                                    }
                                });
                        }
                        break;
                    }
                }
            });
        }

        public async Task UploadPhotosAsync()
        {
            int queuePhotosAmount;
            int photosInRequestAmount = 0;
            int timer = 0;
            MultipartFormDataContent form = new MultipartFormDataContent();
            List<PhotoUploadProperties> photosUploadProperties = new List<PhotoUploadProperties>();
            while (true)
            {
                queuePhotosAmount = photosQueue.Count;
                if (queuePhotosAmount > 5 - photosInRequestAmount)
                {
                    queuePhotosAmount = 5 - photosInRequestAmount;
                }
                for (int i = 0; i < queuePhotosAmount; ++i)
                {
                    if (photosQueue.TryDequeue(out PhotoUploadProperties photoUploadProperties))
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
                                var uploadServer = vkStuff.apiPhotos.Photo.GetUploadServer(vkStuff.mainAlbumId, vkStuff.groupId);
                                response = null;
                                response = await ScheduleBot.client.PostAsync(new Uri(uploadServer.UploadUrl), form);
                                if (response != null)
                                {
                                    IReadOnlyCollection<Photo> photos = vkStuff.apiPhotos.Photo.Save(new PhotoSaveParams
                                    {
                                        SaveFileResponse = Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync()),
                                        AlbumId = vkStuff.mainAlbumId,
                                        GroupId = vkStuff.groupId
                                    });
                                    if (photos.Count == photosInRequestAmount)
                                    {
                                        for (int currentPhoto = 0; currentPhoto < photosInRequestAmount; currentPhoto++)
                                        {
                                            photosUploadProperties[currentPhoto].Id = (long)photos.ElementAt(currentPhoto).Id;
                                            UploadedPhotoResponse(photosUploadProperties[currentPhoto]);
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
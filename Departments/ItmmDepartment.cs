//! DEBUG ONLY
//#define SOME_MESSAGES_TEST
//#define DONT_UPLOAD_WEEK_SCHEDULE
#define DONT_CHECK_CHANGES
//! DEBUG ONLY


using HtmlAgilityPack;
using Newtonsoft.Json;
using Schedulebot.Drawing;
using Schedulebot.Schedule.Relevance;
using Schedulebot.Mapper;
using Schedulebot.Mapper.Utils;
using Schedulebot.Users;
using Schedulebot.Users.Enums;
using Schedulebot.Departments.Utils;
using Schedulebot.Utils;
using Schedulebot.Vk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using Schedulebot;

namespace Schedulebot.Departments
{
    public class DepartmentItmm : IDepartment
    {
        private string Path { get; }

        private readonly ConcurrentQueue<string> commandsQueue
            = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<PhotoUploadProperties> photosQueue
            = new ConcurrentQueue<PhotoUploadProperties>();

        private int CoursesCount { get; } = 4;
        private readonly Course[] courses = new Course[4];

        private List<MessageKeyboard>[] CoursesKeyboards { get; set; }

        private readonly VkStuff vkStuff;
        private readonly Mapper.Mapper mapper;
        private readonly IRelevance relevance;
        private readonly UserRepository userRepository;
        private readonly Dictionaries dictionaries;

        private int startDay;
        private int startWeek;

        private string importantInfo = "Здесь ничего нет.";

#if DEBUG
        private static readonly bool[] loadModule = {
            true, // 0 ExecuteMethodsAsync()
            true, // 1 GetMessagesAsync()
            true, // 2 UploadPhotosAsync()
            true, // 3 SaveUsersAsync()
            false  // 4 CheckRelevanceAsync()
        };
#else
        private static readonly bool[] loadModule = { 
            true, // 0 ExecuteMethodsAsync()
            true, // 1 GetMessagesAsync()
            true, // 2 UploadPhotosAsync()
            true, // 3 SaveUsersAsync()
            true  // 4 CheckRelevanceAsync()
        };
#endif

        public DepartmentItmm(string path, ref List<Task> tasks)
        {
            Path = path + Constants.defaultFolder;

            vkStuff = new VkStuff(Path + Constants.settingsFilename);

            userRepository = new UserRepository(Path + Constants.userRepositoryFilename);
            dictionaries = new Dictionaries(Path + Constants.dictionariesManualProcessingFolder);

            List<List<string>> filenames = Utils.Utils.GetCoursesFilePaths(Path + Constants.coursesPathsFilename);
            for (int currentCourse = 0; currentCourse < CoursesCount; ++currentCourse)
            {
                if (currentCourse < filenames.Count)
                    courses[currentCourse] = new Course(filenames[currentCourse], dictionaries);
                else
                    courses[currentCourse] = new Course(new List<string>(), dictionaries);
            }

            mapper = new Mapper.Mapper(courses);
            relevance = new RelevanceItmm(Path, Path + Constants.defaultDownloadFolder);

            LoadSettings();

            CoursesKeyboards = Utils.Utils.ConstructKeyboards(in mapper, CoursesCount);

            LoadUploadedSchedule();

            if (loadModule[0])
                tasks.Add(ExecuteMethodsAsync());
            if (loadModule[1])
                tasks.Add(GetMessagesAsync());
            if (loadModule[2])
                tasks.Add(UploadPhotosAsync());
            if (loadModule[3])
                tasks.Add(SaveUsersAsync());

            EnqueueMessage(
                userId: vkStuff.adminId,
                message: DateTime.Now.ToString() + " | Запустился"
            );

#if !DONT_CHECK_CHANGES || RELEASE
            bool changes = UploadWeekSchedule();
            while (!commandsQueue.IsEmpty || !photosQueue.IsEmpty)
                Thread.Sleep(5000);
            if (changes)
                SaveUploadedSchedule();
#endif

            if (loadModule[4])
                tasks.Add(StartRelevanceModule());

            EnqueueMessage(
                userId: vkStuff.adminId,
                message: DateTime.Now.ToString() + " | Запустил CheckRelevance"
            );
        }

        private bool UploadWeekSchedule()
        {
            bool result = false;
            for (int currentCourse = 0; currentCourse < CoursesCount; currentCourse++)
            {
                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.Count; currentGroup++)
                {
                    for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
                    {
                        if (courses[currentCourse].groups[currentGroup].subgroups[currentSubgroup].PhotoId == 0)
                        {
                            UpdateProperties updateProperties = new UpdateProperties();

                            updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                            updateProperties.drawingStandartScheduleInfo.date
                                = relevance.DatesAndUrls.dates[currentCourse];
                            updateProperties.drawingStandartScheduleInfo.weeks
                                = courses[currentCourse].groups[currentGroup].subgroups[currentSubgroup].weeks;
                            updateProperties.drawingStandartScheduleInfo.group
                                = courses[currentCourse].groups[currentGroup].name;

                            courses[currentCourse].groups[currentGroup]
                                .DrawSubgroupSchedule(currentSubgroup, ref updateProperties);

                            updateProperties.photoUploadProperties.GroupName
                                = courses[currentCourse].groups[currentGroup].name;
                            updateProperties.photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                            updateProperties.photoUploadProperties.Course = currentCourse;
                            updateProperties.photoUploadProperties.GroupIndex = currentGroup;
                            updateProperties.photoUploadProperties.ToSend = false;

#if !DONT_UPLOAD_WEEK_SCHEDULE
                            photosQueue.Enqueue(new PhotoUploadProperties(updateProperties.photoUploadProperties));
#endif

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
                await Task.Delay(Constants.saveUsersDelay);
                userRepository.SaveUsers(Path);
            }
        }

        private int CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            return ((DateTime.Now.DayOfYear - startDay) / 7 + startWeek) % 2;
        }

        private void LoadSettings()
        {
            using StreamReader file = new StreamReader(Path + Constants.settingsFilename, Encoding.Default);
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
                            startDay = int.Parse(value);
                            break;
                        }
                        case "startWeek":
                        {
                            startWeek = int.Parse(value);
                            break;
                        }
                    }
                }
            }
        }

        private void LoadUploadedSchedule()
        {
            using StreamReader file = new StreamReader(
                Path + Constants.uploadedScheduleFilename,
                System.Text.Encoding.Default);

            string rawLine;
            while (!file.EndOfStream)
            {
                rawLine = file.ReadLine();
                if (string.IsNullOrEmpty(rawLine))
                    break;

                var rawSpan = rawLine.AsSpan();

                int spaceIndex = rawSpan.IndexOf(' ');
                int lastSpaceIndex = rawSpan.LastIndexOf(' ');

                long id = long.Parse(rawSpan.Slice(0, spaceIndex));
                string group = rawSpan.Slice(spaceIndex + 1, lastSpaceIndex - spaceIndex - 1).ToString();
                int subgroup = int.Parse(rawSpan.Slice(lastSpaceIndex + 1, 1));

                if (mapper.TryGetCourseAndGroupIndex(group, out UserMapping mapping))
                    courses[mapping.Course].groups[mapping.GroupIndex].subgroups[subgroup - 1].PhotoId = id;
            }
        }

        private void SaveUploadedSchedule()
        {
            using StreamWriter file = new StreamWriter(Path + Constants.uploadedScheduleFilename);

            StringBuilder stringBuilder = new StringBuilder();
            for (int currentCourse = 0; currentCourse < CoursesCount; currentCourse++)
            {
                int groupsAmount = courses[currentCourse].groups.Count;
                for (int currentGroup = 0; currentGroup < groupsAmount; currentGroup++)
                {
                    stringBuilder.Append(courses[currentCourse].groups[currentGroup].subgroups[0].PhotoId);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(courses[currentCourse].groups[currentGroup].name);
                    stringBuilder.Append(" 1\n");
                    stringBuilder.Append(courses[currentCourse].groups[currentGroup].subgroups[1].PhotoId);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(courses[currentCourse].groups[currentGroup].name);
                    stringBuilder.Append(" 2\n");
                }
            }
            if (stringBuilder.Length != 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            file.WriteLine(stringBuilder.ToString());
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
                        Console.WriteLine(DateTime.Now.ToString() + " Получаю сообщения");
                        historyResponse = vkStuff.api.Groups.GetBotsLongPollHistory(botsLongPollHistoryParams);
                        if (historyResponse == null)
                            continue;
                        botsLongPollHistoryParams.Ts = historyResponse.Ts;
                        if (!historyResponse.Updates.Any())
                            continue;
                        foreach (var update in historyResponse.Updates)
                        {
                            //Console.WriteLine(update.Message.Text);
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

        private void SubscribeResponse(long? peerId, string group, int subgroup = 1, bool messageFromKeyboard = false)
        {
            StringBuilder messageBuilder = new StringBuilder();
            switch (userRepository.AddOrEditUser(peerId, group, subgroup))
            {
                case AddOrEditUserResult.Added:
                    messageBuilder.Append("Вы подписались на ");
                    break;
                case AddOrEditUserResult.Edited:
                    messageBuilder.Append("Вы изменили настройки на ");
                    break;
            }
            messageBuilder.Append(Utils.Utils.ConstructGroupSubgroup(group, subgroup));

            EnqueueMessage(
                userId: peerId,
                message: messageBuilder.ToString(),
                keyboardId: messageFromKeyboard ? (int?)0 : null);
        }

        private void CurrentWeekResponse(long? peerId)
        {
            EnqueueMessage(
                userId: peerId,
                message: Converter.WeekToString(CurrentWeek()));
        }

        //? Не знаю стоит ли это того
        private void SubscribeMessageResponse(long? peerId, bool messageFromKeyboard = false)
        {
            if (messageFromKeyboard)
            {
                EnqueueMessage(
                    userId: peerId,
                    keyboardId: 4);
            }
            else
            {
                EnqueueMessage(
                    userId: peerId,
                    attachments: new List<MediaAttachment>()
                    { vkStuff.subscribeInfo },
                    message: null);
            }
        }

        private void TextCommandSubscribeResponse(string message, long? peerId)
        {
            message = message.Substring(message.IndexOf(' ') + 1).Trim();
            if (!message.Contains(' '))
            {
                SubscribeResponse(peerId, message);
                return;
            }
            if (message.Length == message.IndexOf(' ') + 2
                && int.TryParse(message.Substring(message.Length - 1), out int subgroup)
                && (subgroup == 1 || subgroup == 2))
            {
                SubscribeResponse(peerId, message[0..^2], subgroup);
                return;
            }
            EnqueueMessage(
                userId: peerId,
                attachments: new List<MediaAttachment>() { vkStuff.subscribeInfo },
                message: "Некорректный ввод настроек");
        }

        private void UnsubscribeResponse(long? peerId, bool messageFromKeyboard = false)
        {
            const string cantUnsubscribe = "Вы не можете отписаться, так как Вы не подписаны";
            const string unsubscribeSuccess = "Отменена подписка на расписание";

            EnqueueMessage(
                userId: peerId,
                message: userRepository.DeleteUser(peerId) ? unsubscribeSuccess : cantUnsubscribe,
                keyboardId: messageFromKeyboard ? (int?)2 : null);
        }

        private void ImportantInfoResponse(long? peerId)
        {
            EnqueueMessage(
                userId: peerId,
                message: importantInfo);
        }

        /// <summary>
        /// Отвечает на сообщения:
        /// <list type="bullet">
        /// <item>На неделю</item>
        /// <item>На сегодня</item>
        /// <item>На завтра</item>
        /// <item>Ссылка</item>
        /// </list>
        /// <para>На сообщения не из списка отправляет стандартный ответ</para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="peerId"></param>
        /// <param name="messageFromKeyboard"></param>
        private void ScheduleMessageResponse(string message, long? peerId, bool messageFromKeyboard = false)
        {
            const string unknownUserMessage = "Вы не настроили свою группу";
            const string userGroupUnknownMessage = "Ваша группа не существует, настройте заново";
            const string yourCourseScheduleBroken = "Расписание Вашего курса не обработано";

            if (!userRepository.GetUser(peerId, out Users.User user))
            {
                if (messageFromKeyboard)
                {
                    EnqueueMessage(
                        userId: peerId,
                        message: Constants.unknownUserWithPayloadMessage,
                        keyboardId: 2);
                }
                else
                {
                    EnqueueMessage(
                        userId: peerId,
                        attachments: new List<MediaAttachment>() { vkStuff.subscribeInfo },
                        message: unknownUserMessage);
                }
                return;
            }
            if (!mapper.TryGetCourseAndGroupIndex(user.Group, out UserMapping userMapping))
            {
                MessageKeyboard customKeyboard = vkStuff.menuKeyboards[3];

                customKeyboard.Buttons.First().First().Action.Label =
                    Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: peerId,
                    message: userGroupUnknownMessage,
                    customKeyboard: customKeyboard);
                return;
            }
            if (courses[userMapping.Course].isUpdating)
            {
                EnqueueMessage(
                    userId: peerId,
                    message: Constants.scheduleUpdatingMessage);
                return;
            }
            if (message == Constants.linkCommand)
            {
                const string courseStr = " курса: ";

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(Constants.scheduleFor);
                stringBuilder.Append(userMapping.Course + 1);
                stringBuilder.Append(courseStr);
                stringBuilder.Append(String.Join("\n", relevance.DatesAndUrls.urls[userMapping.Course].ToArray()));

                EnqueueMessage(
                    userId: peerId,
                    message: stringBuilder.ToString());
                return;
            }
            if (courses[userMapping.Course].isBroken)
            {
                EnqueueMessage(
                    userId: peerId,
                    message: yourCourseScheduleBroken);
                return;
            }
            switch (message)
            {
                case Constants.forWeekCommand:
                    ForWeek(userMapping, user);
                    return;
                case Constants.forTodayCommand:
                    ForToday(userMapping, user);
                    return;
                case Constants.forTomorrowCommand:
                    ForTomorrow(userMapping, user);
                    return;
                default:
                    if (messageFromKeyboard)
                    {
                        EnqueueMessage(
                            userId: peerId,
                            message: Constants.oldKeyboardMessage,
                            keyboardId: 0);
                    }
                    else
                    {
                        EnqueueMessage(
                            userId: peerId,
                            attachments: new List<MediaAttachment>() { vkStuff.textCommandsInfo },
                            message: null,
                            keyboardId: 0);
                    }
                    return;
            }
        }

        private void ChangeSubgroupResponse(long? peerId)
        {
            if (userRepository.ChangeSubgroup(peerId, out Users.User user))
            {
                MessageKeyboard keyboardCustom;
                keyboardCustom = vkStuff.menuKeyboards[3];
                keyboardCustom.Buttons.First().First().Action.Label =
                    Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: peerId,
                    message: Constants.yourSubgroup + user.Subgroup.ToString(),
                    customKeyboard: keyboardCustom);
                return;
            }
            else
            {
                EnqueueMessage(
                    userId: peerId,
                    message: Constants.unknownUserWithPayloadMessage,
                    keyboardId: 2);
                return;
            }
        }

        private void SettingsResponse(long? peerId)
        {
            if (userRepository.GetUser(peerId, out Users.User user))
            {
                MessageKeyboard keyboardCustom = vkStuff.menuKeyboards[3];
                keyboardCustom.Buttons.First().First().Action.Label =
                     Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: peerId,
                    customKeyboard: keyboardCustom);
                return;
            }
            else
            {
                EnqueueMessage(
                    userId: peerId,
                    keyboardId: 2);
                return;
            }
        }

        public void MessageResponse(Message message)
        {
            const string startPayloadCommand = "start";

            if (message.Payload == null)
            {
                const string subscribeSign = "ПОДПИСАТЬСЯ ";

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
                        if (toWhom == "всем" || toWhom == "all")
                        {
                            EnqueueMessage(
                                userIds: userRepository.GetIds(),
                                message: temp.Substring(temp.IndexOf(' ') + 1));
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Выполнено");
                        }
                        else if (toWhom.Length == 1)
                        {
                            if (int.TryParse(toWhom, out int toCourse) && toCourse >= 1 && toCourse <= 4)
                            {
                                --toCourse;
                                EnqueueMessage(
                                    userIds: userRepository.GetIds(toCourse, mapper),
                                    message: temp.Substring(temp.IndexOf(' ') + 1));
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    message: "Выполнено");
                                return;
                            }
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Ошибка рассылки:\nневерный курс: " + toWhom + "\nВведите значение от 1 до 4");
                        }
                        else
                        {
                            EnqueueMessage(
                                userIds: userRepository.GetIds(toWhom),
                                message: temp.Substring(temp.IndexOf(' ') + 1));
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: "Выполнено");
                        }
                    }
                    else if (message.Text.IndexOf("Обновить") == 0 || message.Text.IndexOf("Update") == 0)
                    {
                        // TODO: update command
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
                        userRepository.SaveUsers(Path);
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

                if (message.Text.ToUpper().IndexOf(subscribeSign) == 0)
                {
                    TextCommandSubscribeResponse(message.Text, message.PeerId);
                    return;
                }

                switch (message.Text.ToUpper())
                {
                    case Constants.importantInfoCommand:
                        ImportantInfoResponse(message.PeerId);
                        return;
                    case Constants.unsubscribeCommand:
                        UnsubscribeResponse(message.PeerId);
                        return;
                    case Constants.subscribeCommand:
                        EnqueueMessage(
                            userId: message.PeerId,
                            attachments: new List<MediaAttachment>()
                            { vkStuff.subscribeInfo },
                            message: null);
                        return;
                    case Constants.currentWeekCommand:
                        CurrentWeekResponse(message.PeerId);
                        return;
                    default:
                        ScheduleMessageResponse(message.Text, message.PeerId);
                        return;
                }
            }

            PayloadStuff payloadStuff;
            try
            {
                payloadStuff = JsonConvert.DeserializeObject<PayloadStuff>(message.Payload);
            }
            catch
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: Constants.oldKeyboardMessage,
                    keyboardId: 0);
                return;
            }
            if (payloadStuff.Command == startPayloadCommand)
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: Constants.startMessage,
                    keyboardId: 0);
                return;
            }
            // По idшникам меню сортируем сообщения
            string messageStr = message.Text.ToUpper();
            switch (payloadStuff.Menu)
            {
                case null:
                {
                    EnqueueMessage(
                        userId: message.PeerId,
                        message: Constants.unknownError,
                        keyboardId: 0);
                    return;
                }
                case 0:
                {
                    switch (messageStr)
                    {
                        case Constants.scheduleMenuItem:
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 1);
                            return;
                        case Constants.currentWeekCommand:
                            CurrentWeekResponse(message.PeerId);
                            return;
                        case Constants.settingsMenuItem:
                            SettingsResponse(message.PeerId);
                            return;
                        case Constants.informationMenuItem:
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: Constants.about);
                            return;
                        default:
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: 0);
                            return;
                    }
                }
                case 1:
                {
                    switch (messageStr)
                    {
                        case Constants.backMenuItem:
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 0);
                            return;
                        case Constants.importantInfoCommand:
                            ImportantInfoResponse(message.PeerId);
                            return;
                        default:
                            ScheduleMessageResponse(messageStr, message.PeerId, true);
                            return;
                    }
                }
                case 2:
                {
                    const string subscribedOrNotButtonMessage = "ПОДПИСАНЫ";

                    if (messageStr.Contains(subscribedOrNotButtonMessage))
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: Constants.pressAnotherButton);
                        return;
                    }
                    switch (messageStr)
                    {
                        case Constants.unsubscribeCommand:
                            UnsubscribeResponse(message.PeerId, true);
                            return;
                        case Constants.subscribeCommand:
                        case Constants.resubscribeCommand:
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 4);
                            return;
                        case "ИЗМЕНИТЬ ПОДГРУППУ":
                            ChangeSubgroupResponse(message.PeerId);
                            return;
                        case Constants.backMenuItem:
                            EnqueueMessage(
                                userId: message.PeerId,
                                keyboardId: 0);
                            return;
                        default:
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: 0);
                            return;
                    }
                }
                case 4:
                {
                    switch (messageStr)
                    {
                        case Constants.chooseCourseMenuItem:
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: Constants.pressAnotherButton);
                            return;
                        case Constants.backMenuItem:
                            SettingsResponse(message.PeerId);
                            return;
                    }
                    if (messageStr.Length == 1)
                    {
                        Int32.TryParse(messageStr, out int course);
                        course--;
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: "Выберите группу",
                            customKeyboard: CoursesKeyboards[course][0]);
                        return;
                    }
                    EnqueueMessage(
                        userId: message.PeerId,
                        message: Constants.oldKeyboardMessage,
                        keyboardId: 0);
                    return;
                }
                case 5:
                {
                    if (messageStr == Constants.backMenuItem)
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            customKeyboard: CoursesKeyboards[payloadStuff.Course][0]);
                        return;
                    }
                    if (messageStr.Length == 1)
                    {
                        if (int.TryParse(messageStr, out int subgroup))
                        {
                            SubscribeResponse(message.PeerId, payloadStuff.Group, subgroup, true);
                            return;
                        }
                        else
                        {
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: Constants.unknownError,
                                keyboardId: 0);
                            return;
                        }
                    }
                    EnqueueMessage(
                        userId: message.PeerId,
                        message: Constants.oldKeyboardMessage,
                        keyboardId: 0);
                    return;
                }
                case 40:
                {
                    if (payloadStuff.Page == -1)
                    {
                        MessageKeyboard customKeyboard;
                        customKeyboard = vkStuff.menuKeyboards[5];
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("{\"menu\": \"5\", \"group\": \"");
                        //! message.Text.ToUpper() влияет на это
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
                    switch (messageStr)
                    {
                        case Constants.backMenuItem:
                        {
                            if (payloadStuff.Page == 0)
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    keyboardId: 4);
                                return;
                            }
                            else
                            {
                                EnqueueMessage(
                                    userId: message.PeerId,
                                    customKeyboard: CoursesKeyboards[payloadStuff.Course][payloadStuff.Page - 1]);
                                return;
                            }
                        }
                        case Constants.forwardMenuItem:
                        {
                            MessageKeyboard keyboardCustom;
                            if (payloadStuff.Page == CoursesKeyboards[payloadStuff.Course].Count - 1)
                                keyboardCustom = CoursesKeyboards[payloadStuff.Course][0];
                            else
                                keyboardCustom = CoursesKeyboards[payloadStuff.Course][payloadStuff.Page + 1];
                            EnqueueMessage(
                                userId: message.PeerId,
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
                                message: Constants.oldKeyboardMessage,
                                keyboardId: 0);
                            return;
                        }
                    }
                }
            }
        }

        private void ForWeek(UserMapping userMapping, Users.User user)
        {
            EnqueueMessage(
            userId: user.Id,
            message: Constants.scheduleFor + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup),
            attachments: new List<MediaAttachment>
            {
                new Photo()
                {
                    AlbumId = vkStuff.mainAlbumId,
                    OwnerId = -vkStuff.groupId,
                    Id = courses[userMapping.Course].groups[userMapping.GroupIndex]
                        .subgroups[user.Subgroup - 1].PhotoId
                }
            });
            return;
        }

        //! test it
        private void ForTomorrow(UserMapping userMapping, Users.User user)
        {
            const int maxFindStudyingDayTries = 12;

            const string tomorrowIsSundayMessage =
            "Завтра воскресенье, вот расписание на ближайший учебный день";
            const string tomorrowIsNotStudyingDay =
            "Завтра Вы не учитесь, вот расписание на ближайший учебный день";
            const string scheduleForTomorrow = "Расписание на завтра";
            const string yourScheduleIsEmpty = "В Вашем расписании нет учебных дней";

            int week = CurrentWeek();
            int today = (int)DateTime.Now.DayOfWeek;
            string message = null;
            int dayIndex;
            long photoId;

            if (today == 6) // DayOfWeek == 6 - Суббота
            {
                dayIndex = 0;
                week = (week + 1) % 2;
                message = tomorrowIsSundayMessage;
            }
            else if (today == 0) // DayOfWeek == 0 - Воскресенье
            {
                dayIndex = 0;
                week = (week + 1) % 2;
            }
            else
            {
                dayIndex = today;
            }

            int findStudyingDayTries = 0;

            int weekBeforeStudyingDayFinding = week;

            while (!courses[userMapping.Course].groups[userMapping.GroupIndex]
            .subgroups[user.Subgroup - 1].weeks[week].days[dayIndex].IsStudying)
            {
                dayIndex++;
                if (dayIndex == 6)
                {
                    dayIndex = 0;
                    week = (week + 1) % 2;
                }

                findStudyingDayTries++;
                if (findStudyingDayTries == maxFindStudyingDayTries)
                {
                    EnqueueMessage(
                        userId: user.Id,
                        message: yourScheduleIsEmpty);
                    return;
                }
            }
            if (message != null)
            {
                if (dayIndex == today && week == weekBeforeStudyingDayFinding)
                    message = scheduleForTomorrow;
                else
                    message = tomorrowIsNotStudyingDay;
            }

            photoId = courses[userMapping.Course].groups[userMapping.GroupIndex]
            .subgroups[user.Subgroup - 1].weeks[week].days[dayIndex].PhotoId;
            if (photoId == 0)
            {
                DrawingDayScheduleInfo drawingDayScheduleInfo = new DrawingDayScheduleInfo
                {
                    date = relevance.DatesAndUrls.dates[userMapping.Course],
                    day = courses[userMapping.Course].groups[userMapping.GroupIndex]
                    .subgroups[user.Subgroup - 1].weeks[week].days[dayIndex],
                    dayOfWeek = dayIndex,
                    group = user.Group,
                    subgroup = user.Subgroup.ToString(),
                    vkGroupUrl = vkStuff.groupUrl,
                    weekProperties = week
                };

                PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties
                {
                    UploadingSchedule = UploadingSchedule.Day,
                    ToSend = true,
                    AlbumId = vkStuff.mainAlbumId,
                    Course = userMapping.Course,
                    GroupIndex = userMapping.GroupIndex,
                    Day = dayIndex,
                    GroupName = user.Group,
                    Subgroup = user.Subgroup - 1,
                    Week = week,
                    PeerId = user.Id,
                    Message = message
                };
                try
                {
                    photoUploadProperties.Photo =
                        DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                }
                catch
                {
                    EnqueueMessage(
                        userId: user.Id,
                        message: Constants.drawPhotoError);
                    return;
                }
                photosQueue.Enqueue(photoUploadProperties);
                return;
            }
            else
            {
                EnqueueMessage(
                userId: user.Id,
                message: message,
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

        private void ForToday(UserMapping userMapping, Users.User user)
        {
            const string todayIsSunday = "Сегодня воскресенье";
            const string scheduleForToday = "Расписание на сегодня";
            const string todayYouAreNotStudying = "Сегодня Вы не учитесь";

            int week = CurrentWeek();
            int today = (int)DateTime.Now.DayOfWeek;
            if (today == 0)
            {
                EnqueueMessage(
                userId: user.Id,
                message: todayIsSunday);
                return;
            }
            today--;
            if (courses[userMapping.Course].groups[userMapping.GroupIndex].subgroups[user.Subgroup - 1]
            .weeks[week].days[today].IsStudying)
            {
                long photoId = courses[userMapping.Course].groups[userMapping.GroupIndex]
                .subgroups[user.Subgroup - 1].weeks[week].days[today].PhotoId;
                if (photoId == 0)
                {
                    DrawingDayScheduleInfo drawingDayScheduleInfo = new DrawingDayScheduleInfo
                    {
                        date = relevance.DatesAndUrls.dates[userMapping.Course],
                        day = courses[userMapping.Course].groups[userMapping.GroupIndex]
                            .subgroups[user.Subgroup - 1].weeks[week].days[today],
                        dayOfWeek = today,
                        group = user.Group,
                        subgroup = user.Subgroup.ToString(),
                        vkGroupUrl = vkStuff.groupUrl,
                        weekProperties = week
                    };

                    PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties
                    {
                        UploadingSchedule = UploadingSchedule.Day,
                        ToSend = true,
                        AlbumId = vkStuff.mainAlbumId,
                        Course = userMapping.Course,
                        GroupIndex = userMapping.GroupIndex,
                        Day = today,
                        GroupName = user.Group,
                        Subgroup = user.Subgroup - 1,
                        Week = week,
                        PeerId = user.Id,
                        Message = scheduleForToday
                    };
                    try
                    {
                        photoUploadProperties.Photo =
                            DrawingSchedule.DaySchedule.Draw(drawingDayScheduleInfo);
                    }
                    catch
                    {
                        EnqueueMessage(
                            userId: user.Id,
                            message: Constants.drawPhotoError
                        );
                        return;
                    }
                    photosQueue.Enqueue(photoUploadProperties);
                    return;
                }
                EnqueueMessage(
                userId: user.Id,
                message: scheduleForToday,
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
            EnqueueMessage(
            userId: user.Id,
            message: todayYouAreNotStudying);
            return;
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

        public async Task StartRelevanceModule()
        {
            while (true)
            {
                // TODO: При неуспешной загрузке добавляем тег и тут проверяем его, если что пытаемся еще раз скачать

                HtmlDocument htmlDocument = await relevance.DownloadHtmlDocument(Constants.websiteUrl);

                // if parse important information
                DateTime dt = DateTime.Now;
                importantInfo = "От " + dt.ToString() + "\n\n" + relevance.ParseInformation(htmlDocument);
                // if parse schedule
                List<(int, List<int>)> toUpdate = relevance.UpdateDatesAndUrls(htmlDocument);

                if (toUpdate == null || toUpdate.Count == 0)
                {
                    await Task.Delay(Constants.loadWebsiteDelay);
                    continue;
                }

                List<PhotoUploadProperties> photosToUpload = new List<PhotoUploadProperties>();
                List<int> updatingCourses = new List<int>();

                UpdateProperties updateProperties = new UpdateProperties();
                updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.groupUrl;
                updateProperties.photoUploadProperties.AlbumId = vkStuff.mainAlbumId;
                updateProperties.photoUploadProperties.ToSend = true;
                updateProperties.photoUploadProperties.UploadingSchedule = UploadingSchedule.Week;

                for (int i = 0; i < toUpdate.Count; ++i)
                {
                    int courseIndex = toUpdate[i].Item1;

                    List<string> pathsToFile = new List<string>();
                    for (int j = 0; j < relevance.DatesAndUrls.urls[courseIndex].Count; j++)
                        pathsToFile.Add(Path + Constants.defaultDownloadFolder + j.ToString() + '_' + courseIndex.ToString() + IRelevance.defaultFilenameBody);
                    courses[courseIndex].PathsToFile = pathsToFile;

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("Вышло новое расписание от ");
                    stringBuilder.Append(relevance.DatesAndUrls.dates[courseIndex]);
                    stringBuilder.Append(". Ожидайте результата обработки. Возможно дата совпадает с прошлой, но ссылки на расписание на сайте новые.");

                    EnqueueMessage(
                        userIds: userRepository.GetIds(courseIndex, mapper),
                        message: stringBuilder.ToString());

                    if (!await relevance.DownloadScheduleFiles(courseIndex, toUpdate[i].Item2))
                    {
                        courses[courseIndex].isBroken = true;

                        StringBuilder errorMessageBuilder = new StringBuilder();
                        errorMessageBuilder.Append("Не удалось загрузить расписание от ");
                        errorMessageBuilder.Append(relevance.DatesAndUrls.dates[courseIndex]);
                        errorMessageBuilder.Append(". Новое расписание здесь: ");
                        errorMessageBuilder.Append(Constants.websiteUrl);

                        //relevance.DatesAndUrls.dates[courseIndex] = "УСТАРЕЛО";

                        EnqueueMessage(
                            userIds: userRepository.GetIds(courseIndex, mapper),
                            message: errorMessageBuilder.ToString());

                        continue;
                    }

                    updateProperties.drawingStandartScheduleInfo.date = relevance.DatesAndUrls.dates[courseIndex];

                    var coursePhotosToUpload = courses[courseIndex].Update(updateProperties, dictionaries);
                    if (coursePhotosToUpload != null)
                    {
                        photosToUpload.AddRange(coursePhotosToUpload);
                        updatingCourses.Add(courseIndex);
                    }
                }
                if (updatingCourses.Count != 0)
                {
                    mapper.CreateMaps(courses);

                    for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
                    {
                        if (courses[updatingCourses[currentUpdatingCourse]].isBroken)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("Не удалось обработать расписание от ");
                            stringBuilder.Append(relevance.DatesAndUrls.dates[currentUpdatingCourse]);
                            stringBuilder.Append(". Новое расписание здесь: ");
                            stringBuilder.Append(Constants.websiteUrl);

                            EnqueueMessage(
                                userIds: userRepository.GetIds(updatingCourses[currentUpdatingCourse], mapper),
                                message: stringBuilder.ToString());
                        }
                    }

                    for (int photoIndex = 0; photoIndex < photosToUpload.Count; photoIndex++)
                    {
                        if (mapper.TryGetCourseAndGroupIndex(photosToUpload[photoIndex].GroupName, out UserMapping mapping))
                        {
                            photosToUpload[photoIndex].Course = mapping.Course;
                            photosToUpload[photoIndex].GroupIndex = mapping.GroupIndex;

                            photosQueue.Enqueue(photosToUpload[photoIndex]);
                        }
                    }

                    List<(string, int)> newGroupSubgroupList = new List<(string, int)>();
                    for (int currentPhoto = 0; currentPhoto < photosToUpload.Count; currentPhoto++)
                        newGroupSubgroupList.Add((photosToUpload[currentPhoto].GroupName, photosToUpload[currentPhoto].Subgroup + 1));

                    EnqueueMessage(
                        message: "Для Вас изменений нет",
                        userIds: userRepository.GetIds(mapper.GetOldGroupSubgroupList(newGroupSubgroupList, updatingCourses)));

                    CoursesKeyboards = Utils.Utils.ConstructKeyboards(in mapper, CoursesCount);
                    Utils.Utils.SaveCoursesFilePaths(in courses, CoursesCount, Path + Constants.coursesPathsFilename);

                    while (true)
                    {
                        if (photosQueue.IsEmpty)
                        {
                            await Task.Delay(Constants.waitPhotosUploadingDelay);
                            for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
                                courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;
                            break;
                        }
                        await Task.Delay(Constants.checkPhotosQueueDelay);
                    }

                    SaveUploadedSchedule();

                    for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
                        courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;

                    relevance.DatesAndUrls.Save();
                }
                await Task.Delay(Constants.loadWebsiteDelay);
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
                        courses[photo.Course].groups[photo.GroupIndex].subgroups[photo.Subgroup]
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
                        courses[photo.Course].groups[photo.GroupIndex].subgroups[photo.Subgroup].PhotoId
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
    }
}
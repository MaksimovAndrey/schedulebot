using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;
using System.Net;
using System.Xml;
using GemBox.Spreadsheet;
using System.IO;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using VkNet.Categories;
using VkNet.Enums.SafetyEnums;
using System.Drawing;
using System.Text.RegularExpressions;
using VkNet.Model.Keyboard;
using VkNet.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using VkNet.Exception;

namespace schedulebot
{
    public static class Vk
    {
        public static void GetMessages()
        {
            LongPollServerResponse serverResponse = Glob.api.Groups.GetLongPollServer(Const.groupId);
            //LongPollServerResponse serverResponse = Glob.api.Groups.GetLongPollServer(178155012);
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
                Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " Получаю сообщения"); // 3log
                try
                {
                    historyResponse = Glob.api.Groups.GetBotsLongPollHistory(botsLongPollHistoryParams);
                    if (historyResponse == null)
                        continue;
                    botsLongPollHistoryParams.Ts = historyResponse.Ts;
                    if (!historyResponse.Updates.Any())
                        continue;
                    foreach (var update in historyResponse.Updates)
                    {
                        if (update.Type.ToString() == "error")
                        {
                            Thread.Sleep(1);
                            serverResponse = Glob.api.Groups.GetLongPollServer(178155012);
                            botsLongPollHistoryParams.Server = serverResponse.Server;
                            botsLongPollHistoryParams.Key = serverResponse.Key;
                            botsLongPollHistoryParams.Ts = serverResponse.Ts;
                            continue;
                        }
                        else if (update.Type == GroupUpdateType.MessageNew)
                        {
                            MessageResponseAsync(update.Message);
                        }
                    }
                }
                catch (LongPollException exception)
                {
                    if (exception is LongPollOutdateException outdateException)
                        botsLongPollHistoryParams.Ts = outdateException.Ts;
                    else
                    {
                        var server = Glob.api.Groups.GetLongPollServer(178155012);
                        botsLongPollHistoryParams.Ts = server.Ts;
                        botsLongPollHistoryParams.Key = server.Key;
                        botsLongPollHistoryParams.Server = server.Server;
                    }
                }
                catch (Exception e)
                {
                    // todo: long poll ultra rare error
                    Console.WriteLine("Long poll error = " + e);
                }
                Thread.Sleep(1);
            }
        }
        public static void SendMessage(long? userId, bool oneTime = false, string message = "Отправляю клавиатуру", List<MediaAttachment> attachments = null, int keyboardId = 0, bool onlyKeyboard = false, string keyboardSpecial = "", MessageKeyboard customKeyboard = null)
        {
            Random random = new Random();
            Int32 randomId;
            randomId = (Int32)((2 * random.NextDouble() - 1) * Int32.MaxValue);
            MessagesSendParams messagesSendParams = new MessagesSendParams()
            {
                // UserId = userId,
                PeerId = userId,
                Message = message,
                RandomId = randomId
            };
            switch (keyboardId)
            {
                case -1:
                {
                    messagesSendParams.Keyboard = customKeyboard;
                    break;
                }
                case 0:
                {
                    lock (Glob.lockerKeyboards)
                    {
                        messagesSendParams.Keyboard = Const.keyboards[0];
                    }
                    break;
                }
                case 1:
                {
                    lock (Glob.lockerKeyboards)
                    {
                        messagesSendParams.Keyboard = Const.keyboards[1];
                    }
                    messagesSendParams.Attachments = attachments;
                    break;
                }
                case 3:
                {
                    lock (Glob.lockerKeyboards)
                    {
                        messagesSendParams.Keyboard = Const.keyboards[3];
                    }
                    break;
                }
            }
            if (oneTime)
                messagesSendParams.Keyboard.OneTime = true;
            Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
        }
        public static async void MessageResponseAsync(Message message)
        {
            await Task.Run(() =>
            {
                if (message.Payload == null)
                {
                    if (message.PeerId == 133040900)
                    {
                        if (message.Text.IndexOf("Рассылка") == 0)
                        {
                            string temp = message.Text.Substring(message.Text.IndexOf(' ') + 1);
                            string toWhom = temp.Substring(0, temp.IndexOf(' '));
                            temp = temp.Substring(temp.IndexOf(' ') + 1); // сообщение
                            if (toWhom == "всем")
                            {
                                Distribution.ToAll(temp);
                            }
                            else if (toWhom.Length == 1)
                            {
                                int toCourse = -1;
                                int.TryParse(toWhom, out toCourse);
                                if (toCourse != -1)
                                {
                                    Distribution.ToCourse(toCourse, temp);
                                }
                                else
                                {
                                    // TODO: неправильная рассылка
                                }
                            }
                            else
                            {
                                Distribution.ToGroup(toWhom, temp);
                            }
                        }
                        else if (message.Text.IndexOf("Обновить") == 0)
                        {
                            string temp = message.Text.Substring(message.Text.IndexOf(' ') + 1);
                            bool sendUpdates = true;
                            string course = temp.Substring(0, temp.IndexOf(' '));
                            temp = temp.Substring(temp.IndexOf(' ') + 1);
                            if (temp == "нет")
                                sendUpdates = false;
                            if (course == "все")
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
                                    Utils.TomorrowStuding();
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
                                        Utils.TomorrowStuding();
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
                        }
                        else if (message.Text.IndexOf("Перезагрузка") == 0)
                        {
                            // TODO: reboot to apply update without server reboot
                        }
                    }
                    else if (message.Attachments.Count != 0)
                    {
                        if (message.Attachments.Single().ToString() == "Sticker")
                        {
                            SendMessage(userId: message.PeerId, message: "🤡");
                            return;
                        }
                    }
                    else
                    {
                        SendMessage(userId: message.PeerId, message: "Нажмите на кнопку");
                        return;
                    }
                    return;
                }
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
                switch (args[0])
                {
                    case -1: // в случае ошибки
                    {
                        SendMessage(userId: message.PeerId, message: "Что-то пошло не так", onlyKeyboard: false, keyboardId: 0);
                        return;
                    }
                    case 0: // сделать информацию
                    {
                        switch (message.Text)
                        {
                            case "Расписание":
                            {
                                SendMessage(userId: message.PeerId, onlyKeyboard: true, keyboardId: 1);
                                return;
                            }
                            case "Неделя":
                            {
                                SendMessage(userId: message.PeerId, message: Utils.CurrentWeek(), keyboardId: 0);
                                return;
                            }
                            case "Настройки":
                            {
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Const.keyboards[2];
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
                                    customKeyboard: keyboardCustom,
                                    onlyKeyboard: true);
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
                            SendMessage(userId: message.PeerId, onlyKeyboard: true, keyboardId: 0);
                            return;
                        }
                        else if (message.Text == "Ссылка")
                        {
                            MessageKeyboard keyboardCustom;
                            bool contains;
                            lock (Glob.lockerKeyboards)
                            {
                                keyboardCustom = Const.keyboards[2];
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
                                        keyboardCustom = Const.keyboards[2];
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
                                        keyboardCustom = Const.keyboards[2];
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
                                                    study = Glob.tomorrow_studing[mapping.Course, mapping.Index, today, week];
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
                                        keyboardCustom = Const.keyboards[2];
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
                                                        while (!Glob.tomorrow_studing[mapping.Course, mapping.Index, day, week])
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
                                                        while (!Glob.tomorrow_studing[mapping.Course, mapping.Index, day, week])
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
                                                customKeyboard: keyboardCustom,
                                                onlyKeyboard: true);
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
                                keyboardCustom = Const.keyboards[2];
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
                                    keyboardCustom = Const.keyboards[2];
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
                                SendMessage(userId: message.PeerId, onlyKeyboard: true, keyboardId: 3);
                                return;
                            }
                            case "Изменить подгруппу":
                            {
                                bool contains = false;
                                MessageKeyboard keyboardCustom;
                                lock (Glob.lockerKeyboards)
                                {
                                    keyboardCustom = Const.keyboards[2];
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
                                SendMessage(userId: message.PeerId, onlyKeyboard: true, keyboardId: 0);
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
                                    keyboardCustom = Const.keyboards[2];
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
                                    customKeyboard: keyboardCustom,
                                    onlyKeyboard: true);
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
                                customKeyboard = Const.keyboards[4];
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
            });
        }
        public static void UploadPhoto(string path, long albumId, string caption, int course, int number, int? dayOfWeek = null, int? weekProperties = null)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка фотографии"); // log
            bool success = false;
            ulong temp;
            lock (Glob.locker)
            {
                if (dayOfWeek == null)
                    temp = Glob.schedule_uploaded[course, number];
                else
                    temp = Glob.tomorrow_uploaded[course, number, (int)dayOfWeek, (int)weekProperties];
            }
            while (!success)
            {
                try
                {
                    UploadServerInfo uploadServer = Glob.apiPhotos.Photo.GetUploadServer(albumId, 178155012);
                    WebClient webClient = new WebClient();
                    string responseFile = Encoding.ASCII.GetString(webClient.UploadFile(uploadServer.UploadUrl, path));
                    IReadOnlyCollection<Photo> photos = Glob.apiPhotos.Photo.Save(new PhotoSaveParams
                    {
                        SaveFileResponse = responseFile,
                        AlbumId = albumId,
                        Caption = caption,
                        GroupId = 178155012
                    });
                    // Удаление перед загрузкой
                    // if (temp != 0)
                    // {
                    //     Glob.apiPhotos.Photo.Delete(temp, -178155012);
                    //     temp = 0;
                    // }
                    lock (Glob.locker)
                    {
                        if (dayOfWeek == null)
                            Glob.schedule_uploaded[course, number] = (ulong)photos.First().Id;
                        else
                            Glob.tomorrow_uploaded[course, number, (int)dayOfWeek, (int)weekProperties] = (ulong)photos.First().Id;
                    }
                    success = true;
                }
                catch { }
                if (!success)
                {
                    Console.WriteLine("\n" + DateTime.Now.TimeOfDay.ToString() + " [ERROR] При загрузке фотографии возникла ошибка\n"); // log
                    Thread.Sleep(50);
                }
                else
                {
                    Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка фотографии"); // log
                }
            }
        }
        public static void ExecuteMethods()
        {
            int timer = 0;
            int count = 0;
            string code = "";
            int queueCount;
            while (true)
            {
                queueCount = Glob.queueCommands.Count();
                if (queueCount > 25 - count)
                {
                    queueCount = 25 - count;
                }
                for (int i = 0; i < queueCount; ++i)
                {
                    if (Glob.queueCommands.TryDequeue(out string command))
                    {
                        code += command;
                        ++count;
                    }
                    else
                    {
                        --i;
                        timer += 1;
                        Thread.Sleep(1);
                    }
                }
                if ((count == 25 && timer >= 56) || timer >= 200)
                {
                    if (count == 0)
                    {
                        timer = 0;
                    }
                    else
                    {
                        VkResponse vkResponse = Glob.api.Execute.Execute(code); // timeout error
                        timer = 0;
                        count = 0;
                        code = "";
                    }
                }
                timer += 8;
                Thread.Sleep(8);
            }
        }
    }
}
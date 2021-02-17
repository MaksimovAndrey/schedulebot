using Newtonsoft.Json;
using Schedulebot.Mapping.Utils;
using Schedulebot.Users;
using Schedulebot.Users.Enums;
using Schedulebot.Departments.Utils;
using Schedulebot.Utils;
using Schedulebot.Vk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : IDepartment
    {
        private void ResponseMessage(Message message)
        {
            if (message.Payload == null)
            {
                if (message.PeerId == vkStuff.AdminId)
                    AdminMessageResponse(message);
                else if (message.Attachments.Count != 0)
                    AttachmentsMessageResponse(message);
                else
                    TextMessageResponse(message);
            }
            else
            {
                ButtonMessageResponse(message);
            }
        }

        private void AdminMessageResponse(Message message)
        {
            string messageStr = message.Text.ToUpper();

            if (Constants.textHelpCommand.Contains(messageStr))
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
            else if (messageStr == "АПТАЙМ" || messageStr == "UPTIME")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Время запуска: ");
                sb.Append(StartTime.ToString());
                sb.Append("\nАптайм: ");
                sb.Append((DateTime.Now - StartTime).ToString());
                EnqueueMessage(
                    userId: message.PeerId,
                    message: sb.ToString());
                return;
            }
            else if (message.Text.IndexOf("Обновить") == 0 || message.Text.IndexOf("Update") == 0)
            {
                // TODO: update command
                EnqueueMessage(
                    userId: message.PeerId,
                    message: "todo");
                return;
            }
            else if (messageStr == "ПЕРЕЗАГРУЗКА" || messageStr == "REBOOT")
            {
                IUserRepositorySaver.Save(userRepository, Path + Constants.userRepositoryFilename);
                Environment.Exit(0);
            }
        }

        private void ButtonMessageResponse(Message message)
        {
            const string startPayloadCommand = "start";

            string messageStr = message.Text.ToUpper();

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
                            EnqueueMessage(
                                userId: message.PeerId,
                                message: Converter.WeekToString(CurrentWeek()));
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
                    if (messageStr == Constants.backMenuItem)
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            keyboardId: 0);
                    }
                    else if (messageStr == Constants.importantInfoCommand)
                    {
                        ImportantInfoResponse(message.PeerId);
                    }
                    else if(messageStr == Constants.forWeekCommand
                        || messageStr == Constants.forTodayCommand
                        || messageStr == Constants.forTomorrowCommand)
                    {
                        ScheduleMessageResponse(messageStr, message.PeerId, true);
                    }
                    else
                    {
                        EnqueueMessage(
                            userId: message.PeerId,
                            message: Constants.oldKeyboardMessage,
                            keyboardId: 0);
                    }
                    return;
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
                        customKeyboard = vkStuff.MenuKeyboards[5];
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

        private void AttachmentsMessageResponse(Message message)
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

        private void TextMessageResponse(Message message)
        {
            string messageStr = message.Text.ToUpper();

            if (messageStr.IndexOf(Constants.subscribeSign) == 0)
            {
                TextCommandSubscribeResponse(message.Text, message.PeerId);
                return;
            }

            if (Constants.textUnsubscribeCommand.Contains(messageStr))
            {
                ImportantInfoResponse(message.PeerId);
            }
            else if (Constants.textUnsubscribeCommand.Contains(messageStr))
            {
                UnsubscribeResponse(message.PeerId);
            }
            else if (Constants.textSubscribeCommand.Contains(messageStr))
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    attachments: vkStuff.SubscribeInfoAttachments,
                    message: null);
            }
            else if (Constants.textCurrentWeekCommand.Contains(messageStr))
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: Converter.WeekToString(CurrentWeek()));
            }
            else if (Constants.textTodayCommand.Contains(messageStr)
                || Constants.textTomorrowCommand.Contains(messageStr)
                || Constants.textWeekCommand.Contains(messageStr)
                || Constants.textLinkCommand.Contains(messageStr))
            {
                ScheduleMessageResponse(messageStr, message.PeerId);
            }
            else
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    attachments: vkStuff.TextCommandsAttachments,
                    message: null,
                    keyboardId: 0);
            }
        }

        private void SubscribeResponse(long? peerId, string group, int subgroup = 1, bool messageFromKeyboard = false)
        {
            StringBuilder messageBuilder = new StringBuilder();
            switch (userRepository.AddOrEdit(peerId, group, subgroup))
            {
                case AddOrEditResult.Added:
                    messageBuilder.Append("Вы подписались на ");
                    break;
                case AddOrEditResult.Edited:
                    messageBuilder.Append("Вы изменили настройки на ");
                    break;
            }
            messageBuilder.Append(Utils.Utils.ConstructGroupSubgroup(group, subgroup));

            EnqueueMessage(
                userId: peerId,
                message: messageBuilder.ToString(),
                keyboardId: messageFromKeyboard ? (int?)0 : null);
        }

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
                    attachments: vkStuff.SubscribeInfoAttachments,
                    message: null);
            }
        }

        private void TextCommandSubscribeResponse(string message, long? peerId)
        {
            message = message.Substring(message.IndexOf(' ') + 1).Trim();
            if (message.ToUpper().IndexOf("НА ") == 0 && message.Length > 3)
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
                attachments: vkStuff.SubscribeInfoAttachments,
                message: "Некорректный ввод настроек");
        }

        private void UnsubscribeResponse(long? peerId, bool messageFromKeyboard = false)
        {
            const string cantUnsubscribe = "Вы не можете отписаться, так как Вы не подписаны";
            const string unsubscribeSuccess = "Отменена подписка на расписание";

            EnqueueMessage(
                userId: peerId,
                message: userRepository.Delete(peerId) ? unsubscribeSuccess : cantUnsubscribe,
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
            if (!userRepository.Get(peerId, out Users.User user))
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
                        attachments: vkStuff.SubscribeInfoAttachments,
                        message: Constants.unknownUserMessage);
                }
            }
            else if (!mapper.TryGetCourseAndGroupIndex(user.Group, out UserMapping userMapping))
            {
                MessageKeyboard customKeyboard = vkStuff.MenuKeyboards[3];

                customKeyboard.Buttons.First().First().Action.Label =
                    Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: peerId,
                    message: Constants.userGroupUnknownMessage,
                    customKeyboard: customKeyboard);
            }
            else
            {
                while (courses[userMapping.Course].groups[userMapping.GroupIndex].IsUpdating == true)
                {
                    Task.Delay(20).RunSynchronously();
                }
                if (DateTime.Now - courses[userMapping.Course].groups[userMapping.GroupIndex].LastTimeUpdated > TimeSpan.FromMinutes(180))
                {
                    UpdateGroupSchedule(userMapping.Course, userMapping.GroupIndex);
                }
                if (message == Constants.forWeekCommand
                    || Constants.textWeekCommand.Contains(message))
                {
                    ForWeek(userMapping, user);
                }
                else if (message == Constants.forTodayCommand
                    || Constants.textTodayCommand.Contains(message))
                {
                    ForToday(userMapping, user);
                }
                else if (message == Constants.forTomorrowCommand
                    || Constants.textTomorrowCommand.Contains(message))
                {
                    ForTomorrow(userMapping, user);
                }
            }
        }

        private void ChangeSubgroupResponse(long? peerId)
        {
            if (userRepository.ChangeSubgroup(peerId, out Users.User user))
            {
                MessageKeyboard keyboardCustom;
                keyboardCustom = vkStuff.MenuKeyboards[3];
                keyboardCustom.Buttons.First().First().Action.Label =
                    Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: peerId,
                    message: Constants.yourSubgroup + user.Subgroup.ToString(),
                    customKeyboard: keyboardCustom);
            }
            else
            {
                EnqueueMessage(
                    userId: peerId,
                    message: Constants.unknownUserWithPayloadMessage,
                    keyboardId: 2);
            }
        }

        private void SettingsResponse(long? peerId)
        {
            if (userRepository.Get(peerId, out Users.User user))
            {
                MessageKeyboard keyboardCustom = vkStuff.MenuKeyboards[3];
                keyboardCustom.Buttons.First().First().Action.Label =
                     Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: peerId,
                    customKeyboard: keyboardCustom);
            }
            else
            {
                EnqueueMessage(
                    userId: peerId,
                    keyboardId: 2);
            }
        }

        private void ForWeek(UserMapping userMapping, Users.User user)
        {
            StringBuilder str = new StringBuilder();

            str.Append("Обновлено ");
            str.Append(courses[userMapping.Course].groups[userMapping.GroupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm"));
            for (int i = 0; i < courses[userMapping.Course].groups[userMapping.GroupIndex].days.Count; i++)
            {
                str.Append("\n\n");
                str.Append(courses[userMapping.Course].groups[userMapping.GroupIndex].days[i].ToString());
            }

            EnqueueMessage(userId: user.Id, message: str.ToString());
        }

        private void ForTomorrow(UserMapping userMapping, Users.User user)
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);
            string addBeforeMsg = "Завтра ";
            bool tomorrowIsSunday = false;
            if (tomorrow.DayOfWeek == DayOfWeek.Sunday)
            {
                tomorrowIsSunday = true;
                addBeforeMsg = "Завтра воскресение, расписание на ";
                tomorrow = tomorrow.AddDays(1);
            }

            while (tomorrow < DateTime.Today.AddDays(12))
            {
                for (int curDay = 0; curDay < courses[userMapping.Course].groups[userMapping.GroupIndex].days.Count; curDay++)
                {
                    if (courses[userMapping.Course].groups[userMapping.GroupIndex].days[curDay].Date == tomorrow)
                    {
                        EnqueueMessage(
                            userId: user.Id,
                            message: "Обновлено "
                                + courses[userMapping.Course].groups[userMapping.GroupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm")
                                + "\n\n" + addBeforeMsg
                                + courses[userMapping.Course].groups[userMapping.GroupIndex].days[curDay].ToString());
                        return;
                    }
                }
                if (!tomorrowIsSunday)
                    addBeforeMsg = "Завтра Вы не учитесь, расписание на ";
                tomorrow = tomorrow.AddDays(1);
            }
            EnqueueMessage(
                userId: user.Id,
                message: "Нет информации или Вы не учитесь");
            return;
        }

        private void ForToday(UserMapping userMapping, Users.User user)
        {
            DateTime today = DateTime.Today;
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                EnqueueMessage(
                userId: user.Id,
                message: Constants.todayIsSunday);
                return;
            }

            for (int curDay = 0; curDay < courses[userMapping.Course].groups[userMapping.GroupIndex].days.Count; curDay++)
            {
                if (courses[userMapping.Course].groups[userMapping.GroupIndex].days[curDay].Date == today)
                {
                    EnqueueMessage(
                        userId: user.Id,
                        message: "Обновлено "
                            + courses[userMapping.Course].groups[userMapping.GroupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm")
                            + "\n\n" + "Сегодня "
                            + courses[userMapping.Course].groups[userMapping.GroupIndex].days[curDay].ToString());
                    return;
                }
            }

            EnqueueMessage(
                userId: user.Id,
                message: "Нет информации или Вы сегодня не учитесь");
            return;
        }
    }
}

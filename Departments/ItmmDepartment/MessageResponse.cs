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
using VkNet.Model.GroupUpdate;
using Schedulebot.Departments.Enums;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : IDepartment
    {
        private void ResponseMessageEvent(MessageEvent messageEvent)
        {
            EnqueueEventAnswer(messageEvent.EventId, messageEvent.UserId.Value, messageEvent.PeerId.Value);
            ButtonMessageResponse(true, messageEvent: messageEvent);
        }

        private void ResponseMessage(Message message, bool callbackSupported)
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
                ButtonMessageResponse(callbackSupported, message: message);
            }
        }

        private void AdminMessageResponse(Message message)
        {
            string messageStr = message.Text.ToUpper();

            if (Constants.textHelpCommand.Contains(messageStr))
            {
                EnqueueMessage(userId: message.PeerId, message: Constants.adminHelp);
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
                            userIds: userRepository.GetIds(mapper.GetGroupNames(toCourse)),
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

        private void PayloadMessageResponse(PayloadStuff payload, long userId, bool callbackSupported)
        {
            switch (payload.Menu)
            {
                case null:
                {
                    EnqueueMessage(
                        userId: userId,
                        message: Constants.unknownError,
                        keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                    return;
                }
                case 0:
                {
                    switch (payload.Act)
                    {
                        case 1:
                            EnqueueMessage(
                                userId: userId,
                                keyboardId: callbackSupported ? 1 + Constants.keyboardsCount : 1);
                            return;
                        case 2:
                            EnqueueMessage(
                                userId: userId,
                                message: Converter.WeekToString(CurrentWeek()));
                            return;
                        case 3:
                            SettingsResponse(userId, callbackSupported);
                            return;
                        case 4:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.about);
                            return;
                        default:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                    }
                }
                case 1:
                {
                    switch (payload.Act)
                    {
                        case 0:
                            EnqueueMessage(
                                userId: userId,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                        case 1:
                            InfoResponse(userId);
                            return;
                        case 2:
                            ScheduleResponse(ScheduleResponseType.Week, userId, callbackSupported, true);
                            return;
                        case 3:
                            ScheduleResponse(ScheduleResponseType.Today, userId, callbackSupported, true);
                            return;
                        case 4:
                            ScheduleResponse(ScheduleResponseType.Tomorrow, userId, callbackSupported, true);
                            return;
                        default:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                    }
                }
                case 2:
                {
                    switch (payload.Act)
                    {
                        case 0:
                            EnqueueMessage(
                                userId: userId,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                        case 1:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.pressAnotherButton);
                            return;
                        case 2:
                            UnsubscribeResponse(userId, true, callbackSupported);
                            return;
                        case 3:
                            EnqueueMessage(
                                userId: userId,
                                keyboardId: callbackSupported ? 4 + Constants.keyboardsCount : 4);
                            return;
                        case 4:
                            ChangeSubgroupResponse(userId, callbackSupported, true);
                            return;
                        default:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                    }
                }
                case 4:
                {
                    switch (payload.Act)
                    {
                        case 0:
                            SettingsResponse(userId, callbackSupported);
                            return;
                        case 1:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.pressAnotherButton);
                            return;
                        case 2:
                            EnqueueMessage(
                                userId: userId,
                                message: "Выберите группу",
                                customKeyboard: CoursesKeyboards[payload.Course, callbackSupported ? 1 : 0][0]);
                            return;
                        default:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                    }
                }
                case 5:
                {
                    switch (payload.Act)
                    {
                        case 0:
                            EnqueueMessage(
                                userId: userId,
                                customKeyboard: CoursesKeyboards[payload.Course, callbackSupported ? 1 : 0][0]);
                            return;
                        case 1:
                            SubscribeResponse(userId, payload.Group, payload.Subgroup, true, callbackSupported);
                            return;
                        default:
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                    }
                }
                case 40:
                {
                    switch (payload.Act)
                    {
                        case 1:
                        {
                            MessageKeyboard customKeyboard;
                            customKeyboard = vkStuff.MenuKeyboards[callbackSupported ? 5 + Constants.keyboardsCount : 5];
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("{\"menu\":\"5\",\"act\":\"1\",\"course\":\"");
                            stringBuilder.Append(payload.Course);
                            stringBuilder.Append("\",\"group\":\"");
                            stringBuilder.Append(payload.Group);
                            stringBuilder.Append("\",\"subgroup\":\"");
                            customKeyboard.Buttons.First().First().Action.Payload
                                = stringBuilder.ToString() + 1 + "\"}";
                            customKeyboard.Buttons.First().ElementAt(1).Action.Payload
                                = stringBuilder.ToString() + 2 + "\"}";
                            customKeyboard.Buttons.ElementAt(1).First().Action.Payload
                                = "{\"menu\":\"5\",\"act\":\"0\",\"course\":\"" + payload.Course + "\"}";
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.Messages.Menu.chooseSubgroup,
                                customKeyboard: customKeyboard);
                            return;
                        }
                        case 2:
                        {
                            if (payload.Page == 0)
                            {
                                EnqueueMessage(
                                    userId: userId,
                                    keyboardId: callbackSupported ? 4 + Constants.keyboardsCount : 4);
                            }
                            else
                            {
                                EnqueueMessage(
                                    userId: userId,
                                    customKeyboard: CoursesKeyboards[payload.Course, callbackSupported ? 1 : 0][payload.Page - 1]);
                            }
                            return;
                        }
                        case 3:
                        {
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.Messages.Menu.currentPage);
                            return;
                        }
                        case 4:
                        {
                            MessageKeyboard customKeyboard;
                            if (payload.Page == CoursesKeyboards[payload.Course, callbackSupported ? 1 : 0].Count - 1)
                                customKeyboard = CoursesKeyboards[payload.Course, callbackSupported ? 1 : 0][0];
                            else
                                customKeyboard = CoursesKeyboards[payload.Course, callbackSupported ? 1 : 0][payload.Page + 1];
                            EnqueueMessage(
                                userId: userId,
                                customKeyboard: customKeyboard);
                            return;
                        }
                        default:
                        {
                            EnqueueMessage(
                                userId: userId,
                                message: Constants.oldKeyboardMessage,
                                keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                            return;
                        }
                    }
                }
            }
        }

        private void ButtonMessageResponse(bool callbackSupported, Message message = null, MessageEvent messageEvent = null)
        {
            const string startPayloadCommand = "start";

            PayloadStuff payload;
            try
            {
                payload = JsonConvert.DeserializeObject<PayloadStuff>(message != null ? message.Payload : messageEvent.Payload);
            }
            catch
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: Constants.oldKeyboardMessage,
                    keyboardId: callbackSupported ? 0 + Constants.keyboardsCount : 0);
                return;
            }

            if (payload.Command == startPayloadCommand)
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: Constants.startMessage,
                    keyboardId: 0);
                return;
            }

            PayloadMessageResponse(
                payload,
                (message != null ? message.PeerId : messageEvent.PeerId).Value,
                callbackSupported);
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
            string uppercaseMessage = message.Text.ToUpper();

            if (uppercaseMessage.IndexOf(Constants.subscribeSign) == 0)
            {
                TextCommandSubscribeResponse(message.Text, message.PeerId.Value);
            } 
            else if (Constants.textInfoCommand.Contains(uppercaseMessage))
            {
                InfoResponse(message.PeerId.Value);
            }
            else if (Constants.textUnsubscribeCommand.Contains(uppercaseMessage))
            {
                UnsubscribeResponse(message.PeerId.Value);
            }
            else if (Constants.textSubscribeCommand.Contains(uppercaseMessage))
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    attachments: vkStuff.SubscribeInfoAttachments,
                    message: null);
            }
            else if (Constants.textCurrentWeekCommand.Contains(uppercaseMessage))
            {
                EnqueueMessage(
                    userId: message.PeerId,
                    message: Converter.WeekToString(CurrentWeek()));
            }
            else if (Constants.textTodayCommand.Contains(uppercaseMessage))
            {
                ScheduleResponse(ScheduleResponseType.Today, message.PeerId.Value, false);
            }
            else if (Constants.textTomorrowCommand.Contains(uppercaseMessage))
            {
                ScheduleResponse(ScheduleResponseType.Tomorrow, message.PeerId.Value, false);
            }
            else if (Constants.textWeekCommand.Contains(uppercaseMessage))
            {
                ScheduleResponse(ScheduleResponseType.Week, message.PeerId.Value, false);
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

        private void SubscribeResponse(long userId, string group, int subgroup = 1, bool messageFromKeyboard = false, bool callbackSupported = false)
        {
            StringBuilder messageBuilder = new StringBuilder();
            switch (userRepository.AddOrEdit(userId, group, subgroup))
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
                userId: userId,
                message: messageBuilder.ToString(),
                keyboardId: messageFromKeyboard ? (int?)(callbackSupported ? 0 + Constants.keyboardsCount : 0) : null);
        }

        private void SubscribeMessageResponse(long userId, bool messageFromKeyboard = false, bool callbackSupported = false)
        {
            if (messageFromKeyboard)
            {
                EnqueueMessage(
                    userId: userId,
                    keyboardId: callbackSupported ? 4 + Constants.keyboardsCount : 4);
            }
            else
            {
                EnqueueMessage(
                    userId: userId,
                    attachments: vkStuff.SubscribeInfoAttachments,
                    message: null);
            }
        }

        private void TextCommandSubscribeResponse(string message, long userId)
        {
            message = message.Substring(message.IndexOf(' ') + 1).Trim();
            if (message.ToUpper().IndexOf("НА ") == 0 && message.Length > 3)
                message = message.Substring(message.IndexOf(' ') + 1).Trim();

            if (!message.Contains(' '))
            {
                SubscribeResponse(userId, message);
                return;
            }
            if (message.Length == message.IndexOf(' ') + 2
                && int.TryParse(message.Substring(message.Length - 1), out int subgroup)
                && (subgroup == 1 || subgroup == 2))
            {
                SubscribeResponse(userId, message[0..^2], subgroup);
                return;
            }
            EnqueueMessage(
                userId: userId,
                attachments: vkStuff.SubscribeInfoAttachments,
                message: "Некорректный ввод настроек");
        }

        private void UnsubscribeResponse(long userId, bool messageFromKeyboard = false, bool callbackSupported = false)
        {
            const string cantUnsubscribe = "Вы не можете отписаться, так как Вы не подписаны";
            const string unsubscribeSuccess = "Отменена подписка на расписание";

            EnqueueMessage(
                userId: userId,
                message: userRepository.Delete(userId) ? unsubscribeSuccess : cantUnsubscribe,
                keyboardId: messageFromKeyboard ? (int?)(callbackSupported ? (2 + Constants.keyboardsCount) : 2) : null);
        }

        private void InfoResponse(long userId)
        {
            EnqueueMessage(
                userId: userId,
                message: importantInfo);
        }

        private bool CheckUser(long userId, out UserMapping userMapping, bool callbackSupported, bool buttonMessage)
        {
            if (!userRepository.Get(userId, out Users.User user))
            {
                if (buttonMessage)
                {
                    EnqueueMessage(
                        userId: userId,
                        message: Constants.unknownUserWithPayloadMessage,
                        keyboardId: callbackSupported ? 2 + Constants.keyboardsCount : 2);
                }
                else
                {
                    EnqueueMessage(
                        userId: userId,
                        attachments: vkStuff.SubscribeInfoAttachments,
                        message: Constants.unknownUserMessage);
                }
                userMapping = default;
                return false;
            }
            else if (!mapper.TryGetCourseAndGroupIndex(user.Group, out userMapping))
            {
                MessageKeyboard customKeyboard = vkStuff.MenuKeyboards[callbackSupported ? 3 + Constants.keyboardsCount : 3];

                customKeyboard.Buttons.First().First().Action.Label =
                    Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: userId,
                    message: Constants.userGroupUnknownMessage,
                    customKeyboard: customKeyboard);
                return false;
            }
            return true;
        }

        private async Task<bool> CheckUpdates(UserMapping userMapping)
        {
            while (courses[userMapping.Course].groups[userMapping.GroupIndex].IsUpdating == true)
            {
                await Task.Delay(20); // TODO const
            }
            if (DateTime.Now - courses[userMapping.Course].groups[userMapping.GroupIndex].LastTimeUpdated > TimeSpan.FromMinutes(180))
            {
                return UpdateGroupSchedule(userMapping.Course, userMapping.GroupIndex);
            }
            return true;
        }

        private void ChangeSubgroupResponse(long userId, bool callbackSupported, bool buttonMessage = false)
        {
            if (userRepository.ChangeSubgroup(userId, out Users.User user))
            {
                MessageKeyboard keyboardCustom;
                keyboardCustom = vkStuff.MenuKeyboards[callbackSupported ? 3 + Constants.keyboardsCount : 3];
                keyboardCustom.Buttons.First().First().Action.Label =
                    Constants.youAreSubscribed + Utils.Utils.ConstructGroupSubgroup(user.Group, user.Subgroup);

                EnqueueMessage(
                    userId: userId,
                    message: Constants.yourSubgroup + user.Subgroup.ToString(),
                    customKeyboard: keyboardCustom);
            }
            else
            {
                EnqueueMessage(
                    userId: userId,
                    message: Constants.unknownUserWithPayloadMessage,
                    keyboardId: callbackSupported ? 2 + Constants.keyboardsCount : 2);
            }
        }

        private void SettingsResponse(long? peerId, bool callbackSupported)
        {
            if (userRepository.Get(peerId, out Users.User user))
            {
                MessageKeyboard keyboardCustom = vkStuff.MenuKeyboards[callbackSupported ? 3 + Constants.keyboardsCount : 3];
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
                    keyboardId: callbackSupported ? 2 + Constants.keyboardsCount : 2);
            }
        }

        private void ScheduleResponse(ScheduleResponseType type, long userId, bool callbackSupported, bool buttonMessage = false)
        {
            if (!CheckUser(userId, out UserMapping userMapping, callbackSupported, buttonMessage))
                return;

            if (!CheckUpdates(userMapping).Result)
            {
                EnqueueMessage(
                    userId: userId,
                    message: Constants.unnAPIError);
            }

            switch (type)
            {
                case ScheduleResponseType.Today:
                    ForTodayResponse(userId, userMapping.Course, userMapping.GroupIndex);
                    return;
                case ScheduleResponseType.Tomorrow:
                    ForTomorrowResponse(userId, userMapping.Course, userMapping.GroupIndex);
                    return;
                case ScheduleResponseType.Week:
                    ForWeekResponse(userId, userMapping.Course, userMapping.GroupIndex);
                    return;
            }
        }

        private void ForWeekResponse(long userId, int course, int groupIndex)
        {
            StringBuilder msg = new StringBuilder();

            msg.Append("Обновлено ");
            msg.Append(courses[course].groups[groupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm"));
            for (int i = 0; i < courses[course].groups[groupIndex].days.Count; i++)
            {
                msg.Append("\n\n");
                msg.Append(courses[course].groups[groupIndex].days[i].ToString());
            }

            EnqueueMessage(userId: userId, message: msg.ToString());
        }

        private void ForTomorrowResponse(long userId, int course, int groupIndex)
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);
            string addBeforeMsg = Constants.scheduleForTomorrow;
            bool tomorrowIsSunday = false;
            if (tomorrow.DayOfWeek == DayOfWeek.Sunday)
            {
                tomorrowIsSunday = true;
                addBeforeMsg = Constants.tomorrowIsSundayMessage;
                tomorrow = tomorrow.AddDays(1);
            }

            while (tomorrow < DateTime.Today.AddDays(12))
            {
                for (int curDay = 0; curDay < courses[course].groups[groupIndex].days.Count; curDay++)
                {
                    if (courses[course].groups[groupIndex].days[curDay].Date == tomorrow)
                    {
                        DayScheduleResponse(addBeforeMsg, course, groupIndex, curDay, userId);
                        return;
                    }
                }
                if (!tomorrowIsSunday)
                    addBeforeMsg = Constants.tomorrowIsNotStudyingDay;
                tomorrow = tomorrow.AddDays(1);
            }
            EnqueueMessage(
                userId: userId,
                message: "Нет информации или Вы не учитесь");
            return;
        }

        private void ForTodayResponse(long userId, int course, int groupIndex)
        {
            DateTime today = DateTime.Today;
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                EnqueueMessage(
                userId: userId,
                message: Constants.todayIsSunday);
                return;
            }

            for (int curDay = 0; curDay < courses[course].groups[groupIndex].days.Count; curDay++)
            {
                if (courses[course].groups[groupIndex].days[curDay].Date == today)
                {
                    DayScheduleResponse(Constants.scheduleForToday, course, groupIndex, curDay, userId);
                    return;
                }
            }
            EnqueueMessage(
                userId: userId,
                message: "Нет информации или Вы сегодня не учитесь");
            return;
        }

        private void DayScheduleResponse(string message, int course, int groupIndex, int dayIndex, long id)
        {
            bool sendPhoto = true;
            if (courses[course].groups[groupIndex].days[dayIndex].PhotoId == 0
                && !courses[course].groups[groupIndex].days[dayIndex].IsPhotoUploading)
            {
                courses[course].groups[groupIndex].days[dayIndex].IsPhotoUploading = true;
                string groupName = courses[course].groups[groupIndex].Name;

                Drawing.Day.DrawerInfo drawingDayScheduleInfo = new Drawing.Day.DrawerInfo
                {
                    day = courses[course].groups[groupIndex].days[dayIndex],
                    group = groupName
                };

                PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties
                {
                    UploadingSchedule = UploadingSchedule.Day,
                    ToSend = true,
                    AlbumId = vkStuff.MainAlbumId,
                    Course = course,
                    GroupIndex = groupIndex,
                    Day = dayIndex,
                    GroupName = groupName,
                    PeerId = id,
                    Message = "Обновлено "
                        + courses[course].groups[groupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm")
                        + "\n" + message + '\n'
                        + courses[course].groups[groupIndex].days[dayIndex].GetDateString()
                };
                try
                {
                    photoUploadProperties.Photo = Drawing.Day.Drawer.Draw(drawingDayScheduleInfo);
                    photosQueue.Enqueue(photoUploadProperties);
                    return;
                }
                catch
                {
                    courses[course].groups[groupIndex].days[dayIndex].IsPhotoUploading = false;
                    sendPhoto = false;
                }
            }
            else if (courses[course].groups[groupIndex].days[dayIndex].PhotoId == 0)
            {
                // Создаём Task, который ожидает загрузки фотографии и вызывает тот же DayScheduleResponse
                _ = Task.Run(async () =>
                {
                    while (courses[course].groups[groupIndex].days[dayIndex].IsPhotoUploading)
                    {
                        await Task.Delay(Constants.waitPhotoUploadingDelay);
                    }
                    DayScheduleResponse(message, course, groupIndex, dayIndex, id);
                });
                return;
            }
            if (sendPhoto)
            {
                EnqueueMessage(
                    userId: id,
                    message: "Обновлено "
                        + courses[course].groups[groupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm")
                        + "\n" + message + Constants.delimiter
                        + courses[course].groups[groupIndex].days[dayIndex].GetDateString(),
                    attachments: new List<MediaAttachment>
                    {
                    new Photo() { AlbumId = vkStuff.MainAlbumId, OwnerId = -vkStuff.GroupId, Id = courses[course].groups[groupIndex].days[dayIndex].PhotoId }
                    });
            }
            else
            {
                EnqueueMessage(
                    userId: id,
                    message: "Обновлено "
                        + courses[course].groups[groupIndex].LastTimeUpdated.ToString("dd'.'MM'.'yyyy HH:mm")
                        + "\n\n" + message + '\n'
                        + courses[course].groups[groupIndex].days[dayIndex].ToString()
                );
            }
        }


        private async void UploadedPhotoResponse(PhotoUploadProperties photo)
        {
            await Task.Run(() =>
            {
                if (photo.UploadingSchedule == UploadingSchedule.Day)
                {
                    courses[photo.Course].groups[photo.GroupIndex].days[photo.Day].PhotoId = photo.Id;
                    courses[photo.Course].groups[photo.GroupIndex].days[photo.Day].IsPhotoUploading = false;
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
                                    OwnerId = -vkStuff.GroupId,
                                    Id = photo.Id
                                }
                            });
                    }

                    courses[photo.Course].groups[photo.GroupIndex].uploadedDays.Add(
                        courses[photo.Course].groups[photo.GroupIndex].days[photo.Day]);
                }
            });
        }
    }
}

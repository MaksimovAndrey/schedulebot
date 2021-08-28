using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schedulebot.Commands;
using Schedulebot.Parsing;
using Schedulebot.Schedule;
using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using VkNet.Utils;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : Department
    {
        private bool UpdateGroupSchedule(int course, int group)
        {
            courses[course].groups[group].IsUpdating = true;
            DateTime newUpdateTime = DateTime.Now;

            var newSchedule = GetNewSchedule(course, courses[course].groups[group].Name);
            if (newSchedule is null)
                return false;

            courses[course].groups[group].days = newSchedule;
            courses[course].groups[group].CheckForUploadedDays();
            courses[course].groups[group].LastTimeUpdated = newUpdateTime;
            courses[course].groups[group].IsUpdating = false;

            return true;
        }

        private List<ScheduleDay> GetNewSchedule(int course, string groupName)
        {
            string url = Constants.portalAPI + dictionaries[course][groupName]
                + "?start=" + DateTime.Now.ToString("yyyy'.'MM'.'dd")
                + "&finish=" + DateTime.Now.AddDays(8).ToString("yyyy'.'MM'.'dd")
                + Constants.portalAPILangArg;

            string result;
            try
            {
                result = ScheduleBot.client.GetStringAsync(url).Result;
            }
            catch
            {
                return null;
            }

            return Parser.ParseScheduleFromJson(result);
        }

        private void EnqueueEventAnswer(
            string eventId,
            long userId,
            long peerId,
            bool haveEventData = false,
            string text = null,
            MessageEventType messageEventType = null)
        {
            VkParameters vkParameters = new VkParameters
            {
                { "event_id", eventId },
                { "user_id", userId },
                { "peer_id", peerId }
            };

            if (haveEventData)
            {
                vkParameters.Add("event_data",
                    JsonConvert.SerializeObject(new EventData()
                    {
                        Text = text,
                        Type = messageEventType
                    })
                );
            }

            commandsQueue.Enqueue(new Command(CommandType.SendMessageEventAnswer, vkParameters));
        }

        private void EnqueueMessage(
            bool sendAsNewMessage,
            bool editingEnabled,
            long? userId = null,
            List<long> userIds = null,
            string message = null,
            List<MediaAttachment> attachments = null,
            int? keyboardId = null,
            MessageKeyboard customKeyboard = null)
        {
            VkParameters vkParameters = new VkParameters
            {
                { "message", message is null ? Constants.defaultMessage : message },
                { "random_id", (int)DateTime.Now.Ticks }
            };
            CommandType type;

            if (userIds != null)
            {
                if (userIds.Count == 0)
                    return;

                type = CommandType.SendMessage;
                if (sendAsNewMessage)
                    userRepository.RemoveLastMessageId(userIds);

                if (userIds.Count > 100)
                {
                    vkParameters.Add("user_ids", JsonConvert.SerializeObject(userIds.GetRange(0, 100)));
                    userIds.RemoveRange(0, 100);
                    EnqueueMessage(
                        sendAsNewMessage: true,
                        editingEnabled: false,
                        userIds: userIds,
                        message: message,
                        attachments: attachments,
                        keyboardId: keyboardId,
                        customKeyboard: customKeyboard);
                }
                else
                {
                    vkParameters.Add("user_ids", JsonConvert.SerializeObject(userIds));
                }
            }
            else
            {
                long lastMessageId = (sendAsNewMessage || !editingEnabled) ? userRepository.GetAndRemoveLastMessageId(userId.GetValueOrDefault())
                    : userRepository.GetLastMessageId(userId.GetValueOrDefault());

                if (lastMessageId != 0 && !sendAsNewMessage)
                {
                    vkParameters.Add("message_id", lastMessageId);
                    type = CommandType.EditMessage;
                }
                else if (editingEnabled)
                {
                    type = CommandType.SendMessageAndGetMessageId;
                }
                else
                {
                    type = CommandType.SendMessage;
                }
            }

            vkParameters.Add("peer_id", userId);

            if (attachments != null)
            {
                StringBuilder atts = new StringBuilder();
                atts.Append(attachments[0].ToString());
                for (int i = 1; i < attachments.Count; i++)
                {
                    atts.Append(',');
                    atts.Append(attachments[i].ToString());
                }
                vkParameters.Add("attachment", atts.ToString());
            }

            if (keyboardId == null && customKeyboard != null)
            {
                vkParameters.Add("keyboard", JsonConvert.SerializeObject(customKeyboard));
            }
            else if (keyboardId != null)
            {
                vkParameters.Add("keyboard", JsonConvert.SerializeObject(vkStuff.MenuKeyboards[keyboardId.Value]));
            }

            commandsQueue.Enqueue(new Command(type, vkParameters, userId));
        }

        private void ProcessExecutionResponse(VkResponse response)
        {
            if (!response.HasToken())
                return;

            if (response.ContainsKey("userIdsAndLastMessageIds"))
            {
                var jsonArray = JArray.Parse(response["userIdsAndLastMessageIds"].ToString());
                if (jsonArray.Count != 2)
                    return;

                var userIds = ((JArray)jsonArray[0]).ToObject<long[]>();
                var lastMessageIds = ((JArray)jsonArray[1]).ToObject<long[]>();
                if (userIds.Length == 0 || userIds.Length != lastMessageIds.Length)
                    return;

                userRepository.SetLastMessageId(userIds, lastMessageIds);

                Console.WriteLine("TEST + " + userRepository.GetLastMessageId(userIds[0]));
            }
        }

        private int CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            return ((DateTime.Now.DayOfYear - startDay) / 7 + startWeek) % 2;
        }
    }
}

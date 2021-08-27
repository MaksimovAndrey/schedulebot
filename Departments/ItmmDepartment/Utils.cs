using HtmlAgilityPack;
using Newtonsoft.Json;
using Schedulebot.Schedule.Relevance;
using Schedulebot.Mapping;
using Schedulebot.Mapping.Utils;
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
using Schedulebot.Schedule;
using Schedulebot.Parsing;
using VkNet.Utils;
using VkNet.Model.Template;

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
            string evendId,
            long userId,
            long peerId,
            bool haveEventData = false,
            string text = null,
            MessageEventType messageEventType = null)
        {
            VkParameters vkParameters = new VkParameters
            {
                { "event_id", evendId },
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

            commandsQueue.Enqueue("API.messages.sendMessageEventAnswer(" + JsonConvert.SerializeObject(vkParameters) + ");");
        }

        private void EnqueueMessage(
            long? userId = null,
            List<long> userIds = null,
            string message = Constants.defaultMessage,
            List<MediaAttachment> attachments = null,
            int? keyboardId = null,
            MessageKeyboard customKeyboard = null)
        {
            VkParameters vkParameters = new VkParameters
            {
                { "message", message },
                { "random_id", (int)DateTime.Now.Ticks }
            };

            if (userIds != null)
            {
                if (userIds.Count == 0)
                    return;
                else if (userIds.Count > 100)
                {
                    vkParameters.Add("user_ids", JsonConvert.SerializeObject(userIds.GetRange(0, 100)));
                    userIds.RemoveRange(0, 100);
                    EnqueueMessage(
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
            commandsQueue.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(vkParameters) + ");");
        }

        private int CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            return ((DateTime.Now.DayOfYear - startDay) / 7 + startWeek) % 2;
        }
    }
}
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

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : IDepartment
    {
        private bool UpdateGroupSchedule(int course, int group)
        {
            courses[course].groups[group].IsUpdating = true;
            DateTime newUpdateTime = DateTime.Now;
            var newSchedule = GetNewSchedule(course, courses[course].groups[group].Name);
            // TODO: check for changes
            courses[course].groups[group].days = newSchedule;
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

            return Parser.ParseScheduleFromJson(ScheduleBot.client.GetStringAsync(url).Result);
        }

        private void EnqueueMessage(
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
            commandsQueue.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messageSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
        }

//        private bool UploadWeekSchedule()
//        {
//            bool result = false;
//            for (int currentCourse = 0; currentCourse < CoursesCount; currentCourse++)
//            {
//                for (int currentGroup = 0; currentGroup < courses[currentCourse].groups.Count; currentGroup++)
//                {
//                    for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
//                    {
//                        if (courses[currentCourse].groups[currentGroup].subgroups[currentSubgroup].PhotoId == 0)
//                        {
//                            UpdateProperties updateProperties = new UpdateProperties();

//                            updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
//                            updateProperties.drawingStandartScheduleInfo.date
//                                = relevance.DatesAndUrls.dates[currentCourse];
//                            updateProperties.drawingStandartScheduleInfo.weeks
//                                = courses[currentCourse].groups[currentGroup].subgroups[currentSubgroup].weeks;
//                            updateProperties.drawingStandartScheduleInfo.group
//                                = courses[currentCourse].groups[currentGroup].name;

//                            courses[currentCourse].groups[currentGroup]
//                                .DrawSubgroupSchedule(currentSubgroup, ref updateProperties);

//                            updateProperties.photoUploadProperties.GroupName
//                                = courses[currentCourse].groups[currentGroup].name;
//                            updateProperties.photoUploadProperties.AlbumId = vkStuff.MainAlbumId;
//                            updateProperties.photoUploadProperties.Course = currentCourse;
//                            updateProperties.photoUploadProperties.GroupIndex = currentGroup;
//                            updateProperties.photoUploadProperties.ToSend = false;

//#if !DONT_UPLOAD_WEEK_SCHEDULE
//                            photosQueue.Enqueue(new PhotoUploadProperties(updateProperties.photoUploadProperties));
//#endif
//                            result = true;
//                        }
//                    }
//                }
//            }
//            return result;
//        }

        private int CurrentWeek() // Определение недели (верхняя или нижняя)
        {
            return ((DateTime.Now.DayOfYear - startDay) / 7 + startWeek) % 2;
        }
    }
}
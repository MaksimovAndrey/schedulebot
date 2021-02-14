using HtmlAgilityPack;
using Schedulebot.Schedule.Relevance;
using Schedulebot.Mapping.Utils;
using Schedulebot.Users;
using Schedulebot.Vk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : IDepartment
    {
        public void SaveUsers()
        {
            IUserRepositorySaver.Save(userRepository, Path + Constants.userRepositoryFilename);
        }

        private async Task SaveUsersAsync()
        {
            string filename = Path + Constants.userRepositoryFilename;
            while (true)
            {
                await Task.Delay(Constants.saveUsersDelay);
                IUserRepositorySaver.Save(userRepository, filename);
            }
        }

        private Task GetMessages()
        {
            LongPollServerResponse serverResponse = vkStuff.Api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
            BotsLongPollHistoryResponse historyResponse;
            BotsLongPollHistoryParams botsLongPollHistoryParams = new BotsLongPollHistoryParams()
            {
                Server = serverResponse.Server,
                Ts = serverResponse.Ts,
                Key = serverResponse.Key,
                Wait = 25
            };
            while (true)
            {
                try
                {
#if DEBUG
                    Console.WriteLine(DateTime.Now.ToString() + " Получаю сообщения");
                    Console.WriteLine("commandsQueue count = {0}", commandsQueue.Count);
                    Console.WriteLine("messagesQueue count = {0}", messagesQueue.Count);
#endif
                    historyResponse = vkStuff.Api.Groups.GetBotsLongPollHistory(botsLongPollHistoryParams);
                    if (historyResponse == null)
                        continue;
                    botsLongPollHistoryParams.Ts = historyResponse.Ts;
                    if (!historyResponse.Updates.Any())
                        continue;
                    foreach (var update in historyResponse.Updates)
                    {
                        if (update.Type == GroupUpdateType.MessageNew)
                            messagesQueue.Enqueue(update.Message);
                    }
                    historyResponse = null;
                }
                catch (LongPollException exception)
                {
                    if (exception is LongPollOutdateException outdateException)
                        botsLongPollHistoryParams.Ts = outdateException.Ts;
                    else
                    {
                        LongPollServerResponse server = vkStuff.Api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
                        botsLongPollHistoryParams.Ts = server.Ts;
                        botsLongPollHistoryParams.Key = server.Key;
                        botsLongPollHistoryParams.Server = server.Server;
                    }
                }
                catch
                {
                    LongPollServerResponse server = vkStuff.Api.Groups.GetLongPollServer((ulong)vkStuff.GroupId);
                    botsLongPollHistoryParams.Ts = server.Ts;
                    botsLongPollHistoryParams.Key = server.Key;
                    botsLongPollHistoryParams.Server = server.Server;
                }
            }
        }

        private async Task ResponseMessagesAsync()
        {
            Console.WriteLine("ResponseMessagesAsync");
            const int delay = 100;
            while (true)
            {
                while (!messagesQueue.IsEmpty)
                {
                    if (messagesQueue.TryDequeue(out Message message))
                    {
                        ResponseMessage(message);
                    }
                    else
                        await Task.Delay(1);
                }
                await Task.Delay(delay);
            }
        }

        private async Task ExecuteMethodsAsync()
        {
            Console.WriteLine("ExecuteMethodsAsync");
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
                        var response = vkStuff.Api.Execute.Execute(stringBuilder.ToString());
                        timer = 0;
                        commandsInRequestAmount = 0;
                        stringBuilder.Clear();
                    }
                }
                timer += 20;
                await Task.Delay(20);
            }
        }

        //private async Task StartRelevanceModule()
        //{
            //while (true)
            //{
            //    HtmlDocument htmlDocument = await relevance.DownloadHtmlDocument(Constants.websiteUrl);

            //    DateTime dt = DateTime.Now;
            //    importantInfo = "От " + dt.ToString() + "\n\n" + relevance.ParseInformation(htmlDocument);

            //    List<(int, List<int>)> toUpdate = relevance.UpdateDatesAndUrls(htmlDocument);

            //    if (toUpdate == null || toUpdate.Count == 0)
            //    {
            //        await Task.Delay(Constants.loadWebsiteDelay);
            //        continue;
            //    }

            //    List<PhotoUploadProperties> photosToUpload = new List<PhotoUploadProperties>();
            //    List<int> updatingCourses = new List<int>();

            //    UpdateProperties updateProperties = new UpdateProperties();
            //    updateProperties.drawingStandartScheduleInfo.vkGroupUrl = vkStuff.GroupUrl;
            //    updateProperties.photoUploadProperties.AlbumId = vkStuff.MainAlbumId;
            //    updateProperties.photoUploadProperties.ToSend = true;
            //    updateProperties.photoUploadProperties.UploadingSchedule = UploadingSchedule.Week;

            //    for (int i = 0; i < toUpdate.Count; ++i)
            //    {
            //        int courseIndex = toUpdate[i].Item1;

            //        List<string> pathsToFile = new List<string>();
            //        for (int j = 0; j < relevance.DatesAndUrls.urls[courseIndex].Count; j++)
            //            pathsToFile.Add(Path + Constants.defaultDownloadFolder + j.ToString() + '_' + courseIndex.ToString() + IRelevance.defaultFilenameBody);
            //        courses[courseIndex].PathsToFile = pathsToFile;

            //        StringBuilder stringBuilder = new StringBuilder();
            //        stringBuilder.Append(Constants.newSchedule);
            //        stringBuilder.Append(relevance.DatesAndUrls.dates[courseIndex]);
            //        stringBuilder.Append(Constants.waitScheduleUpdatingResult);

            //        EnqueueMessage(
            //            userIds: userRepository.GetIds(courseIndex, mapper),
            //            message: stringBuilder.ToString());

            //        if (!await relevance.DownloadScheduleFiles(courseIndex, toUpdate[i].Item2))
            //        {
            //            courses[courseIndex].isBroken = true;

            //            StringBuilder errorMessageBuilder = new StringBuilder();
            //            errorMessageBuilder.Append(Constants.loadScheduleError);
            //            errorMessageBuilder.Append(relevance.DatesAndUrls.dates[courseIndex]);
            //            errorMessageBuilder.Append(Constants.newScheduleHere);
            //            errorMessageBuilder.Append(Constants.websiteUrl);

            //            EnqueueMessage(
            //                userIds: userRepository.GetIds(courseIndex, mapper),
            //                message: errorMessageBuilder.ToString());

            //            continue;
            //        }

            //        updateProperties.drawingStandartScheduleInfo.date = relevance.DatesAndUrls.dates[courseIndex];

            //        var coursePhotosToUpload = courses[courseIndex].Update(updateProperties, dictionaries);
            //        if (coursePhotosToUpload != null)
            //        {
            //            photosToUpload.AddRange(coursePhotosToUpload);
            //            updatingCourses.Add(courseIndex);
            //        }
            //    }
            //    if (updatingCourses.Count != 0)
            //    {
            //        mapper.CreateMaps(courses);

            //        for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
            //        {
            //            if (courses[updatingCourses[currentUpdatingCourse]].isBroken)
            //            {
            //                StringBuilder stringBuilder = new StringBuilder();
            //                stringBuilder.Append(Constants.updateScheduleError);
            //                stringBuilder.Append(relevance.DatesAndUrls.dates[currentUpdatingCourse]);
            //                stringBuilder.Append(Constants.newScheduleHere);
            //                stringBuilder.Append(Constants.websiteUrl);

            //                EnqueueMessage(
            //                    userIds: userRepository.GetIds(updatingCourses[currentUpdatingCourse], mapper),
            //                    message: stringBuilder.ToString());
            //            }
            //        }

            //        for (int photoIndex = 0; photoIndex < photosToUpload.Count; photoIndex++)
            //        {
            //            if (mapper.TryGetCourseAndGroupIndex(photosToUpload[photoIndex].GroupName, out UserMapping mapping))
            //            {
            //                photosToUpload[photoIndex].Course = mapping.Course;
            //                photosToUpload[photoIndex].GroupIndex = mapping.GroupIndex;

            //                photosQueue.Enqueue(photosToUpload[photoIndex]);
            //            }
            //        }

            //        List<(string, int)> newGroupSubgroupList = new List<(string, int)>();
            //        for (int currentPhoto = 0; currentPhoto < photosToUpload.Count; currentPhoto++)
            //            newGroupSubgroupList.Add((photosToUpload[currentPhoto].GroupName, photosToUpload[currentPhoto].Subgroup + 1));

            //        EnqueueMessage(
            //            message: Constants.noChanges,
            //            userIds: userRepository.GetIds(mapper.GetOldGroupSubgroupList(newGroupSubgroupList, updatingCourses)));

            //        CoursesKeyboards = Utils.Utils.ConstructKeyboards(in mapper, CoursesCount);
            //        Utils.Utils.SaveCoursesFilePaths(in courses, CoursesCount, Path + Constants.coursesPathsFilename);

            //        while (true)
            //        {
            //            if (photosQueue.IsEmpty)
            //            {
            //                await Task.Delay(Constants.waitPhotosUploadingDelay);
            //                for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
            //                    courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;
            //                break;
            //            }
            //            await Task.Delay(Constants.checkPhotosQueueDelay);
            //        }

            //        SaveUploadedSchedule(Path + Constants.uploadedScheduleFilename);

            //        for (int currentUpdatingCourse = 0; currentUpdatingCourse < updatingCourses.Count; currentUpdatingCourse++)
            //            courses[updatingCourses[currentUpdatingCourse]].isUpdating = false;

            //        relevance.DatesAndUrls.Save();
            //    }
            //    await Task.Delay(Constants.loadWebsiteDelay);
            //}
        //}

        /// <summary>
        /// !!! НЕ РАБОТАЕТ !!!
        /// </summary>
        /// <returns></returns>
        //private async Task UploadPhotosAsync()
        //{
        //    int queuePhotosAmount;
        //    int photosInRequestAmount = 0;
        //    int timer = 0;
        //    MultipartFormDataContent form = new MultipartFormDataContent();
        //    List<PhotoUploadProperties> photosUploadProperties = new List<PhotoUploadProperties>();
        //    while (true)
        //    {
        //        queuePhotosAmount = photosQueue.Count;
        //        if (queuePhotosAmount > 5 - photosInRequestAmount)
        //        {
        //            queuePhotosAmount = 5 - photosInRequestAmount;
        //        }
        //        for (int i = 0; i < queuePhotosAmount; ++i)
        //        {
        //            if (photosQueue.TryDequeue(out PhotoUploadProperties photoUploadProperties))
        //            {
        //                photosUploadProperties.Add(photoUploadProperties);
        //                form.Add(new ByteArrayContent(photosUploadProperties[i].Photo), "file" + i.ToString(), i.ToString() + ".png");
        //                ++photosInRequestAmount;
        //            }
        //            else
        //            {
        //                --i;
        //                timer += 1;
        //                await Task.Delay(1);
        //            }
        //        }
        //        if (photosInRequestAmount == 5 || timer >= 333)
        //        {
        //            if (photosInRequestAmount == 0)
        //            {
        //                timer = 0;
        //            }
        //            else
        //            {
        //                bool success = false;
        //                HttpResponseMessage response;
        //                while (!success)
        //                {
        //                    try
        //                    {
        //                        var uploadServer = vkStuff.ApiPhotos.Photo.GetUploadServer(vkStuff.MainAlbumId, vkStuff.GroupId);
        //                        response = null;
        //                        response = await ScheduleBot.client.PostAsync(new Uri(uploadServer.UploadUrl), form);
        //                        if (response != null)
        //                        {
        //                            IReadOnlyCollection<Photo> photos = vkStuff.ApiPhotos.Photo.Save(new PhotoSaveParams
        //                            {
        //                                SaveFileResponse = Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync()),
        //                                AlbumId = vkStuff.MainAlbumId,
        //                                GroupId = vkStuff.GroupId
        //                            });
        //                            if (photos.Count == photosInRequestAmount)
        //                            {
        //                                for (int currentPhoto = 0; currentPhoto < photosInRequestAmount; currentPhoto++)
        //                                {
        //                                    photosUploadProperties[currentPhoto].Id = (long)photos.ElementAt(currentPhoto).Id;
        //                                    UploadedPhotoResponse(photosUploadProperties[currentPhoto]);
        //                                }
        //                                success = true;
        //                            }
        //                            else
        //                            {
        //                                await Task.Delay(1000);
        //                            }
        //                        }
        //                    }
        //                    catch
        //                    {
        //                        await Task.Delay(1000);
        //                    }
        //                }
        //                timer = 0;
        //                photosInRequestAmount = 0;
        //                form.Dispose();
        //                form = new MultipartFormDataContent();
        //                photosUploadProperties.Clear();
        //            }
        //        }
        //        timer += 333;
        //        await Task.Delay(333);
        //    }
        //}
    }
}

using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schedulebot.Commands;
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
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : Department
    {
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
                    Console.WriteLine("messagesQueue count = {0}", updatesQueue.Count);
                    Console.WriteLine("photosQueue count = {0}", photosQueue.Count);
#endif
                    historyResponse = vkStuff.Api.Groups.GetBotsLongPollHistory(botsLongPollHistoryParams);
                    if (historyResponse == null)
                        continue;
                    botsLongPollHistoryParams.Ts = historyResponse.Ts;
                    if (!historyResponse.Updates.Any())
                        continue;
                    for (int i = 0; i < historyResponse.Updates.Count(); i++)
                    {
                        updatesQueue.Enqueue(historyResponse.Updates.ElementAt(i));
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

        private Task CreateResponseMessagesTasks(int tasksCount)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < tasksCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (true)
                    {
                        while (!updatesQueue.IsEmpty)
                        {
                            if (updatesQueue.TryDequeue(out GroupUpdate update))
                            {
                                if (update.Type == GroupUpdateType.MessageNew)
                                {
                                    ResponseMessage(update.MessageNew.Message,
                                        update.MessageNew.ClientInfo.ButtonActions.Contains(KeyboardButtonActionType.Callback));
                                }
                                else if (update.Type == GroupUpdateType.MessageEvent)
                                {
                                    ResponseMessageEvent(update.MessageEvent);
                                }
                            }
                            else
                            {
                                await Task.Delay(1);
                            }
                        }
                        await Task.Delay(Constants.noMessagesDelay);
                    }
                }));
            }
            return Task.WhenAny(tasks);
        }

        private async Task ExecuteMethodsAsync()
        {
            int queueCommandsAmount;
            int commandsInRequestAmount = 0;
            int timer = 0;
            StringBuilder executeCode = new StringBuilder();
            StringBuilder codePartReturn = new StringBuilder();
            List<long> returnMessageUserIds = new List<long>();
            while (true)
            {
                queueCommandsAmount
                = commandsQueue.Count <= 25 - commandsInRequestAmount
                ? commandsQueue.Count : 25 - commandsInRequestAmount;
                for (int i = 0; i < queueCommandsAmount; ++i)
                {
                    if (commandsQueue.TryDequeue(out Command command))
                    {
                        switch (command.Type)
                        {
                            case CommandType.SendMessage:
                                executeCode.Append("API.messages.send(");
                                executeCode.Append(JsonConvert.SerializeObject(command.VkParameters));
                                executeCode.Append(");");
                                break;
                            case CommandType.SendMessageEventAnswer:
                                executeCode.Append("API.messages.sendMessageEventAnswer(");
                                executeCode.Append(JsonConvert.SerializeObject(command.VkParameters));
                                executeCode.Append(");");
                                break;
                            case CommandType.SendMessageAndGetMessageId:
                                returnMessageUserIds.Add(command.UserId.GetValueOrDefault());
                                codePartReturn.Append("ids.push(");
                                codePartReturn.Append("API.messages.send(");
                                codePartReturn.Append(JsonConvert.SerializeObject(command.VkParameters));
                                codePartReturn.Append("));");
                                break;
                            case CommandType.EditMessage:
                                executeCode.Append("API.messages.edit(");
                                executeCode.Append(JsonConvert.SerializeObject(command.VkParameters));
                                executeCode.Append(");");
                                break;
                        }
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
                        if (codePartReturn.Length > 0)
                        {
                            executeCode.Append("var ids = [];");
                            executeCode.Append(codePartReturn.ToString());
                            executeCode.Append("return {\"userIdsAndLastMessageIds\":[[");
                            executeCode.Append(String.Join(", ", returnMessageUserIds.ToArray()));
                            executeCode.Append("],ids]");
                            executeCode.Append("};");
                        }
#if DEBUG
                        Console.WriteLine(executeCode.ToString());
#endif
                        var response = vkStuff.Api.Execute.Execute(executeCode.ToString());

                        ProcessExecutionResponse(response);

                        timer = 0;
                        commandsInRequestAmount = 0;
                        executeCode.Clear();
                        codePartReturn.Clear();
                        returnMessageUserIds.Clear();
                    }
                }
                timer += 20;
                await Task.Delay(20);
            }
        }

        private async Task StartRelevanceModule()
        {
            HtmlDocument htmlDocument;
            while (true)
            {
                htmlDocument = await relevance.DownloadHtmlDocument(Constants.websiteUrl);

                DateTime dt = DateTime.Now;
                importantInfo = "От " + dt.ToString() + "\n\n" + relevance.ParseInformation(htmlDocument);

                htmlDocument = null;
                await Task.Delay(Constants.loadWebsiteDelay);
            }
        }

        private async Task UploadPhotosAsync()
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
                                var uploadServer = vkStuff.ApiPhotos.Photo.GetUploadServer(vkStuff.MainAlbumId, vkStuff.GroupId);
                                response = null;
                                response = await ScheduleBot.client.PostAsync(new Uri(uploadServer.UploadUrl), form);
                                if (response != null)
                                {
                                    IReadOnlyCollection<Photo> photos = vkStuff.ApiPhotos.Photo.Save(new PhotoSaveParams
                                    {
                                        SaveFileResponse = Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync()),
                                        AlbumId = vkStuff.MainAlbumId,
                                        GroupId = vkStuff.GroupId
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

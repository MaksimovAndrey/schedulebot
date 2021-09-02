using System.Collections.Generic;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;

namespace Schedulebot.Vk
{
    public class VkStuff
    {
        public VkApi Api { get; }
        public VkApi ApiPhotos { get; }
        public long GroupId { get; }
        public long MainAlbumId { get; }
        public long AdminId { get; }
        public string GroupUrl { get; }

        private Photo TextCommands { get; }
        private Document SubscribeInfo { get; }

        public List<MediaAttachment> TextCommandsAttachments { get; }
        public List<MediaAttachment> SubscribeInfoAttachments { get; }

        public MessageKeyboard[] MenuKeyboards { get; }
        public MessageKeyboard[] InlineKeyboards { get; }

        public VkStuff(string key, string keyPhotos, long groupId, long mainAlbumId, long adminId,
            string groupUrl, Photo textCommandsInfo, Document subscribeInfo)
        {
            this.Api = new VkApi()
            {
                RequestsPerSecond = 20
            };
            this.Api.Authorize(new ApiAuthParams() { AccessToken = key });

            this.ApiPhotos = new VkApi();
            this.ApiPhotos.Authorize(new ApiAuthParams() { AccessToken = keyPhotos });

            this.GroupId = groupId;
            this.MainAlbumId = mainAlbumId;
            this.AdminId = adminId;
            this.GroupUrl = groupUrl;

            this.TextCommands = textCommandsInfo;
            this.SubscribeInfo = subscribeInfo;

            this.TextCommandsAttachments = new List<MediaAttachment>() { this.TextCommands };
            this.SubscribeInfoAttachments = new List<MediaAttachment>() { this.SubscribeInfo };

            this.InlineKeyboards = new MessageKeyboard[1];
            this.InlineKeyboards[0] = new KeyboardBuilder(isOneTime: false)
                .SetInline(true)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.openUNN,
                    Link = Constants.unnScheduleUri,
                    Type = KeyboardButtonActionType.OpenLink,
                    Payload = "{\"inline\":\"0\",\"act\":\"-1\"}"
                })
                .Build();
            
            this.MenuKeyboards = new MessageKeyboard[12];

            this.MenuKeyboards[0] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.schedule,
                    Payload = "{\"menu\":\"0\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.info,
                    Payload = "{\"menu\":\"0\",\"act\":\"2\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.settings,
                    Payload = "{\"menu\":\"0\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.about,
                    Payload = "{\"menu\":\"0\",\"act\":\"4\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[1] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.weekSchedule,
                    Payload = "{\"menu\":\"1\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.todaySchedule,
                    Payload = "{\"menu\":\"1\",\"act\":\"2\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.tomorrowSchedule,
                    Payload = "{\"menu\":\"1\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"1\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[2] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.youAreNotSubscribed,
                    Payload = "{\"menu\":\"2\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.subscribe,
                    Payload = "{\"menu\":\"2\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Positive)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"2\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[3] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    //Label = "",
                    Payload = "{\"menu\":\"2\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.unsubscribe,
                    Payload = "{\"menu\":\"2\",\"act\":\"2\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Negative)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.resubscribe,
                    Payload = "{\"menu\":\"2\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Positive)
                .AddLine()
                /* Отключаем кнопку изменения подгруппы
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.changeSubgroup,
                    Payload = "{\"menu\":\"2\",\"act\":\"4\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                */
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"2\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[4] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.chooseCourse,
                    Payload = "{\"menu\":\"4\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.first,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"0\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.second,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.third,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"2\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.fourth,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"3\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Primary)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"4\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[5] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.first,
                    Payload = "{\"menu\":\"5\",\"act\":\"1\",\"subgroup\":\"1\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.second,
                    Payload = "{\"menu\":\"5\",\"act\":\"1\",\"subgroup\":\"2\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Primary)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"5\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Text
                }, KeyboardButtonColor.Default)
                .Build();

            // with callbacks
            this.MenuKeyboards[6] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.schedule,
                    Payload = "{\"menu\":\"0\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.info,
                    Payload = "{\"menu\":\"0\",\"act\":\"2\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.settings,
                    Payload = "{\"menu\":\"0\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.about,
                    Payload = "{\"menu\":\"0\",\"act\":\"4\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[7] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.openUNN,
                    Link = Constants.unnScheduleUri,
                    Type = KeyboardButtonActionType.OpenLink,
                    Payload = "{\"menu\":\"1\",\"act\":\"-1\"}"
                })
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.weekSchedule,
                    Payload = "{\"menu\":\"1\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.todaySchedule,
                    Payload = "{\"menu\":\"1\",\"act\":\"2\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.tomorrowSchedule,
                    Payload = "{\"menu\":\"1\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"1\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[8] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.youAreNotSubscribed,
                    Payload = "{\"menu\":\"2\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.subscribe,
                    Payload = "{\"menu\":\"2\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Positive)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"2\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[9] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    //Label = "",
                    Payload = "{\"menu\":\"2\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.unsubscribe,
                    Payload = "{\"menu\":\"2\",\"act\":\"2\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Negative)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.resubscribe,
                    Payload = "{\"menu\":\"2\",\"act\":\"3\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Positive)
                .AddLine()
                /* Отключаем кнопку изменения подгруппы
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.changeSubgroup,
                    Payload = "{\"menu\":\"2\",\"act\":\"4\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                */
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"2\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[10] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.chooseCourse,
                    Payload = "{\"menu\":\"4\",\"act\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.first,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"0\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.second,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.third,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"2\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.fourth,
                    Payload = "{\"menu\":\"4\",\"act\":\"2\",\"course\":\"3\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Primary)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"4\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .Build();

            this.MenuKeyboards[11] = new KeyboardBuilder(isOneTime: false)
                .SetInline(false)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.first,
                    Payload = "{\"menu\":\"5\",\"act\":\"1\",\"subgroup\":\"1\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Primary)
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.Courses.second,
                    Payload = "{\"menu\":\"5\",\"act\":\"1\",\"subgroup\":\"2\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Primary)
                .AddLine()
                .AddButton(new MessageKeyboardButtonAction()
                {
                    Label = Constants.Labels.back,
                    Payload = "{\"menu\":\"5\",\"act\":\"0\"}",
                    Type = KeyboardButtonActionType.Callback
                }, KeyboardButtonColor.Default)
                .Build();
        }
    }
}

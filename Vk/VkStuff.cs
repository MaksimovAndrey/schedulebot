using System;
using System.Collections.Generic;
using System.IO;
using VkNet;
using VkNet.Enums;
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

            this.MenuKeyboards = new MessageKeyboard[6]
            {
                // main
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Расписание",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Неделя",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Настройки",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Информация",
                                    Payload = "{\"menu\": \"0\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // schedule
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Важная информация",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "На неделю",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "На сегодня",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "На завтра",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Ссылка",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"1\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // настройки когда НЕ подписан
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Вы не подписаны",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Positive,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Подписаться",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                 // настройки когда подписан
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Negative,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Отписаться",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Positive,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Переподписаться",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Изменить подгруппу",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"2\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // выбор курса
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Выберите курс",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "1",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "2",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "3",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "4",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"4\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                },
                // выбор подгруппы
                new MessageKeyboard
                {
                    Buttons = new List<List<MessageKeyboardButton>>
                    {
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "1",
                                    Payload = "{\"menu\": \"5\"}"
                                }
                            },
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Primary,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "2",
                                    Payload = "{\"menu\": \"5\"}"
                                }
                            }
                        },
                        new List<MessageKeyboardButton> {
                            new MessageKeyboardButton() {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction {
                                    Type = KeyboardButtonActionType.Text,
                                    Label = "Назад",
                                    Payload = "{\"menu\": \"5\"}"
                                }
                            }
                        }
                    },
                    OneTime = false
                }
            };
        }
    }
}
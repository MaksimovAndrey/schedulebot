using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using VkNet;
using VkNet.Categories;
using VkNet.Utils;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using VkNet.Enums;
using VkNet.Enums.SafetyEnums;

namespace schedulebot
{
    public class NeZnau
    {
        public static MessageKeyboard[] keyboards = new MessageKeyboard[5]
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
            // settings
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
                                Payload = "{\"menu\": \"3\"}"
                            }
                        }
                    },
                    new List<MessageKeyboardButton> {
                        new MessageKeyboardButton() {
                            Color = KeyboardButtonColor.Primary,
                            Action = new MessageKeyboardButtonAction {
                                Type = KeyboardButtonActionType.Text,
                                Label = "1",
                                Payload = "{\"menu\": \"3\"}"
                            }
                        },
                        new MessageKeyboardButton() {
                            Color = KeyboardButtonColor.Primary,
                            Action = new MessageKeyboardButtonAction {
                                Type = KeyboardButtonActionType.Text,
                                Label = "2",
                                Payload = "{\"menu\": \"3\"}"
                            }
                        },
                        new MessageKeyboardButton() {
                            Color = KeyboardButtonColor.Primary,
                            Action = new MessageKeyboardButtonAction {
                                Type = KeyboardButtonActionType.Text,
                                Label = "3",
                                Payload = "{\"menu\": \"3\"}"
                            }
                        },
                        new MessageKeyboardButton() {
                            Color = KeyboardButtonColor.Primary,
                            Action = new MessageKeyboardButtonAction {
                                Type = KeyboardButtonActionType.Text,
                                Label = "4",
                                Payload = "{\"menu\": \"3\"}"
                            }
                        }
                    },
                    new List<MessageKeyboardButton> {
                        new MessageKeyboardButton() {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction {
                                Type = KeyboardButtonActionType.Text,
                                Label = "Назад",
                                Payload = "{\"menu\": \"3\"}"
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
            }
        };
        // кнопки
        public const int buttons_in_line = 2; // 1..4
        public const int lines_in_keyboard = 4; // 1..9
    }
}
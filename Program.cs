using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
//using HtmlAgilityPack;
using System.Net;
//using System.Xml;
//using GemBox.Spreadsheet;
//using System.IO;
using VkNet;
//using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
//using VkNet.Model.Attachments;
//using VkNet.Categories;
using VkNet.Enums.SafetyEnums;
//using System.Text.RegularExpressions;
using VkNet.Model.Keyboard;
using VkNet.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace schedulebot
{
    public class User : IEquatable<User>
    {
        public string Group = null;
        public string Subgroup = null;

        public User() { }
        public User(string g, string s) { Group = g; Subgroup = s; }

        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            return other != null && Group == other.Group && Subgroup == other.Subgroup;
        }

        public override int GetHashCode()
        {
            var hashCode = 390074312;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Group);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Subgroup);
            return hashCode;
        }
    }
    public struct Mapping : IEquatable<Mapping>
    {
        public int Course;
        public int Index;

        public override bool Equals(object obj)
        {
            return obj is Mapping && Equals((Mapping)obj);
        }

        public bool Equals(Mapping other)
        {
            return Course == other.Course && Index == other.Index;
        }

        public override int GetHashCode()
        {
            var hashCode = -1145404541;
            hashCode = hashCode * -1521134295 + Course.GetHashCode();
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            return hashCode;
        }
    }
    public struct Text
    {
        public Font font;
        public Brush brush;
        public int indent;
        public float fix;
    }
    public static class Const
    {
        // пути
        public const string path_linux = @"/media/projects/";
        // public const string path_linux = @"C:/Custom/Projects/Shared/";
        public const string path_config = path_linux + @"schedulebot/config/";
        public const string path_schedule = path_linux + @"schedulebot/config/schedule/";
        public const string path_images = path_linux + @"schedulebot/config/images/";
        public const string path_downloads = path_linux + @"schedulebot/downloads/";
        public const string path_manual = path_linux + @"schedulebot/config/manualProcessing/";
        // семестр
        public static int start_day = 245; // день начала семестра или ранее
        public static int start_week = 0; // неделя в начале семестра //!ВСЕГДА НИЖНЯЯ
        // вк
        public static string key = "";
        public static string keyPhotos = "";
        public static ulong groupId = 178155012; //! почти не используется, в коде константа
        public static long mainAlbumId = 260528652;
        public static long tomorrowAlbumId = 264876124;
        // клавиатуры
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
        // размер изображения
        public const int image_width = 700; // ширина
        public const int image_height = 3000; // высота
        // содержимое
        public static Color l_color = Color.FromArgb(255, 211, 211, 211); // цвет линий
        public static Brush l_brush = new SolidBrush(l_color);// кисть линий
        public const int border_size = 8; // ширина границы
        public const int l1_size = 4; // ширина линии между парами
        public const int l2_size = 2; // ширина линий между верхней и нижней
        public const int indicator_size = 2; // ширина индикатора
        public static Color indicator_color = Color.FromArgb(255, 112, 112, 112); // цвет индикатора
        public static Brush indicator_brush = new SolidBrush(indicator_color); // кисть индикатора
        public static Text header = new Text
        {
            brush = new SolidBrush(Color.Black),
            font = new Font("TT Commons Light Italic", 18),
            indent = 8,
            fix = 2
            // brush = new SolidBrush(Color.Black),
            // font = new Font("TT Commons Medium Italic", 18),
            // indent = 6,
            // fix = 2
        };
        public static Text day = new Text
        {
            brush = new SolidBrush(Color.Black),
            font = new Font("TT Commons Light Italic", 18),
            indent = 8,
            fix = 2
            // brush = new SolidBrush(Color.Black),
            // font = new Font("TT Commons Italic", 18),
            // indent = 8,
            // fix = 2
        };
        public static Text lesson = new Text
        {
            brush = new SolidBrush(Color.Black),
            font = new Font("TT Commons Light Italic", 18),
            indent = 8,
            fix = 2
        };
        public static Text lesson_solo = new Text
        {
            brush = new SolidBrush(Color.Black),
            font = new Font("TT Commons Light Italic", 24),
            indent = 12,
            fix = 3
        };
        public static Text time = new Text
        {
            brush = new SolidBrush(Color.Black),
            font = new Font("TT Commons Light Italic", 18),
            indent = 11,
            fix = 6
        };
        public static Color background_color = Color.White; // цвет фона
        // отступы
        public const int same_lessons = 6; // сдвиг вверх и вниз при одинаковых парах по верхним и нижним неделям
        public const int string_height = 40; // высота строки
        public const int time_width = 120; // ширина времени пар
        public const int lesson_fix = 4;
        // предварительный подсчет
        public const int timeend_pos_x = border_size + indicator_size + l2_size + time_width;
        public const int cell_height = string_height * 2 + l2_size;
        public const int lesson_width = image_width - timeend_pos_x - l1_size - border_size;
        // размер изображения tomorrow
        public const int tomorrow_image_width = 500; // ширина
        public const int tomorrow_image_height = 816; // высота
        // отступы tomorrow
        public const int tomorrow_same_lessons = 6; // сдвиг вверх и вниз при одинаковых парах по верхним и нижним неделям
        public const int tomorrow_string_height = 40; // высота строки
        public const int tomorrow_time_width = 96; // ширина времени пар
        public const int tomorrow_lesson_fix = 4;
        // предварительный подсчет tomorrow
        public const int tomorrow_timeend_pos_x = border_size + indicator_size + l2_size + tomorrow_time_width;
        public const int tomorrow_cell_height = tomorrow_string_height * 2;
        public const int tomorrow_lesson_width = tomorrow_image_width - tomorrow_timeend_pos_x - l1_size - border_size;
        // текст
        public static string footer_text = "vk.com/itmmschedulebot · v2.2";
    }
    public static class Glob
    {
        public static string[] schedule_url = new string[4];
        public static string[] data = new string[4];
        public static string[,,] schedule = new string[4, 40, 98];
        public static ulong[,] schedule_uploaded = new ulong[4, 40];
        public static bool[,,,] tomorrow_studing = new bool[4, 40, 6, 2];
        public static ulong[,,,] tomorrow_uploaded = new ulong[4, 40, 6, 2];
        public static Dictionary<User, Mapping> schedule_mapping = new Dictionary<User, Mapping>(1000);
        public static string[] full_name = new string[100];
        public static int full_name_count;
        public static string[,] acronym_to_phrase = new string[2, 50];
        public static int acronym_to_phrase_count;
        public static string[,] double_optionally_subject = new string[2, 8];
        public static int double_optionally_subject_count = 8;

        public static int startDay = Const.start_day;
        public static int startWeek = Const.start_week;
        public static Dictionary<long?, User> users = new Dictionary<long?, User>(1000);
        public static VkApi api = new VkApi();
        public static VkApi apiPhotos = new VkApi();
        public static int[] keyboardsNewSubCount = new int[4] { 0, 0, 0, 0 };
        public static MessageKeyboard[,] keyboardsNewSub = new MessageKeyboard[4, 10];
        public static ConcurrentQueue<string> queueCommands = new ConcurrentQueue<string>();
        public static bool subsChanges = false;
        public static bool isUpdating = false;
        public static bool[] isBroken = { false, false, false, false };
        // lockers
        public static object locker = new object(); // за доступ к глобальным
        public static object lockerKeyboards = new object(); // за доступ к клавиатурам
        public static object lockerIsUpdating = new object(); // за доступ во время обновления
        public static object lockerIsBroken = new object(); // за доступ, если расписание сломано
    }
    class Program
    {
        static void Main(string[] args)
        {
            // ЗАПУСК
            Utils.StartUp();
            Thread relevanceCheck = new Thread(Schedule.СheckRelevance); // расписание
            Thread executeMethods = new Thread(Vk.ExecuteMethods); // Собирает команды и отправляет
            Thread vkGetMessages = new Thread(Vk.GetMessages)
            {
                Priority = ThreadPriority.AboveNormal
            }; // вк
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " -THREAD- Запуск \"relevanceCheck\""); // threadlog
            relevanceCheck.Start();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " -THREAD- Запущен \"relevanceCheck\""); // threadlog
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " -THREAD- Запуск \"executeMethods\""); // threadlog
            executeMethods.Start();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " -THREAD- Запущен \"executeMethods\""); // threadlog
            Thread.Sleep(500); // ставить побольше
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " -THREAD- Запуск \"vkGetMessages\""); // threadlog
            vkGetMessages.Start();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " -THREAD- Запущен \"vkGetMessages\""); // threadlog
            // отправляем сообщение, что запустились
            var messagesSendParams = new MessagesSendParams()
            {
                UserId = 135696841,
                Message = "Запустился",
                RandomId = new Random().Next()
            };
            Glob.queueCommands.Enqueue("API.messages.send(" + JsonConvert.SerializeObject(MessagesSendParams.ToVkParameters(messagesSendParams), Newtonsoft.Json.Formatting.Indented) + ");");
        }
    }
}
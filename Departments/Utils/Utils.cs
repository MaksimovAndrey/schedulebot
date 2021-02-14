using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;

namespace Schedulebot.Departments.Utils
{
    public static class Utils
    {
        public static List<MessageKeyboard>[] ConstructKeyboards(in Mapping.Mapper mapper, int coursesCount)
        {
            const int buttonsInLine = 2; // 1..4 ограничения vk
            const int linesInKeyboard = 4; // 1..9 ограничения vk

            List<MessageKeyboard>[] result = new List<MessageKeyboard>[coursesCount];
            for (int currentCourse = 0; currentCourse < coursesCount; ++currentCourse)
            {
                result[currentCourse] = new List<MessageKeyboard>();
                List<string> groupNames = mapper.GetGroupNames(currentCourse);
                int pagesAmount = (int)Math.Ceiling((double)groupNames.Count
                    / (double)(linesInKeyboard * buttonsInLine));
                int currentPage = 0;
                List<MessageKeyboardButton> line = new List<MessageKeyboardButton>();
                List<List<MessageKeyboardButton>> buttons = new List<List<MessageKeyboardButton>>();
                List<MessageKeyboardButton> serviceLine = new List<MessageKeyboardButton>();
                for (int currentName = 0; currentName < groupNames.Count; currentName++)
                {
                    line.Add(new MessageKeyboardButton()
                    {
                        Color = KeyboardButtonColor.Primary,
                        Action = new MessageKeyboardButtonAction
                        {
                            Type = KeyboardButtonActionType.Text,
                            Label = groupNames[currentName],
                            Payload = "{\"menu\": \"40\", \"course\": \"" + currentCourse + "\"}"
                        }
                    });
                    if (line.Count == buttonsInLine
                        || (currentName + 1 == groupNames.Count && line.Count != 0))
                    {
                        buttons.Add(new List<MessageKeyboardButton>(line));
                        line.Clear();
                    }
                    if (buttons.Count == linesInKeyboard
                        || (currentName + 1 == groupNames.Count && buttons.Count != 0))
                    {
                        string payloadService = "{\"menu\": \"40\", \"page\": \"" + currentPage + "\", \"course\": \"" + currentCourse + "\"}";
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = Constants.back,
                                Payload = payloadService
                            }
                        });
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = (currentPage + 1) + " из " + pagesAmount,
                                Payload = payloadService
                            }
                        });
                        serviceLine.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Default,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = KeyboardButtonActionType.Text,
                                Label = Constants.forward,
                                Payload = payloadService
                            }
                        });
                        buttons.Add(new List<MessageKeyboardButton>(serviceLine));
                        serviceLine.Clear();
                        result[currentCourse].Add(new MessageKeyboard
                        {
                            Buttons = new List<List<MessageKeyboardButton>>(buttons),
                            OneTime = false
                        });
                        buttons.Clear();
                        ++currentPage;
                    }
                }
            }
            return result;
        }

        //public static void SaveCoursesFilePaths(in Course[] courses, int coursesCount, string path)
        //{
        //    List<List<string>> coursesFilePaths = new List<List<string>>();
        //    for (int currentCourse = 0; currentCourse < coursesCount; currentCourse++)
        //        coursesFilePaths.Add(courses[currentCourse].PathsToFile);

        //    using StreamWriter file = new StreamWriter(path);
        //    file.WriteLine(JsonConvert.SerializeObject(coursesFilePaths));
        //}

        public static List<List<string>> GetCoursesFilePaths(string path)
        {
            if (File.Exists(path))
            {
                using StreamReader file = new StreamReader(path, Encoding.Default);
                return JsonConvert.DeserializeObject<List<List<string>>>(file.ReadToEnd());
            }
            else
                return new List<List<string>>();
        }

        public static string ConstructGroupSubgroup(string group, int subgroup)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(group);
            stringBuilder.Append(" (");
            stringBuilder.Append(subgroup);
            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }
    }
}

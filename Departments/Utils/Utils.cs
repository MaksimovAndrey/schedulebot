using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace Schedulebot.Departments.Utils
{
    public static class Utils
    {
        public static List<MessageKeyboard>[,] ConstructKeyboards(in Mapping.Mapper mapper, int coursesCount)
        {
            const int buttonsInLine = 2; // 1..4 ограничения vk
            const int linesInKeyboard = 4; // 1..9 ограничения vk

            List<MessageKeyboard>[,] result = new List<MessageKeyboard>[coursesCount, 2];
            for (int curCourse = 0; curCourse < coursesCount; ++curCourse)
            {
                for (int type = 0; type < 2; type++)
                {
                    result[curCourse, type] = new List<MessageKeyboard>();
                    List<string> groupNames = mapper.GetGroupNames(curCourse);
                    int pagesAmount = (int)Math.Ceiling((double)groupNames.Count
                        / (double)(linesInKeyboard * buttonsInLine));
                    int curPage = 0;
                    List<MessageKeyboardButton> line = new List<MessageKeyboardButton>();
                    List<List<MessageKeyboardButton>> buttons = new List<List<MessageKeyboardButton>>();
                    List<MessageKeyboardButton> serviceLine = new List<MessageKeyboardButton>();
                    for (int curName = 0; curName < groupNames.Count; curName++)
                    {
                        line.Add(new MessageKeyboardButton()
                        {
                            Color = KeyboardButtonColor.Primary,
                            Action = new MessageKeyboardButtonAction
                            {
                                Type = type == 0 ? KeyboardButtonActionType.Text : KeyboardButtonActionType.Callback,
                                Label = groupNames[curName],
                                Payload = "{\"menu\":\"40\",\"act\":\"1\",\"course\":\"" + curCourse + "\",\"group\":\"" + groupNames[curName] + "\"}"
                            }
                        });
                        if (line.Count == buttonsInLine
                            || (curName + 1 == groupNames.Count && line.Count != 0))
                        {
                            buttons.Add(new List<MessageKeyboardButton>(line));
                            line.Clear();
                        }
                        if (buttons.Count == linesInKeyboard
                            || (curName + 1 == groupNames.Count && buttons.Count != 0))
                        {
                            serviceLine.Add(new MessageKeyboardButton()
                            {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction
                                {
                                    Type = type == 0 ? KeyboardButtonActionType.Text : KeyboardButtonActionType.Callback,
                                    Label = Constants.Labels.previousPage,
                                    Payload = "{\"menu\":\"40\",\"act\":\"2\",\"course\":\"" + curCourse + "\",\"page\":\"" + curPage + "\"}"
                                }
                            });
                            serviceLine.Add(new MessageKeyboardButton()
                            {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction
                                {
                                    Type = type == 0 ? KeyboardButtonActionType.Text : KeyboardButtonActionType.Callback,
                                    Label = (curPage + 1) + Constants.Labels.currentPageOfMaxDelimeter + pagesAmount,
                                    Payload = "{\"menu\":\"40\",\"act\":\"3\",\"course\":\"" + curCourse + "\"}"
                                }
                            });
                            serviceLine.Add(new MessageKeyboardButton()
                            {
                                Color = KeyboardButtonColor.Default,
                                Action = new MessageKeyboardButtonAction
                                {
                                    Type = type == 0 ? KeyboardButtonActionType.Text : KeyboardButtonActionType.Callback,
                                    Label = Constants.Labels.nextPage,
                                    Payload = "{\"menu\":\"40\",\"act\":\"4\",\"course\":\"" + curCourse + "\",\"page\":\"" + curPage + "\"}"
                                }
                            });
                            buttons.Add(new List<MessageKeyboardButton>(serviceLine));
                            serviceLine.Clear();
                            result[curCourse, type].Add(new MessageKeyboard
                            {
                                Buttons = new List<List<MessageKeyboardButton>>(buttons),
                                OneTime = false
                            });
                            buttons.Clear();
                            ++curPage;
                        }
                    }
                }
            }
            return result;
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

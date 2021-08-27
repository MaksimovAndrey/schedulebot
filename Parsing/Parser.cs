using Newtonsoft.Json;
using Schedulebot.Parsing.Utils;
using Schedulebot.Schedule;
using System;
using System.Collections.Generic;

namespace Schedulebot.Parsing
{
    public static class Parser
    {
        public static List<ScheduleDay> ParseScheduleFromJson(string jsonStr)
        {
            List<ParsedLecture> parsedLectures = JsonConvert.DeserializeObject<List<ParsedLecture>>(jsonStr);
            List<ScheduleDay> days = new List<ScheduleDay>();
            List<string> dates = new List<string>();
            for (int curParsedLecture = 0; curParsedLecture < parsedLectures.Count; curParsedLecture++)
            {
                if (dates.Contains(parsedLectures[curParsedLecture].Date))
                {
                    days[dates.IndexOf(parsedLectures[curParsedLecture].Date)].lectures.Add(
                        new ScheduleLecture(
                            parsedLectures[curParsedLecture]));
                }
                else
                {
                    dates.Add(parsedLectures[curParsedLecture].Date);
                    days.Add(new ScheduleDay(DateTime.Parse(parsedLectures[curParsedLecture].Date)));
                    days[dates.Count - 1].lectures.Add(
                        new ScheduleLecture(
                            parsedLectures[curParsedLecture]));
                }
            }
            return days;
        }
    }
}

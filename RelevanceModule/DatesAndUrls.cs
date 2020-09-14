using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Schedulebot.Schedule.Relevance
{
    // SAVED DATESANDURLS.TXT
    // {"urls":[[],[],[],[],[]],"dates":["","","","",""],"Count":4}
    public class DatesAndUrls
    {
        public int Count { get; set; } = 4; // TODO: >5 => error
        public List<List<string>> urls;
        public string[] dates;

        private string Path { get; }

        private const string c_defaultFilename = "datesAndUrls.txt";
        private const int c_maxCourses = 5;

        public DatesAndUrls() { }

        public DatesAndUrls(string path)
        {
            Path = path + c_defaultFilename;
            Load();
        }

        public void Save()
        {
            using (StreamWriter file = new StreamWriter(Path))
            {
                file.WriteLine(JsonConvert.SerializeObject(this));
            }
        }

        private void Load()
        {
            urls = new List<List<string>>();
            dates = new string[c_maxCourses];
            for (int currenCourse = 0; currenCourse < c_maxCourses; currenCourse++)
            {
                urls.Add(new List<string>());
                dates[currenCourse] = "";
            }

            if (File.Exists(Path))
            {
                using (StreamReader file = new StreamReader(Path, System.Text.Encoding.Default))
                {
                    string json = file.ReadToEnd();
                    DatesAndUrls readed = JsonConvert.DeserializeObject<DatesAndUrls>(json);
                    for (int currentCourse = 0; currentCourse < Math.Min(readed.urls.Count, c_maxCourses); currentCourse++)
                    {
                        urls[currentCourse] = readed.urls[currentCourse];
                        dates[currentCourse] = readed.dates[currentCourse];
                    }
                }
            }
        }
    }
}
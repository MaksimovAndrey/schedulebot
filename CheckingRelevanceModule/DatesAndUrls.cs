using System.IO;

namespace Schedulebot.Schedule.Relevance
{
    public class DatesAndUrls
    {
        public int Count { get; set; } = 4; // todo: >5 => error
        public string[] urls = new string[5];
        public string[] dates = new string[5];
        
        private string Path { get; }

        public DatesAndUrls() { }

        public DatesAndUrls(string path)
        {
            Path = path + "datesAndUrls.txt";
            Load();
        }

        public void Save()
        {
            using (StreamWriter file = new StreamWriter(Path))
            {
                for (int currentCourse = 0; currentCourse < Count; currentCourse++)
                    file.WriteLine(urls[currentCourse] + " " + dates[currentCourse]);
            }
        }

        private void Load()
        {
            using (StreamReader file = new StreamReader(Path, System.Text.Encoding.Default))
            {
                for (int currentCourse = 0; currentCourse < Count; currentCourse++)
                {
                    string str = file.ReadLine();
                    urls[currentCourse] = str.Substring(0, str.IndexOf(' '));
                    dates[currentCourse] = str.Substring(str.IndexOf(' ') + 1);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace schedulebot
{
    public static class IO
    {   
        public static void LoadSettings()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка настроек"); // log
            StreamReader file = new StreamReader(
                Const.path_config + "settings.txt",
                System.Text.Encoding.Default);
            string str, value;
            while ((str = file.ReadLine()) != null)
            {
                if (str.Contains(':'))
                {
                    value = str.Substring(str.IndexOf(':') + 1);
                    str = str.Substring(0, str.IndexOf(':'));
                    switch (str)
                    {
                        case "key":
                        {
                            Const.key = value;
                            break;
                        }
                        case "keyPhotos":
                        {
                            Const.keyPhotos = value;
                            break;
                        }
                        case "groupId":
                        {
                            Const.groupId = ulong.Parse(value);
                            break;
                        }
                        case "mainAlbumId":
                        {
                            Const.mainAlbumId = Int64.Parse(value);
                            break;
                        }
                        case "tomorrowAlbumId":
                        {
                            Const.tomorrowAlbumId = Int64.Parse(value);
                            break;
                        }
                        case "start_day":
                        {
                            Const.start_day = Int32.Parse(value);
                            break;
                        }
                        case "start_week":
                        {
                            Const.start_week = Int32.Parse(value);
                            break;
                        }
                        case "footer_text":
                        {
                            Const.footer_text = value;
                            break;
                        }
                    }
                }
            }
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка настроек"); // log
        }
        public static void LoadDataUrls()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка сохранённых url на .xls расписание"); // log
            StreamReader fileRead = new StreamReader(
                Const.path_config + "parse.txt",
                System.Text.Encoding.Default);
            lock (Glob.locker)
            {
                for (int i = 0; i < 4; ++i)
                {
                    Glob.data[i] = fileRead.ReadLine();
                    Glob.schedule_url[i] = fileRead.ReadLine();
                }
            }
            fileRead.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка сохранённых url на .xls расписание"); // log
        }
        public static void LoadSavedSchedule()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка сохранённого расписания"); // log
            string[] files = Directory.GetFiles(Const.path_schedule, "*.txt").Select(Path.GetFileName).ToArray();
            foreach (string name in files)
            {
                int.TryParse(name.Substring(0, 1), out int course);
                int.TryParse(name.Substring(2, name.IndexOf('.') - 2), out int index);
                StreamReader file;
                lock (Glob.locker)
                {
                    file = new StreamReader(Const.path_schedule + name, System.Text.Encoding.Default);
                    for (int j = 0; j < 98; ++j)
                    {
                        Glob.schedule[course - 1, index, j] = file.ReadLine();
                    }
                }
                file.Close();
            }
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка сохранённого расписания"); // log
        }
        public static void LoadManualFullName()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка ManualFullName"); // log
            string temp;
            int count = 0;
            StreamReader file = new StreamReader(
                Const.path_manual + "fullName.txt",
                System.Text.Encoding.Default);
            while ((temp = file.ReadLine()) != null)
            {
                Glob.full_name[count] = temp;
                ++count;
            }
            Glob.full_name_count = count;
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка ManualFullName"); // log
        }
        public static void LoadManualAcronymToPhrase()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка ManualAcronymToPhrase"); // log
            string[] temp = new string[2];
            int count = 0;
            StreamReader file = new StreamReader(
                Const.path_manual + "acronymToPhrase.txt",
                System.Text.Encoding.Default);
            while ((temp[0] = file.ReadLine()) != null && (temp[1] = file.ReadLine()) != null)
            {
                Glob.acronym_to_phrase[0, count] = temp[0];
                Glob.acronym_to_phrase[1, count] = temp[1];
                ++count;
            }
            Glob.acronym_to_phrase_count = count;
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка ManualAcronymToPhrase"); // log
        }
        public static void LoadManualDoubleOptionalSubject()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка ManualDoubleOptionalSubject"); // log
            string[] temp = new string[2];
            int count = 0;
            StreamReader file = new StreamReader(
                Const.path_manual + "doubleOptionallySubject.txt",
                System.Text.Encoding.Default);
            while ((temp[0] = file.ReadLine()) != null && (temp[1] = file.ReadLine()) != null)
            {
                Glob.double_optionally_subject[0, count] = temp[0];
                Glob.double_optionally_subject[1, count] = temp[1];
                ++count;
            }
            Glob.double_optionally_subject_count = count;
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка ManualDoubleOptionalSubject"); // log
        }
        public static void LoadUploadedSchedule()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка UploadedSchedule"); // log
            StreamReader file;
            string line;
            lock (Glob.locker)
            {
                file = new StreamReader(Const.path_config + "uploadedSchedule.txt", System.Text.Encoding.Default);
                while ((line = file.ReadLine()) != null)
                {
                    Glob.schedule_uploaded[int.Parse(line.Substring(0, line.IndexOf(' '))), int.Parse(line.Substring(line.IndexOf(' ') + 1, line.LastIndexOf(' ') - line.IndexOf(' ') - 1))]
                        = ulong.Parse(line.Substring(line.LastIndexOf(' ') + 1));
                }
            }
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка UploadedSchedule"); // log
        }
        public static void SaveUploadedSchedule()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Сохранение UploadedSchedule"); // log
            StreamWriter file = new StreamWriter(Const.path_config + "uploadedSchedule.txt", false, System.Text.Encoding.Default);
            lock (Glob.locker)
            {
                for (int i = 0; i < Glob.schedule_uploaded.GetLength(0); ++i)
                {
                    for (int j = 0; j < Glob.schedule_uploaded.GetLength(1); ++j)
                    {
                        if (Glob.schedule_uploaded[i, j] != 0)
                            file.WriteLine(i + " " + j + " " + Glob.schedule_uploaded[i, j]);
                    }
                }
            }
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Сохранение UploadedSchedule"); // log
        }
        public static void LoadSubscribers()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Загрузка подписанных"); // log
            StreamReader file = new StreamReader(
                Const.path_config + "subscribers.txt",
                System.Text.Encoding.Default);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                User tempUser = new User
                {
                    Group = line.Substring(
                        line.IndexOf(' ') + 1,
                        line.LastIndexOf(' ') - line.IndexOf(' ') - 1),
                    Subgroup = line.Substring(
                        line.LastIndexOf(' ') + 1,
                        1)
                };
                long temp = Convert.ToInt64(line.Substring(0, line.IndexOf(' ')));
                lock (Glob.locker)
                {
                    Glob.users.Add(temp, tempUser);
                }
            }
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Загрузка подписанных"); // log
        }
        public static void SaveSubscribers()
        {
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [S] Сохранение подписанных"); // log
            StreamWriter file;
            lock (Glob.locker)
            {
                file = new StreamWriter(
                    Const.path_config + "subscribers.txt",
                    false,
                    System.Text.Encoding.Default);
                foreach (KeyValuePair<long?, User> pair in Glob.users)
                {
                    file.WriteLine(pair.Key + " " + pair.Value.Group + " " + pair.Value.Subgroup);
                }
            }
            file.Close();
            Console.WriteLine(DateTime.Now.TimeOfDay.ToString() + " [E] Сохранение подписанных"); // log
        }
    }
}
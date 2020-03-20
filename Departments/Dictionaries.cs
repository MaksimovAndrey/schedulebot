using System.IO;
using System.Collections.Generic;

namespace Schedulebot
{
    public class Dictionaries
    {
        public readonly Dictionary<string, string> acronymToPhrase = new Dictionary<string, string>();
        public readonly Dictionary<string, string> doubleOptionallySubject = new Dictionary<string, string>();
        public readonly List<string> fullName = new List<string>();

        public Dictionaries(string path)
        {
            LoadAcronymToPhrase(path);
            LoadDoubleOptionallySubject(path);
            LoadFullName(path);
        }

        private void LoadAcronymToPhrase(string path)
        {
            StreamReader file = null;
            try
            {
                file = new StreamReader(
                    path + "acronymToPhrase.txt",
                    System.Text.Encoding.Default);
                while (!file.EndOfStream)
                    acronymToPhrase.Add(file.ReadLine(), file.ReadLine());
            }
            catch
            {
                acronymToPhrase.Clear();
            }
            finally
            {
                if (file != null)
                    file.Dispose();
            }
        }

        private void LoadDoubleOptionallySubject(string path)
        {
            StreamReader file = null;
            try
            {
                file = new StreamReader(
                    path + "doubleOptionallySubject.txt",
                    System.Text.Encoding.Default);
                while (!file.EndOfStream)
                    doubleOptionallySubject.Add(file.ReadLine(), file.ReadLine());
            }
            catch
            {
                doubleOptionallySubject.Clear();
            }
            finally
            {
                if (file != null)
                    file.Dispose();
            }
        }

        private void LoadFullName(string path)
        {
            StreamReader file = null;
            try
            {
                file = new StreamReader(
                    path + "fullName.txt",
                    System.Text.Encoding.Default);
                while (!file.EndOfStream)
                    fullName.Add(file.ReadLine());
            }
            catch
            {
                fullName.Clear();
            }
            finally
            {
                if (file != null)
                    file.Dispose();
            }
        }
    }
}
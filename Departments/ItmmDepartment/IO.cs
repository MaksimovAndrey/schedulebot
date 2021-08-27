using Schedulebot.Users;
using System.IO;
using System.Text;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : Department
    {
        public override void SaveUsers()
        {
            IUserRepositorySaver.Save(userRepository, Path + Constants.userRepositoryFilename);
        }

        private void LoadSettings(string path)
        {
            using StreamReader file = new StreamReader(path, Encoding.Default);
            string str, value;
            while ((str = file.ReadLine()) != null)
            {
                if (str.Contains(':'))
                {
                    value = str.Substring(str.IndexOf(':') + 1);
                    str = str.Substring(0, str.IndexOf(':'));
                    switch (str)
                    {
                        case "startDay":
                        {
                            startDay = int.Parse(value);
                            break;
                        }
                        case "startWeek":
                        {
                            startWeek = int.Parse(value);
                            break;
                        }
                    }
                }
            }
        }
    }
}

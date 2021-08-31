using System.IO;
using System.Text;

namespace Schedulebot.Users
{
    public interface IUserRepositoryReader
    {
        /// <summary>
        /// Метод для чтения репозитория пользователей
        /// </summary>
        public static void Read(ref IUserRepository userRepository, string filename)
        {
            using StreamReader streamReader = new StreamReader(filename, Encoding.Default);
            while (!streamReader.EndOfStream)
            {
                if (User.TryParse(streamReader.ReadLine(), out var user))
                    userRepository.Add(user);
            }
        }
    }
}

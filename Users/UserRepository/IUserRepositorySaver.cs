using System.IO;

namespace Schedulebot.Users
{
    public interface IUserRepositorySaver
    {
        /// <summary>
        /// Метод для сохранения репозитория пользователей
        /// </summary>
        public static void Save(IUserRepository userRepository, string filename)
        {
            using StreamWriter streamWriter = new StreamWriter(filename);
            streamWriter.WriteLine(userRepository.ToString());
        }
    }
}

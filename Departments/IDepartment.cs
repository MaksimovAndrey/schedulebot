using System.Threading.Tasks;

namespace Schedulebot
{
    public interface IDepartment
    {
        void CheckRelevanceAsync();

        Task GetMessagesAsync();

        Task UploadPhotosAsync();

        Task ExecuteMethodsAsync();
    }
}
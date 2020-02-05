using System.Threading.Tasks;

namespace Schedulebot
{
    public interface IDepartment : IVkStuff
    {
        
    }

    public interface IVkStuff
    {
        void CheckRelevanceAsync();

        Task GetMessagesAsync();

        Task UploadPhotosAsync();

        Task ExecuteMethodsAsync();
    }
}
using VkNet;
using VkNet.Model.Keyboard;
using System.Collections.Concurrent;

namespace Schedulebot.Vk
{
    public class VkStuff
    {
        public readonly ConcurrentQueue<string> commandsQueue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<PhotoUploadProperties> photosQueue = new ConcurrentQueue<PhotoUploadProperties>();
        public readonly VkApi api = new VkApi();
        public readonly VkApi apiPhotos = new VkApi();
        public long GroupId { get; set; }
        public long MainAlbumId { get; set; }
        // public long TomorrowAlbumId { get; set; } нельзя юзать, потому что одновременная загрузка возможна только в 1 альбом, а делать 2 очереди и соответственно метода я пока не желаю
        public long AdminId { get; set; }
        public string GroupUrl { get; set; }
        public MessageKeyboard[] MenuKeyboards { get; set; }
    }

    public class PhotoUploadProperties
    {
        public byte[] Photo { get; set; }

        public string Group { get; set; } = null;

        public int Subgroup { get; set; } = 0;

        public long AlbumId { get; set; }

        public int Week { get; set; } = -1;

        public int Day { get; set; }

        public string Message { get; set; } = null;
        
        public long PeerId { get; set; } = 0; // когда на день, кому отправить

        public PhotoUploadProperties()
        {

        }
        
        public PhotoUploadProperties(PhotoUploadProperties photoUploadProperties)
        {
            Photo = photoUploadProperties.Photo;
            Group = photoUploadProperties.Group;
            Subgroup = photoUploadProperties.Subgroup;
            AlbumId = photoUploadProperties.AlbumId;
            Week = photoUploadProperties.Week;
            Day = photoUploadProperties.Day;
            Message = photoUploadProperties.Message;
            PeerId = photoUploadProperties.PeerId;
        }
    }
    
    public class UpdateProperties
    {
        public Drawing.DrawingStandartScheduleInfo drawingStandartScheduleInfo = new Drawing.DrawingStandartScheduleInfo();
        public Vk.PhotoUploadProperties photoUploadProperties = new PhotoUploadProperties();
    }

}
namespace Schedulebot.Vk
{
    public class PhotoUploadProperties
    {
        public byte[] Photo { get; set; }

        public long AlbumId { get; set; }

        public string GroupName { get; set; } = null;

        public int Subgroup { get; set; } = 0; // Index (0, 1)

        public int Course { get; set; }

        public int GroupIndex { get; set; }

        public int Week { get; set; } = -1;

        public int Day { get; set; }

        public string Message { get; set; } = null;

        public long PeerId { get; set; } = 0; // когда на день, кому отправить

        public bool ToSend { get; set; } = true;

        public long Id { get; set; }

        public UploadingSchedule UploadingSchedule { get; set; }

        public PhotoUploadProperties() { }

        public PhotoUploadProperties(PhotoUploadProperties photoUploadProperties)
        {
            Photo = photoUploadProperties.Photo;
            AlbumId = photoUploadProperties.AlbumId;
            GroupName = photoUploadProperties.GroupName;
            Subgroup = photoUploadProperties.Subgroup;
            Course = photoUploadProperties.Course;
            GroupIndex = photoUploadProperties.GroupIndex;
            Week = photoUploadProperties.Week;
            Day = photoUploadProperties.Day;
            Message = photoUploadProperties.Message;
            PeerId = photoUploadProperties.PeerId;
            ToSend = photoUploadProperties.ToSend;
            Id = photoUploadProperties.Id;
            UploadingSchedule = photoUploadProperties.UploadingSchedule;
        }
    }
}
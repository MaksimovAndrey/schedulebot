using Schedulebot.Drawing;

namespace Schedulebot.Vk
{
    public class UpdateProperties
    {
        public Drawing.DrawingStandartScheduleInfo drawingStandartScheduleInfo;
        public Vk.PhotoUploadProperties photoUploadProperties;


        public UpdateProperties()
        {
            drawingStandartScheduleInfo = new DrawingStandartScheduleInfo();
            photoUploadProperties = new PhotoUploadProperties();
        }

        public UpdateProperties(UpdateProperties up)
        {
            drawingStandartScheduleInfo = new DrawingStandartScheduleInfo(up.drawingStandartScheduleInfo);
            photoUploadProperties = new PhotoUploadProperties(up.photoUploadProperties);
        }
    }
}
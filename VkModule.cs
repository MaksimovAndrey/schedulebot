using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;
using System.Net;
using System.Xml;
using GemBox.Spreadsheet;
using System.IO;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using VkNet.Categories;
using VkNet.Enums.SafetyEnums;
using System.Drawing;
using System.Text.RegularExpressions;
using VkNet.Model.Keyboard;
using VkNet.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using VkNet.Exception;

namespace Schedulebot.Vk
{
    public class VkStuff
    {
        public readonly ConcurrentQueue<string> commandsQueue = new ConcurrentQueue<string>();
        public readonly ConcurrentQueue<PhotoUploadProperties> uploadPhotosQueue = new ConcurrentQueue<PhotoUploadProperties>();
        public readonly VkApi api = new VkApi();
        public readonly VkApi apiPhotos = new VkApi();
        public long GroupId { get; set; }
        public long MainAlbumId { get; set; }
        // public long TomorrowAlbumId { get; set; } нельзя юзать, потому что одновременная загрузка возможна только в 1 альбом, а делать 2 очереди и соответственно метода я пока не желаю
        public long AdminId { get; set; }
        public string GroupUrl { get; set; }
        public MessageKeyboard[] MainMenuKeyboards { get; set; }
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
    }
    
    public class UpdateProperties
    {
        public Drawing.DrawingStandartScheduleInfo drawingStandartScheduleInfo;
        public Vk.PhotoUploadProperties photoUploadProperties;
    }

}
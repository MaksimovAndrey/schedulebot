using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Exception;
using VkNet.Enums.SafetyEnums;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;


using Schedulebot.Vk;

namespace Schedulebot
{
    public interface IDepartment
    {
        int CoursesAmount { get; set; }

        void CheckRelevanceAsync();

        Task GetMessagesAsync();

        Task UploadPhotosAsync();

        Task ExecuteMethodsAsync();
    }
}
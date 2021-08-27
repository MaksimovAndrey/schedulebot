#define DONT_CHECK_CHANGES

using Newtonsoft.Json;
using Schedulebot.Schedule.Relevance;
using Schedulebot.Mapping;
using Schedulebot.Mapping.Utils;
using Schedulebot.Users;
using Schedulebot.Vk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.Keyboard;
using System.Linq;
using VkNet.Model.GroupUpdate;

namespace Schedulebot.Departments
{
    public partial class DepartmentItmm : Department
    {
        private string Path { get; }

        private readonly ConcurrentQueue<string> commandsQueue
            = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<PhotoUploadProperties> photosQueue
            = new ConcurrentQueue<PhotoUploadProperties>();
        private readonly ConcurrentQueue<GroupUpdate> updatesQueue
            = new ConcurrentQueue<GroupUpdate>();

        private int CoursesCount { get; } = 4;
        private readonly Course[] courses = new Course[4];
        private List<Dictionary<string, long>> dictionaries;
        
        private List<MessageKeyboard>[,] CoursesKeyboards { get; set; }

        private readonly VkStuff vkStuff;
        private readonly Mapper mapper;
        private readonly IRelevance relevance;
        private readonly IUserRepository userRepository;

        private int startDay;
        private int startWeek;

        private DateTime StartTime { get; }

        private string importantInfo = "Здесь ничего нет.";

#if DEBUG
        private static readonly bool[] loadModule = {
            true, // 0 ExecuteMethodsAsync()
            true, // 1 GetMessagesAsync()
            true, // 2 ResponseMessagesAsync()
            true, // 3 UploadPhotosAsync()
            true, // 4 SaveUsersAsync()
            true  // 5 CheckRelevanceAsync()
        };
#else
        private static readonly bool[] loadModule = { 
            true, // 0 ExecuteMethodsAsync()
            true, // 1 GetMessagesAsync()
            true, // 2 ResponseMessagesAsync()
            true, // 3 UploadPhotosAsync()
            true, // 4 SaveUsersAsync()
            true  // 5 CheckRelevanceAsync()
        };
#endif

        public DepartmentItmm(string path, ref List<Task> tasks)
        {
            Path = path + Constants.defaultFolder;

            dictionaries = LoadGroupsDictionaries(Path + Constants.dictionariesFilename, CoursesCount);

            vkStuff = IVkStuffConstructor.Construct(Path + Constants.settingsFilename);

            userRepository = new UserRepository();
            IUserRepositoryReader.Read(ref userRepository, Path + Constants.userRepositoryFilename);

            for (int curCourse = 0; curCourse < CoursesCount; ++curCourse)
            {
                courses[curCourse] = new Course();
                for (int curGroup = 0; curGroup < dictionaries[curCourse].Count; curGroup++)
                {
                    courses[curCourse].groups.Add(new Group(dictionaries[curCourse].ElementAt(curGroup).Key));
                }
            }

            mapper = new Mapper(courses);

            relevance = new RelevanceItmm(Path, Path + Constants.defaultDownloadFolder);

            LoadSettings(Path + Constants.settingsFilename);

            CoursesKeyboards = Utils.Utils.ConstructKeyboards(in mapper, CoursesCount);

            if (loadModule[0])
                tasks.Add(Task.Run(() => ExecuteMethodsAsync()));
            if (loadModule[1])
                tasks.Add(Task.Run(() => GetMessages()));
            if (loadModule[2])
                tasks.Add(CreateResponseMessagesTasks(Constants.responseMessagesTaskCount));
            if (loadModule[3])
                tasks.Add(Task.Run(() => UploadPhotosAsync()));
            if (loadModule[4])
                tasks.Add(Task.Run(() => SaveUsersAsync()));

            StartTime = DateTime.Now;

            EnqueueMessage(
                userId: vkStuff.AdminId,
                message: StartTime.ToString() + " | Запустился"
            );

            if (loadModule[5])
            {
                tasks.Add(StartRelevanceModule());
                EnqueueMessage(
                    userId: vkStuff.AdminId,
                    message: DateTime.Now.ToString() + " | Запустил RelevanceModule"
                );
            }
        }
    }
}

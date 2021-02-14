using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot
{
    public static class Constants
    {
        #region delays
        public const int saveUsersDelay = 3600000;
        public const int loadWebsiteDelay = 600000;
        public const int waitPhotosUploadingDelay = 6000;
        public const int checkPhotosQueueDelay = 1000;

        #endregion

        #region textCommands
        public const string subscribeSign = "ПОДПИСАТЬСЯ ";
        public static readonly string[] textInfoCommand = { "ИНФОРМАЦИЯ", "ВАЖНАЯ ИНФОРМАЦИЯ", "INFO", "INFORMATION" };
        public static readonly string[] textUnsubscribeCommand = { "ОТПИСАТЬСЯ", "UNSUBSCRIBE" };
        public static readonly string[] textSubscribeCommand = { "ПОДПИСАТЬСЯ", "SUBSCRIBE" };
        public static readonly string[] textCurrentWeekCommand = { "НЕДЕЛЯ", "ТЕКУЩАЯ НЕДЕЛЯ", "КАКАЯ НЕДЕЛЯ", "WEEK", "CURRENT WEEK" };
        public static readonly string[] textTodayCommand = { "НА СЕГОДНЯ", "СЕГОДНЯ", "TODAY", "FOR TODAY" };
        public static readonly string[] textTomorrowCommand = { "НА ЗАВТРА", "ЗАВТРА", "TOMORROW", "FOR TOMORROW", "NEXT DAY" };
        public static readonly string[] textWeekCommand = { "НА НЕДЕЛЮ", "РАСПИСАНИЕ", "WEEK", "FOR WEEK", "FOR A WEEK", "SCHEDULE", "GET SCHEDULE" };
        public static readonly string[] textLinkCommand = { "ССЫЛКА", "LINK" };
        public static readonly string[] textHelpCommand = { "ПОМОЩЬ", "HELP", "!HELP" };
        #endregion

        #region scheduleUpdateMessages
        public const string newSchedule = "Вышло новое расписание от ";
        public const string waitScheduleUpdatingResult = ". Ожидайте результата обработки. Возможно дата совпадает с прошлой, но ссылки на расписание на сайте новые.";
        public const string loadScheduleError = "Не удалось загрузить расписание от ";
        public const string newScheduleHere = ". Новое расписание здесь: ";
        public const string updateScheduleError = "Не удалось обработать расписание от ";
        public const string noChanges = "Для Вас изменений нет";
        #endregion

        public const string linkCommand = "ССЫЛКА";
        public const string forWeekCommand = "НА НЕДЕЛЮ";
        public const string forTodayCommand = "НА СЕГОДНЯ";
        public const string forTomorrowCommand = "НА ЗАВТРА";
        public const string importantInfoCommand = "ВАЖНАЯ ИНФОРМАЦИЯ";
        public const string unsubscribeCommand = "ОТПИСАТЬСЯ";
        public const string subscribeCommand = "ПОДПИСАТЬСЯ";
        public const string resubscribeCommand = "ПЕРЕПОДПИСАТЬСЯ";
        public const string currentWeekCommand = "НЕДЕЛЯ";

        public const string back = "Назад";
        public const string backMenuItem = "НАЗАД";
        public const string forward = "Вперед";
        public const string forwardMenuItem = "ВПЕРЕД";
        public const string chooseCourse = "Выберите курс";
        public const string chooseCourseMenuItem = "ВЫБЕРИТЕ КУРС";
        public const string scheduleMenuItem = "РАСПИСАНИЕ";
        public const string settingsMenuItem = "НАСТРОЙКИ";
        public const string informationMenuItem = "ИНФОРМАЦИЯ";

        #region ScheduleBot
        public const string name = "schedulebot";
#if DEBUG
        public const string version = "v2.4 DEV";
#else
        public const string version = "v2.4";
#endif
        public const string delimiter = " · ";
        public const string lectureSign = "Л";
        public const string labSign = "Лаб";
        public const string seminarSign = "П";
        public const string remotelySign = "Д";
        #endregion

        #region Drawing
        public const string noOneLecture = "____";
        #endregion

        #region Portal API
        public const string portalAPI = "https://portal.unn.ru/ruzapi/schedule/group/";
        public const string portalAPILangArg = "&lng=1";
        #endregion

        public const string todayIsSunday = "Сегодня воскресенье";
        public const string scheduleForToday = "Расписание на сегодня";
        public const string scheduleForTomorrow = "Расписание на завтра";
        public const string todayYouAreNotStudying = "Сегодня Вы не учитесь";
        public const string tomorrowIsSundayMessage
            = "Завтра воскресенье, вот расписание на ближайший учебный день";
        public const string tomorrowIsNotStudyingDay
            = "Завтра Вы не учитесь, вот расписание на ближайший учебный день";
        public const string yourScheduleIsEmpty = "В Вашем расписании нет учебных дней";

        public const string unknownUserMessage = "Вы не настроили свою группу";
        public const string userGroupUnknownMessage = "Ваша группа не существует, настройте заново";
        public const string yourCourseScheduleBroken = "Расписание Вашего курса не обработано";

        public const string youAreSubscribed = "Вы подписаны: ";
        public const string yourSubgroup = "Ваша подгруппа: ";
        public const string oldKeyboardMessage = "Устаревшая клавиатура, оправляю актуальную";
        public const string scheduleFor = "Расписание для ";
        public const string drawPhotoError = "Не удалось нарисовать картинку, попробуйте позже";
        public const string pressAnotherButton = "Попробуйте нажать на другую кнопку";
        public const string unknownError = "Что-то пошло не так";

        public const string scheduleUpdatingMessage = 
            "Происходит обновление расписания, попробуйте через несколько минут";
        public const string unknownUserWithPayloadMessage =
            "Вы не настроили свою группу, тут можете настроить, нажмите на кнопку подписаться";


        public const string websiteUrl = @"http://www.itmm.unn.ru/studentam/raspisanie/raspisanie-bakalavriata-i-spetsialiteta-ochnoj-formy-obucheniya/";
        public const string about = "Текущая версия - v2.4";
        public const string startMessage = "Здравствуйтe, я буду присылать актуальное расписание, если Вы подпишитесь в настройках.\nКнопка \"Информация\" для получения подробностей";


        public const string defaultFolder = "itmm/";
        public const string defaultDownloadFolder = "downloads/";
        public const string coursesPathsFilename = "coursesPathsToFile.txt";
        public const string userRepositoryFilename = "users.txt";
        public const string dictionariesManualProcessingFolder = "manualProcessing/";
        public const string uploadedScheduleFilename = "uploadedSchedule.txt";

        public const string dictionariesFilename = "dictionaries.txt";
#if DEBUG
        public const string settingsFilename = "settings-.txt";
#else
        public const string settingsFilename = "settings.txt";
#endif
    }
}

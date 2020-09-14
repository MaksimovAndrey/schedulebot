using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot
{
    public static class Constants
    {
        public const int saveUsersDelay = 3600000;
        public const int loadWebsiteDelay = 600000;
        public const int waitPhotosUploadingDelay = 6000;
        public const int checkPhotosQueueDelay = 1000;

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
        public const string about = "Текущая версия - v2.3\n\nПри обновлении расписания на сайте Вам придёт сообщение. Далее Вы получите одно из трех сообщений:\n 1) Новое расписание *картинка*\n 2) Для Вас изменений нет\n 3) Не удалось скачать/обработать расписание *ссылка*\n Если не придёт никакого сообщения, Ваша группа скорее всего изменилась/не найдена. Настройте заново.\n\nВ расписании могут встретиться верхние индексы, предупреждающие о возможных ошибках. Советую ознакомиться со статьёй: vk.com/@itmmschedulebot-raspisanie";
        public const string startMessage = "Здравствуйтe, я буду присылать актуальное расписание, если Вы подпишитесь в настройках.\nКнопка \"Информация\" для получения подробностей";


        public const string defaultFolder = "itmm/";
        public const string defaultDownloadFolder = "downloads/";
        public const string coursesPathsFilename = "coursesPathsToFile.txt";
        public const string userRepositoryFilename = "users.txt";
        public const string dictionariesManualProcessingFolder = "manualProcessing/";
        public const string uploadedScheduleFilename = "uploadedSchedule.txt";

#if DEBUG
        public const string settingsFilename = "settings-.txt";
#else
        public const string settingsFilename = "settings.txt";
#endif
    }
}

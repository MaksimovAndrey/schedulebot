namespace Schedulebot.Utils
{
    public static class Converter
    {
        public static int DayToIndex(string day)
        {
            switch(day)
            {
                case "ПОНЕДЕЛЬНИК":
                    return 0;
                case "ВТОРНИК":
                    return 1;  
                case "СРЕДА":
                    return 2; 
                case "ЧЕТВЕРГ":
                    return 3; 
                case "ПЯТНИЦА":
                    return 4; 
                case "СУББОТА":
                    return 5; 
                default:
                    return -1;  
            }
        }

        public static string IndexToDay(int index)
        {
            switch (index)
            {
                case 0:
                    return "Понедельник";
                case 1:
                    return "Вторник";
                case 2:
                    return "Среда";
                case 3:
                    return "Четверг";
                case 4:
                    return "Пятница";
                case 5:
                    return "Суббота";
                default:
                    return "Неизвестно";
            }
        }


        public static int TimeToIndex(string time)
        {
            switch(time)
            {
                case "9:00-10:30":
                    return 0;
                case "10:40-12:10":
                    return 1;  
                case "12:20-13:50":
                    return 2; 
                case "14:30-16:00":
                    return 3; 
                case "16:10-17:40":
                    return 4; 
                case "17:50-19:20":
                    return 5;
                case "19:30-21:00":
                    return 6;
                default:
                    return -1;  
            }
        }
    }
}
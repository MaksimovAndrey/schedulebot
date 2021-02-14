namespace Schedulebot.Parsing.Enums
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    public enum ParseStatus
    {
        /// <summary>
        /// Статус не определён
        /// </summary>
        Unknown,
        /// <summary>
        /// Найдены все аргументы
        /// <br>Количество найденных аргументов: 1</br>
        /// </summary>
        F1,
        /// <summary>
        /// Найдены все аргументы
        /// <br>Количество найденных аргументов: 2</br>
        /// </summary>
        F2,
        /// <summary>
        /// Найдены все аргументы
        /// <br>Количество найденных аргументов: 3</br>
        /// </summary>
        F3,
        /// <summary>
        /// Аргументы не найдены
        /// </summary>
        N0,
        /// <summary>
        /// Не все аргументы найдены
        /// <br>Количество найденных аргументов: 1</br>
        /// </summary>
        N1
    }
}
namespace VKPlayer.Enums
{
    /// <summary>
    /// Типы кнопок в заголовке окна
    /// </summary>
    public enum HeaderMenuType
    {
        /// <summary>
        /// Все кнопки - "Свернуть, на полный экран, закрыть"
        /// </summary>
        AllButtons,

        /// <summary>
        /// Только кнопка "Закрыть"
        /// </summary>
        OnlyClose,

        /// <summary>
        /// Только "свернуть, закрыть
        /// </summary>
        MinimizeAndClose,

        /// <summary>
        /// Только "развернуть,закрыть"
        /// </summary>
        ExpandAndClose,

        /// <summary>
        /// Только "Свернуть"
        /// </summary>
        OnlyMinimize
    }
}

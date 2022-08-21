namespace DapperAssistant.SqlQueryStrings
{
    /// <summary>
    /// Данные по соединения таблиц
    /// </summary>
    internal class JoinData
    {
        /// <summary>
        /// Название таблицы связанной по внешнему ключу
        /// </summary>
        public string ForeignKeyTableName { get; set; }

        /// <summary>
        /// Тип соединения
        /// </summary>
        public string JoinType { get; set; }
    }
}
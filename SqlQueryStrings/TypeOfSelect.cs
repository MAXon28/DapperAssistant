namespace DapperAssistant.SqlQueryStrings
{
    /// <summary>
    /// Тип запроса выборки к таблице базы данных
    /// </summary>
    internal enum TypeOfSelect
    {
        /// <summary>
        /// Стандартная выборка строк без условия
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Выборка с условием
        /// </summary>
        WithCondition = 1
    }
}
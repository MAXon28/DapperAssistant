namespace DapperAssistant
{
    /// <summary>
    /// Настройки запроса к базе данных
    /// </summary>
    public class QuerySettings
    {
        /// <summary>
        /// Поле условия
        /// </summary>
        public string ConditionField { get; set; }

        /// <summary>
        /// Тип условия (больше, меньше, равно или не равно)
        /// </summary>
        public ConditionType ConditionType { get; set; }

        /// <summary>
        /// Значение условия
        /// </summary>
        public object ConditionFieldValue { get; set; }

        /// <summary>
        /// Указывает на максимальное количество значений, которое нужно получить из базы данных (запрос с предложение "TOP"). Иначе считываются все возможные значения
        /// </summary>
        public int CertainNumberOfRows { get; set; }

        /// <summary>
        /// Указывает на необходимость отсортировать значения по Id в порядке убывания 
        /// </summary>
        public bool NeedSortDescendingOrder { get; set; }
    }
}
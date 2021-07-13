namespace DapperAssistant.SqlQueryStrings
{
    /// <summary>
    /// Тип запроса выборки. Standard - встандартная выборка строк без условия базы данных, WithCondition - выборка с условием из базы данных
    /// </summary>
    internal enum TypeOfSelect
    {
        Standard = 0,
        WithCondition = 60
    }
}
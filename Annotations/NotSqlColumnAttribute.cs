using System;

namespace DapperAssistant.Annotations
{
    /// <summary>
    /// Атрибут, который указывает, что поле сущности не является столбцом в SQL-таблице
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotSqlColumnAttribute : Attribute { }
}
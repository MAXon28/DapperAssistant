using System;

namespace DapperAssistant.Annotations
{
    /// <summary>
    /// Атрибут, который позволяет указать, что поле не является столбцом в SQL-таблице
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotSqlColumnAttribute : Attribute { }
}
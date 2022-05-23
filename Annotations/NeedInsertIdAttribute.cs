using System;

namespace DapperAssistant.Annotations
{
    /// <summary>
    /// Атрибут который указывает на то, что идентификатор для SQL-таблицы будет создаваться на уровне приложения
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NeedInsertIdAttribute : Attribute { }
}
using System;

namespace DapperAssistant.Annotations
{
    /// <summary>
    /// Атрибут, который указывает связанную SQL сущность
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public  class RelatedSqlEntityAttribute : Attribute { }
}
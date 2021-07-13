using System;

namespace DapperAssistant.Annotations
{
    /// <summary>
    /// Атрибут, который позволяет указать, что поле в класса-модели .NET является внешним ключом в таблице базы данных. Нужно указать имя таблицы, которая связывается с внешним ключом.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlForeignKeyAttribute : Attribute
    {
        public SqlForeignKeyAttribute(string foreignKeyTableName)
        {
            ForeignKeyTableName = foreignKeyTableName;
        }

        /// <summary>
        /// Название таблицы связанной по внешнему ключу
        /// </summary>
        public string ForeignKeyTableName { get; }
    }
}
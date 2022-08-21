using System;

namespace DapperAssistant.Annotations
{
    /// <summary>
    /// Атрибут, который позволяет указать, что поле в класса-модели .NET является внешним ключом в таблице базы данных. Нужно указать имя таблицы, которая связывается с внешним ключом.
    /// Также можно указать тип соединения (INNER, RIGHT, LEFT или FULL)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlForeignKeyAttribute : Attribute
    {
        public SqlForeignKeyAttribute(string foreignKeyTableName, TypeOfJoin typeOfJoin = default)
        {
            ForeignKeyTableName = foreignKeyTableName;
            TypeOfJoin = typeOfJoin;
        }

        /// <summary>
        /// Название таблицы связанной по внешнему ключу
        /// </summary>
        public string ForeignKeyTableName { get; }

        /// <summary>
        /// Тип соединения
        /// </summary>
        public TypeOfJoin TypeOfJoin { get; set; }
    }
}
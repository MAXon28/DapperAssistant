using Microsoft.Data.SqlClient;
using System.Data;

namespace DapperAssistant
{
    /// <summary>
    /// Класс, который хранит в себе строку подключения к базе данных
    /// </summary>
    public class DbConnectionKeeper
    {
        /// <summary>
        /// Строка подключения
        /// </summary>
        private readonly string _connectionString;

        public DbConnectionKeeper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Получить подключение к базе данных
        /// </summary>
        /// <returns> Подключение к базе данных </returns>
        public IDbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
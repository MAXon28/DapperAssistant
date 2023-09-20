using Dapper;
using DapperAssistant.SqlQueryStrings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DapperAssistant
{
    /// <summary>
    /// Базовый класс-репозиторий, который содержит стандартную реализацию базовых запросов
    /// </summary>
    /// <typeparam name="TEntity"> Тип, который соответствует таблице в базе данных </typeparam>
    public abstract class StandardRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Запрос идентификатора добавленной записи (вызывается вместе с INSERT)
        /// </summary>
        private const string SelectLastInsertionId = "\nSELECT SCOPE_IDENTITY();";

        /// <summary>
        /// Хранитель строки подключения к базе данных
        /// </summary>
        protected readonly DbConnectionKeeper _dbConnectionKeeper;

        /// <summary>
        /// Имя таблицы в базе данных
        /// </summary>
        private readonly string _sqlTableName;

        /// <summary>
        /// Запрос вставки
        /// </summary>
        private readonly string _insertQuery;

        /// <summary>
        /// Запрос выборки
        /// </summary>
        private readonly string _selectQuery;

        /// <summary>
        /// Запрос выборки с условием
        /// </summary>
        private readonly string _selectQueryWithCondition;

        /// <summary>
        /// Запрос выборки без условия и без join 
        /// </summary>
        private readonly string _selectQueryWithoutConditionJoin;

        /// <summary>
        /// Запрос выборки без join 
        /// </summary>
        private readonly string _selectQueryWithoutJoin;

        /// <summary>
        /// Запрос обновления
        /// </summary>
        private readonly string _updateQuery;

        /// <summary>
        /// Запрос удаления
        /// </summary>
        private readonly string _deleteQuery;

        /// <summary>
        /// Словарь связанных сущностей (тип связанной сущности соответствует индексу свойства в текущей сущности)
        /// </summary>
        private readonly Dictionary<Type, int> _relatedEntitiesDictionary;

        protected StandardRepository(DbConnectionKeeper dbConnectionKeeper)
        {
            _dbConnectionKeeper = dbConnectionKeeper;

            var queriesDictionary = SqlQueryBuilder.GetQueries(typeof(TEntity), out _sqlTableName, out _relatedEntitiesDictionary);

            _insertQuery = queriesDictionary["INSERT"];
            _selectQuery = queriesDictionary["SELECT"];
            _selectQueryWithCondition = queriesDictionary["SELECT_WITH_CONDITION"];
            if (queriesDictionary.ContainsKey("SELECT_WITHOUT_CONDITION_JOIN"))
            {
                _selectQueryWithoutConditionJoin = queriesDictionary["SELECT_WITHOUT_CONDITION_JOIN"];
                _selectQueryWithoutJoin = queriesDictionary["SELECT_WITHOUT_JOIN"];
            }
            _updateQuery = queriesDictionary["UPDATE"];
            _deleteQuery = queriesDictionary["DELETE"];
        }

        /// <inheritdoc/>
        public async virtual Task<int> AddAsync(TEntity newData)
        {
            using var dbConnection = _dbConnectionKeeper.GetDbConnection();
            return await dbConnection.ExecuteAsync(_insertQuery, newData);
        }

        /// <inheritdoc/>
        public async virtual Task<TResult> AddAsync<TResult>(TEntity newData)
        {
            using var dbConnection = _dbConnectionKeeper.GetDbConnection();
            return await dbConnection.QuerySingleAsync<TResult>(_insertQuery.Insert(_insertQuery.Length, SelectLastInsertionId), newData);
        }

        /// <inheritdoc/>
        public async Task<int> AddAsync(TEntity newData, IDbConnection dbConnection, IDbTransaction transaction)
            => await dbConnection.ExecuteAsync(_insertQuery, newData, transaction: transaction);

        /// <inheritdoc/>
        public async Task<TResult> AddAsync<TResult>(TEntity newData, IDbConnection dbConnection, IDbTransaction transaction)
            => await dbConnection.QuerySingleAsync<TResult>(_insertQuery.Insert(_insertQuery.Length, SelectLastInsertionId), newData, transaction: transaction);

        /// <inheritdoc/>
        public async virtual Task<IEnumerable<TEntity>> GetAsync(int certainNumberOfRows = -1, bool needSortDescendingOrder = false)
        {
            var sqlQuery = new StringBuilder(_selectQuery);

            sqlQuery = GetFinalQuery(sqlQuery, certainNumberOfRows, needSortDescendingOrder);

            Console.WriteLine(sqlQuery);

            using var dbConnection = _dbConnectionKeeper.GetDbConnection();

            var neededMethod = GetMethodSelectQuery();

            var arguments = new object[3] { dbConnection, sqlQuery.ToString(), Type.Missing };

            var task = (Task<IEnumerable<TEntity>>)neededMethod.Invoke(this, arguments);

            return await task;
        }

        /// <inheritdoc/>
        public async virtual Task<IEnumerable<TEntity>> GetWithConditionAsync(QuerySettings querySettings)
        {
            var sqlQuery = new StringBuilder(_selectQueryWithCondition);

            sqlQuery.Replace("*condition_field*", querySettings.ConditionField);
            sqlQuery.Replace("*condition_sign*", GetConditionSign(querySettings.ConditionType));

            sqlQuery = GetFinalQuery(sqlQuery, querySettings.CertainNumberOfRows, querySettings.NeedSortDescendingOrder);

            using var dbConnection = _dbConnectionKeeper.GetDbConnection();

            var neededMethod = GetMethodSelectQuery();

            var arguments = new object[3] { dbConnection, sqlQuery.ToString(), querySettings.ConditionFieldValue };

            var task = (Task<IEnumerable<TEntity>>)neededMethod.Invoke(this, arguments);

            return await task;
        }

        /// <inheritdoc/>
        public async virtual Task<IEnumerable<TEntity>> GetWithoutConditionJoinAsync(int certainNumberOfRows = -1, bool needSortDescendingOrder = false)
        {
            var sqlQuery = new StringBuilder(_selectQueryWithoutConditionJoin);

            sqlQuery = GetFinalQuery(sqlQuery, certainNumberOfRows, needSortDescendingOrder);

            Console.WriteLine(sqlQuery);

            using var dbConnection = _dbConnectionKeeper.GetDbConnection();

            return await GetWithRelatedEntitiesAsync(dbConnection, sqlQuery.ToString());
        }

        /// <inheritdoc/>
        public async virtual Task<IEnumerable<TEntity>> GetWithoutJoinAsync(QuerySettings querySettings)
        {
            var sqlQuery = new StringBuilder(_selectQueryWithoutJoin);

            sqlQuery.Replace("*condition_field*", querySettings.ConditionField);
            sqlQuery.Replace("*condition_sign*", GetConditionSign(querySettings.ConditionType));

            sqlQuery = GetFinalQuery(sqlQuery, querySettings.CertainNumberOfRows, querySettings.NeedSortDescendingOrder);

            using var dbConnection = _dbConnectionKeeper.GetDbConnection();

            return await GetWithRelatedEntitiesAsync(dbConnection, sqlQuery.ToString(), querySettings.ConditionFieldValue);
        }

        /// <summary>
        /// Получить окончательный запрос (формирование запроса со всеми нужными дополнениями)
        /// </summary>
        /// <param name="sqlQuery"> Запрос, по которому строится окончательный вариант </param>
        /// <param name="certainNumberOfRows"> Максимальное количество значений, которое нужно получить из базы данных </param>
        /// <param name="needSortDescendingOrder"> Необходимо ли отсортировать значения по Id в порядке убывания </param>
        /// <returns> Окончательный запрос </returns>
        private StringBuilder GetFinalQuery(StringBuilder sqlQuery, int certainNumberOfRows, bool needSortDescendingOrder)
        {
            if (certainNumberOfRows > 0)
            {
                var certainNumberOfRowsForQuery = $"TOP {certainNumberOfRows} ";
                sqlQuery.Insert(7, certainNumberOfRowsForQuery);
            }

            if (needSortDescendingOrder)
            {
                var orderBy = $" ORDER BY {_sqlTableName}.Id DESC";
                sqlQuery.Insert(sqlQuery.Length, orderBy);
            }

            return sqlQuery;
        }

        /// <summary>
        /// Получить знак условия
        /// </summary>
        /// <param name="conditionType"> Тип условия </param>
        /// <returns> Знак условия </returns>
        private string GetConditionSign(ConditionType conditionType) 
            => conditionType switch
            {
                ConditionType.MORE => ">",
                ConditionType.LESS => "<",
                ConditionType.EQUALLY => "=",
                ConditionType.NOT_EQUALLY => "!=",
                _ => throw new ArgumentException(message: "Incorrect enum value", paramName: nameof(conditionType))
            };

        /// <summary>
        /// Получить метод SELECT-запроса (Рефлексия)
        /// </summary>
        /// <returns> Метод SELECT-запроса </returns>
        private MethodInfo GetMethodSelectQuery()
        {
            var relatedTypes = _relatedEntitiesDictionary.Keys.ToArray();

            var methodsInfo = typeof(StandardRepository<TEntity>).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(methodInfo => methodInfo).Where(methodInfo => methodInfo.Name == "GetWithRelatedEntitiesAsync").ToList();

            var neededMethod = methodsInfo[relatedTypes.Length];

            return relatedTypes.Length == 0
                ? neededMethod
                : neededMethod.MakeGenericMethod(relatedTypes);
        }

        /// <inheritdoc/>
        public async virtual Task<int> UpdateAsync(TEntity updateData)
        {
            using var dbConnection = _dbConnectionKeeper.GetDbConnection();

            return await dbConnection.ExecuteAsync(_updateQuery, updateData);
        }

        /// <inheritdoc/>
        public async virtual Task<int> DeleteAsync(int id)
        {
            using var dbConnection = _dbConnectionKeeper.GetDbConnection();

            return await dbConnection.ExecuteAsync(_deleteQuery, new { id });
        }

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE </param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity>(sqlQuery, new { value });

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <typeparam name="TFirstRelatedEntity"> Первая связанная сущность </typeparam>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE </param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync<TFirstRelatedEntity>(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity, TFirstRelatedEntity, TEntity>(
                sqlQuery,
                (currentEntity, firstRelatedEntity) =>
                SetValuesForRelatedEntities(currentEntity, firstRelatedEntity),
                new { value });

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <typeparam name="TFirstRelatedEntity"> Первая связанная сущность </typeparam>
        /// <typeparam name="TSecondRelatedEntity"> Вторая связанная сущность </typeparam>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE</param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync<TFirstRelatedEntity, TSecondRelatedEntity>(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity, TFirstRelatedEntity, TSecondRelatedEntity, TEntity>(
                sqlQuery,
                (currentEntity, firstRelatedEntity, secondRelatedEntity) =>
                SetValuesForRelatedEntities(currentEntity, firstRelatedEntity, secondRelatedEntity),
                new { value });

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <typeparam name="TFirstRelatedEntity"> Первая связанная сущность </typeparam>
        /// <typeparam name="TSecondRelatedEntity"> Вторая связанная сущность </typeparam>
        /// <typeparam name="TThirdRelatedEntity"> Третья связанная сущность </typeparam>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE </param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync<TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity>(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity, TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TEntity>(
                sqlQuery,
                (currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity) =>
                SetValuesForRelatedEntities(currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity),
                new { value });

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <typeparam name="TFirstRelatedEntity"> Первая связанная сущность </typeparam>
        /// <typeparam name="TSecondRelatedEntity"> Вторая связанная сущность </typeparam>
        /// <typeparam name="TThirdRelatedEntity"> Третья связанная сущность </typeparam>
        /// <typeparam name="TFourthRelatedEntity"> Четвёртая связанная сущность </typeparam>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE </param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync<TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TFourthRelatedEntity>(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity, TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TFourthRelatedEntity, TEntity>(
                sqlQuery,
                (currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity, fourthRelatedEntity) =>
                SetValuesForRelatedEntities(currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity, fourthRelatedEntity),
                new { value });

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <typeparam name="TFirstRelatedEntity"> Первая связанная сущность </typeparam>
        /// <typeparam name="TSecondRelatedEntity"> Вторая связанная сущность </typeparam>
        /// <typeparam name="TThirdRelatedEntity"> Третья связанная сущность </typeparam>
        /// <typeparam name="TFourthRelatedEntity"> Четвёртая связанная сущность </typeparam>
        /// <typeparam name="TFifthRelatedEntity"> Пятая связанная сущность </typeparam>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE </param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync<TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TFourthRelatedEntity, TFifthRelatedEntity>(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity, TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TFourthRelatedEntity, TFifthRelatedEntity, TEntity>(
                sqlQuery,
                (currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity, fourthRelatedEntity, fifthRelatedEntity) =>
                SetValuesForRelatedEntities(currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity, fourthRelatedEntity, fifthRelatedEntity),
                new { value });

        /// <summary>
        /// Получить данные со связанными сущностями (связь один-к-одному)
        /// </summary>
        /// <typeparam name="TFirstRelatedEntity"> Первая связанная сущность </typeparam>
        /// <typeparam name="TSecondRelatedEntity"> Вторая связанная сущность </typeparam>
        /// <typeparam name="TThirdRelatedEntity"> Третья связанная сущность </typeparam>
        /// <typeparam name="TFourthRelatedEntity"> Четвёртая связанная сущность </typeparam>
        /// <typeparam name="TFifthRelatedEntity"> Пятая связанная сущность </typeparam>
        /// <typeparam name="TSixthRelatedEntity"> Шестая связанная сущность </typeparam>
        /// <param name="connection"> Соединение с базой данных </param>
        /// <param name="sqlQuery"> SQL-запрос </param>
        /// <param name="value"> Необязательный параметр, который используется в случае SELECT-запроса с WHERE </param>
        /// <returns> Данные со связанными сущностями </returns>
        private async Task<IEnumerable<TEntity>> GetWithRelatedEntitiesAsync<TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TFourthRelatedEntity, TFifthRelatedEntity, TSixthRelatedEntity>(IDbConnection connection, string sqlQuery, object value = null)
            => await connection.QueryAsync<TEntity, TFirstRelatedEntity, TSecondRelatedEntity, TThirdRelatedEntity, TFourthRelatedEntity, TFifthRelatedEntity, TSixthRelatedEntity, TEntity>(
                sqlQuery,
                (currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity, fourthRelatedEntity, fifthRelatedEntity, sixthRelatedEntity) =>
                SetValuesForRelatedEntities(currentEntity, firstRelatedEntity, secondRelatedEntity, thirdRelatedEntity, fourthRelatedEntity, fifthRelatedEntity, sixthRelatedEntity),
                new { value });

        /// <summary>
        /// Установить значения связанных сущностей в сущность, с которой работает репозиторий
        /// </summary>
        /// <param name="currentEntity"> Сущность, с которой работает репозиторий </param>
        /// <param name="relatedEntities"> Связанные сущности </param>
        /// <returns> Сущность, с которой работает репозиторий </returns>
        private TEntity SetValuesForRelatedEntities(TEntity currentEntity, params object[] relatedEntities)
        {
            var properties = currentEntity.GetType().GetProperties();

            foreach (var relatedEntity in relatedEntities)
            {
                if (relatedEntity is not null)
                    properties[_relatedEntitiesDictionary[relatedEntity.GetType()]].SetValue(currentEntity, relatedEntity);
                properties[0].GetValue(currentEntity, null);
            }

            return currentEntity;
        }
    }
}
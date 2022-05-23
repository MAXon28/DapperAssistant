using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DapperAssistant
{
    /// <summary>
    /// Интерфейс паттерна "Репозиторий" со стандартными функциями
    /// </summary>
    /// <typeparam name="TEntity"> Тип, который соответствует таблице в базе данных </typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Добавить значение (вызывает INSERT-запрос)
        /// </summary>
        /// <param name="newData"> Добавляемая сущность </param>
        public Task AddAsync(TEntity newData);

        /// <summary>
        /// Добавить значение (вызывает INSERT-запрос)
        /// </summary>
        /// <param name="newData"> Добавляемая сущность </param>
        /// <param name="dbConnection"> Соединение с базой данных </param>
        /// <param name="transaction"> Транзакция </param>
        public Task AddAsync(TEntity newData, IDbConnection dbConnection = null, IDbTransaction transaction = null);

        /// <summary>
        /// Получить список выбранных значений (вызывает SELECT-запрос)
        /// </summary>
        /// <param name="certainNumberOfRows"> Указывает на максимальное количество значений, которое нужно получить из базы данных (запрос с предложение "TOP"). Иначе считываются все возможные значения </param>
        /// <param name="needSortDescendingOrder"> Указывает на необходимость отсортировать значения по Id в порядке убывания </param>
        /// <returns> Список выбранных значений из базы данных </returns>
        public Task<IEnumerable<TEntity>> GetAsync(int certainNumberOfRows = -1, bool needSortDescendingOrder = false);

        /// <summary>
        /// Получить список выбранных значений с условием (вызывает SELECT-запрос с WHERE)
        /// </summary>
        /// <param name="querySettings"> Настройки запроса </param>
        /// <returns> Список выбранных значений из базы данных </returns>
        public Task<IEnumerable<TEntity>> GetWithConditionAsync(QuerySettings querySettings);

        /// <summary>
        /// Получить список выбранных значений (вызывает SELECT-запрос без WHERE и JOIN)
        /// </summary>
        /// <param name="certainNumberOfRows"> Указывает на максимальное количество значений, которое нужно получить из базы данных (запрос с предложение "TOP"). Иначе считываются все возможные значения </param>
        /// <param name="needSortDescendingOrder"> Указывает на необходимость отсортировать значения по Id в порядке убывания </param>
        /// <returns> Список выбранных значений из базы данных </returns>
        public Task<IEnumerable<TEntity>> GetWihoutConditionJoinAsync(int certainNumberOfRows = -1, bool needSortDescendingOrder = false);

        /// <summary>
        /// Получить список выбранных значений (вызывает SELECT-запрос С WHERE, но без JOIN)
        /// </summary>
        /// <param name="querySettings"> Настройки запроса </param>
        /// <returns> Список выбранных значений из базы данных </returns>
        public Task<IEnumerable<TEntity>> GetWihoutJoinAsync(QuerySettings querySettings);

        /// <summary>
        /// Обновить значение (вызывает UPDATE-запрос)
        /// </summary>
        /// <param name="updateData"> Обновляемая сущность </param>
        public Task UpdateAsync(TEntity updateData);

        /// <summary>
        /// Удалить значение (вызывает DELETE-запрос)
        /// </summary>
        /// <param name="id"> Идентификатор удаляемой сущности </param>
        public Task DeleteAsync(int id);
    }
}
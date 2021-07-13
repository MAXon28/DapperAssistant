using System.Collections.Generic;
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
        /// Получить список выбранных значений (вызывает SELECT-запрос)
        /// </summary>
        /// <param name="certainNumberOfRows"> Необязательный параметр. Указывает на максимальное количество значений, которое нужно получить из базы данных (запрос с предложение "TOP"). Иначе считываются все возможные значения </param>
        /// <param name="needSortDescendingOrder"> Необязательный параметр. Указывает на необходимость отсортировать значения по Id в порядке убывания </param>
        /// <returns> Список выбранных значений из базы данных </returns>
        public Task<List<TEntity>> GetAsync(int certainNumberOfRows = -1, bool needSortDescendingOrder = false);

        /// <summary>
        /// Получить список выбранных значений с условием (вызывает SELECT-запрос с WHERE)
        /// </summary>
        /// <param name="conditionField"> Поле условия </param>
        /// <param name="conditionType"> Тип условия (больше, меньше или равно) </param>
        /// <param name="conditionFieldValue"> Значение условия </param>
        /// <param name="certainNumberOfRows"> Необязательный параметр. Указывает на максимальное количество значений, которое нужно получить из базы данных (запрос с предложение "TOP"). Иначе считываются все возможные значения </param>
        /// <param name="needSortDescendingOrder"> Необязательный параметр. Указывает на необходимость отсортировать значения по Id в порядке убывания </param>
        /// <returns> Список выбранных значений из базы данных </returns>
        public Task<List<TEntity>> GetWithConditionAsync(string conditionField, ConditionType conditionType, object conditionFieldValue, int certainNumberOfRows = -1, bool needSortDescendingOrder = false);

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
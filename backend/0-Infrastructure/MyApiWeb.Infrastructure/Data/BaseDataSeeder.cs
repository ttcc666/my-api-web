using Microsoft.Extensions.Logging;
using MyApiWeb.Repository;
using SqlSugar;

namespace MyApiWeb.Infrastructure.Data
{
    /// <summary>
    /// 数据种子基类
    /// </summary>
    public abstract class BaseDataSeeder<T> : IDataSeeder where T : class, new()
    {
        public abstract string SeedName { get; }

        protected readonly SqlSugarDbContext DbContext;
        protected readonly ILogger Logger;

        protected BaseDataSeeder(SqlSugarDbContext dbContext, ILogger logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        /// <summary>
        /// 初始化种子数据
        /// </summary>
        public async Task SeedAsync(bool forceReinitialize = false)
        {
            try
            {
                if (forceReinitialize)
                {
                    await ClearDataAsync();
                    Logger.LogWarning("已清除 {Type} 数据，开始重新初始化", typeof(T).Name);
                }

                await SeedDataAsync();
                Logger.LogInformation("{Type} 数据种子初始化完成", typeof(T).Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Type} 数据种子初始化失败", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        protected virtual async Task ClearDataAsync()
        {
            await DbContext.Db.Deleteable<T>().ExecuteCommandAsync();
        }

        /// <summary>
        /// 执行种子数据逻辑
        /// </summary>
        protected abstract Task SeedDataAsync();

        /// <summary>
        /// 批量插入不存在的数据
        /// </summary>
        protected async Task<int> BulkInsertIfNotExistsAsync<TEntity>(
            List<TEntity> entities,
            Func<TEntity, Task<bool>> existsCheck) where TEntity : class, new()
        {
            var toInsert = new List<TEntity>();
            foreach (var entity in entities)
            {
                if (!await existsCheck(entity))
                {
                    toInsert.Add(entity);
                }
            }

            if (toInsert.Any())
            {
                await DbContext.Db.Insertable(toInsert).ExecuteCommandAsync();
            }

            return toInsert.Count;
        }
    }
}

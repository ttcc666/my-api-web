using System.Linq.Expressions;

namespace MyApiWeb.Repository.Interfaces
{
    /// <summary>
    /// 通用仓储接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>实体对象</returns>
        Task<T?> GetByIdAsync(object id);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>实体列表</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// 根据条件查询实体列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>实体列表</returns>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 根据条件查询单个实体
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>实体对象</returns>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 插入实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否成功</returns>
        Task<bool> InsertAsync(T entity);

        /// <summary>
        /// 批量插入实体
        /// </summary>
        /// <param name="entities">实体列表</param>
        /// <returns>是否成功</returns>
        Task<bool> InsertRangeAsync(List<T> entities);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteAsync(object id);

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>受影响的行数</returns>
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取实体总数
        /// </summary>
        /// <returns>总数</returns>
        Task<int> CountAsync();

        /// <summary>
        /// 根据条件获取实体总数
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>总数</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
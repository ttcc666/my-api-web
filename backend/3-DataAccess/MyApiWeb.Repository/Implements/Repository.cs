using MyApiWeb.Repository.Interfaces;
using SqlSugar;
using System.Linq.Expressions;

namespace MyApiWeb.Repository.Implements
{
    /// <summary>
    /// 通用仓储实现
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        private readonly SqlSugarDbContext _context;
        private readonly ISugarQueryable<T> _queryable;

        public Repository(SqlSugarDbContext context)
        {
            _context = context;
            _queryable = _context.Queryable<T>();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _context.Db.Queryable<T>().InSingleAsync(id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Db.Queryable<T>().ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Db.Queryable<T>().Where(predicate).ToListAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Db.Queryable<T>().Where(predicate).FirstAsync();
        }

        public async Task<bool> InsertAsync(T entity)
        {
            return await _context.Db.Insertable(entity).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> InsertRangeAsync(List<T> entities)
        {
            return await _context.Db.Insertable(entities).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            return await _context.Db.Updateable(entity).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            return await _context.Db.Deleteable(entity).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> DeleteAsync(object id)
        {
            return await _context.Db.Deleteable<T>().In(id).ExecuteCommandAsync() > 0;
        }

        public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Db.Deleteable<T>().Where(predicate).ExecuteCommandAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Db.Queryable<T>().Where(predicate).AnyAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Db.Queryable<T>().CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Db.Queryable<T>().Where(predicate).CountAsync();
        }
    }
}
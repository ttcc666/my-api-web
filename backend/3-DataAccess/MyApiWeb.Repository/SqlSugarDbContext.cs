using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;
using MyApiWeb.Models.Interfaces;
using SqlSugar;

namespace MyApiWeb.Repository
{
    /// <summary>
    /// SqlSugar 数据库上下文
    /// </summary>
    public class SqlSugarDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlSugarDbContext> _logger;
        private readonly ICurrentUser _currentUser;

        public SqlSugarScope Db { get; private set; } = null!;

        public SqlSugarDbContext(IConfiguration configuration, ILogger<SqlSugarDbContext> logger, ICurrentUser currentUser)
        {
            _configuration = configuration;
            _logger = logger;
            _currentUser = currentUser;

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            Db = new SqlSugarScope(new ConnectionConfig()
            {
                DbType = DbType.SqlServer,
                ConnectionString = connectionString,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                ConfigureExternalServices = new ConfigureExternalServices()
                {
                    EntityService = (c, p) =>
                    {
                        // 配置实体名称映射 - 使用正确的属性名称
                    }
                }
            },
            db =>
            {
                // 配置日志
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    _logger.LogInformation("SQL执行: {Sql}", sql);
                };

                db.Aop.OnError = ex =>
                {
                    _logger.LogError(ex, "SqlSugar执行错误");
                };

                // 配置数据库表创建
                db.Aop.OnLogExecuted = (sql, pars) =>
                {
                    _logger.LogDebug("SQL执行完成, 耗时: {ElapsedMilliseconds}ms", db.Ado.SqlExecutionTime.TotalMilliseconds);
                };

                // 数据审计
                db.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    if (entityInfo.EntityValue is EntityBase entity)
                    {
                        if (entityInfo.OperationType == DataFilterType.InsertByObject)
                        {
                            entity.CreatorId = _currentUser.Id ?? Guid.Empty;
                            entity.CreationTime = DateTimeOffset.Now;
                        }
                        else if (entityInfo.OperationType == DataFilterType.UpdateByObject)
                        {
                            entity.LastModifierId = _currentUser.Id;
                            entity.LastModificationTime = DateTimeOffset.Now;
                        }
                    }
                };
            });

            var enableMigrations = _configuration.GetSection("DatabaseSettings:EnableMigrations").Get<bool>();
            if (enableMigrations)
            {
                // 创建数据库（如果不存在）
                CreateDatabase();

                // 创建表（如果不存在）
                CreateTables();
            }
        }

        private void CreateDatabase()
        {
            try
            {
                Db.DbMaintenance.CreateDatabase();
                _logger.LogInformation("数据库创建成功或已存在");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建数据库失败");
                throw;
            }
        }

        private void CreateTables()
        {
            try
            {
                // 创建种子历史表（优先创建）
                Db.CodeFirst.InitTables(typeof(SeedHistory));

                // 创建用户表
                Db.CodeFirst.InitTables(typeof(User), typeof(RefreshToken));

                // 创建 RBAC 相关表
                Db.CodeFirst.InitTables(
                    typeof(Role),
                    typeof(Permission),
                    typeof(UserRole),
                    typeof(RolePermission),
                    typeof(UserPermission),
                    typeof(Menu)
                );

                // 创建在线用户表
                Db.CodeFirst.InitTables(typeof(OnlineUser));

                _logger.LogInformation("数据库表创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建数据库表失败");
                throw;
            }
        }

        /// <summary>
        /// 获取可查询对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>可查询对象</returns>
        public ISugarQueryable<T> Queryable<T>() where T : class, new()
        {
            return Db.Queryable<T>();
        }

        /// <summary>
        /// 获取可插入对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> Insertable<T>(T entity) where T : class, new()
        {
            return Db.Insertable(entity);
        }

        /// <summary>
        /// 获取可插入对象（批量）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">实体列表</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> Insertable<T>(List<T> entities) where T : class, new()
        {
            return Db.Insertable(entities);
        }

        /// <summary>
        /// 获取可更新对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns>可更新对象</returns>
        public IUpdateable<T> Updateable<T>(T entity) where T : class, new()
        {
            return Db.Updateable(entity);
        }

        /// <summary>
        /// 获取可删除对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>可删除对象</returns>
        public IDeleteable<T> Deleteable<T>() where T : class, new()
        {
            return Db.Deleteable<T>();
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <returns>事务对象</returns>
        public SqlSugarTransaction BeginTran()
        {
            return Db.UseTran();
        }
    }
}

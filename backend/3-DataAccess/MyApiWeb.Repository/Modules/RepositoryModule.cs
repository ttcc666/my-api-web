using Autofac;
using MyApiWeb.Repository.Implements;
using MyApiWeb.Repository.Interfaces;

namespace MyApiWeb.Repository.Modules
{
    /// <summary>
    /// Repository层Autofac模块
    /// </summary>
    public class RepositoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // 注册数据库上下文
            builder.RegisterType<SqlSugarDbContext>()
                   .AsSelf()
                   .SingleInstance();

            // 注册通用Repository
            builder.RegisterGeneric(typeof(Repository<>))
                   .As(typeof(IRepository<>))
                   .InstancePerLifetimeScope();

            // 这里可以注册具体的Repository实现
            // 例如：builder.RegisterType<UserRepository>().As<IUserRepository>();
        }
    }
}
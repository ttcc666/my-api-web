using Autofac;
using MyApiWeb.Models.Interfaces;
using MyApiWeb.Services.Interfaces;
using MyApiWeb.Services.Implements;

namespace MyApiWeb.Services.Modules
{
    /// <summary>
    /// Service层Autofac模块
    /// </summary>
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // 注册用户服务
            builder.RegisterType<UserService>()
                   .As<IUserService>()
                   .InstancePerLifetimeScope();
            
            builder.RegisterType<CurrentUser>()
                   .As<ICurrentUser>()
                   .InstancePerLifetimeScope();
            
            builder.RegisterType<TokenService>()
                   .As<ITokenService>()
                   .InstancePerLifetimeScope();

            // 注册 RBAC 相关服务
            builder.RegisterType<RoleService>()
                   .As<IRoleService>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<PermissionService>()
                   .As<IPermissionService>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<MenuService>()
                   .As<IMenuService>()
                   .InstancePerLifetimeScope();
 
             // 这里可以注册其他服务
             // 例如：builder.RegisterType<ProductService>().As<IProductService>();
        }
    }
}

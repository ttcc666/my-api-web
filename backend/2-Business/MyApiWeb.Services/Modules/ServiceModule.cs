using Autofac;
using MyApiWeb.Models.Interfaces;
using MyApiWeb.Services.Implements.Common;
using MyApiWeb.Services.Implements.System;
using MyApiWeb.Services.Implements.Auth;
using MyApiWeb.Services.Implements.Hub;
using MyApiWeb.Services.Interfaces.System;
using MyApiWeb.Services.Interfaces.Auth;
using MyApiWeb.Services.Interfaces.Hub;

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

            builder.RegisterType<DeviceService>()
                   .As<IDeviceService>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<OnlineUserService>()
                   .As<IOnlineUserService>()
                   .InstancePerLifetimeScope();

            // 注册 CAP 订阅者
            builder.RegisterType<Subscribers.OnlineUserEventSubscriber>()
                   .AsSelf()
                   .InstancePerLifetimeScope();

            // 这里可以注册其他服务
            // 例如：builder.RegisterType<ProductService>().As<IProductService>();
        }
    }
}

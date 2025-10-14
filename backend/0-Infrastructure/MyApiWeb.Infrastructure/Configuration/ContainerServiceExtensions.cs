using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyApiWeb.Repository.Modules;
using MyApiWeb.Services.Modules;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// Autofac 容器服务配置扩展
    /// </summary>
    public static class ContainerServiceExtensions
    {
        /// <summary>
        /// 配置 Autofac 依赖注入容器
        /// </summary>
        /// <param name="host">主机构建器</param>
        /// <param name="registerAdditionalModules">注册额外的模块(如数据种子服务)</param>
        public static IHostBuilder AddAutofacContainer(
            this IHostBuilder host,
            Action<ContainerBuilder>? registerAdditionalModules = null)
        {
            // 使用 Autofac 替换默认 DI 容器
            host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // 配置 Autofac 容器
            host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                // 注册核心模块
                containerBuilder.RegisterModule<RepositoryModule>();
                containerBuilder.RegisterModule<ServiceModule>();

                // 允许外部注册额外的服务(如数据种子服务)
                registerAdditionalModules?.Invoke(containerBuilder);
            });

            return host;
        }
    }
}

using Autofac;
using MyApiWeb.Infrastructure.Configuration;
using MyApiWeb.Infrastructure.Data;
using Serilog;

try
{
    Log.Information("启动应用程序");

    var builder = WebApplication.CreateBuilder(args);

    // 配置 Serilog
    builder.Host.AddSerilogLogging();

    // 配置 Autofac 容器
    builder.Host.AddAutofacContainer(containerBuilder =>
    {
        // 注册 RBAC 数据种子服务
        containerBuilder.RegisterType<MyApiWeb.Infrastructure.Data.RbacDataSeeder>()
                       .AsSelf()
                       .InstancePerLifetimeScope();

        containerBuilder.RegisterType<MyApiWeb.Infrastructure.Data.MenuDataSeeder>()
                       .AsSelf()
                       .InstancePerLifetimeScope();
    });

    // 添加服务到容器
    builder.Services.AddCustomControllers();
    builder.Services.AddCustomCors(builder.Configuration, builder.Environment);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddCapMessageBus(builder.Configuration);

    var app = builder.Build();

    // 配置中间件管道
    app.UseCustomMiddlewarePipeline();

    // 初始化数据种子
    DataSeeder.Seed(app);

    Log.Information("应用程序启动完成");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序启动失败");
}
finally
{
    Log.CloseAndFlush();
}

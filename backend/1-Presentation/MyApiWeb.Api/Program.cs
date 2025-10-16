using Autofac;
using MyApiWeb.Api.Controllers;
using MyApiWeb.Infrastructure.Configuration;
using MyApiWeb.Infrastructure.Data;
using MyApiWeb.Services.Interfaces;
using Serilog;

try
{
    Log.Information("启动应用程序");

    var builder = WebApplication.CreateBuilder(args);

    // 加载模块化配置文件
    var environment = builder.Environment.EnvironmentName;
    Log.Information("当前环境: {Environment}", environment);

    // 定义需要加载的模块配置文件
    var configModules = new[] { "database", "jwt", "cors", "cap", "serilog", "onlineuser" };

    foreach (var module in configModules)
    {
        // 加载基础配置
        builder.Configuration.AddJsonFile(
            $"appsettings/{module}.json",
            optional: true,
            reloadOnChange: true);

        // 加载环境特定配置（会覆盖基础配置）
        builder.Configuration.AddJsonFile(
            $"appsettings/{module}.{environment}.json",
            optional: true,
            reloadOnChange: true);

        Log.Information("已加载模块配置: {Module}", module);
    }

    Log.Information("所有模块化配置文件加载完成");

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

    builder.Services.AddCustomControllers();

    builder.Services.AddCustomCors(builder.Configuration, builder.Environment);

    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddSignalRWithJwtSupport(builder.Configuration);

    builder.Services.AddSwaggerDocumentation();

    builder.Services.AddCapMessageBus(builder.Configuration);

    // 注册 Quartz.NET 定时任务调度
    builder.Services.AddQuartzWithJobs(builder.Configuration);

    var app = builder.Build();

    // 配置中间件管道
    app.UseCustomMiddlewarePipeline();

    // 映射 SignalR Hub 端点
    app.MapHub<ChatHub>("/hubs/chat");

    // 初始化数据种子
    DataSeeder.Seed(app);

    // 清除应用重启前的在线用户数据
    await ClearOnlineUsersOnStartup(app);

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

/// <summary>
/// 清除应用启动前的在线用户数据
/// </summary>
static async Task ClearOnlineUsersOnStartup(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var onlineUserService = scope.ServiceProvider.GetRequiredService<IOnlineUserService>();

        Log.Information("开始清除应用重启前的在线用户数据...");
        var count = await onlineUserService.ClearAllOnlineUsersAsync();

        if (count > 0)
        {
            Log.Warning("已清除 {Count} 个在线用户记录", count);
        }
        else
        {
            Log.Information("无需清除在线用户记录");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "清除在线用户数据失败");
    }
}

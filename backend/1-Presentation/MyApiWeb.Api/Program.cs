using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MyApiWeb.Repository.Modules;
using MyApiWeb.Services.Modules;
using MyApiWeb.Api.Middlewares;
using MyApiWeb.Api.Data;
using Serilog;
using Serilog.Events;

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        rollOnFileSizeLimit: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("启动应用程序");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog 替换默认日志提供程序
    builder.Host.UseSerilog();

    // 使用 Autofac 替换默认 DI 容器
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

    // 配置 Autofac 容器
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        // 注册自定义模块
        containerBuilder.RegisterModule<RepositoryModule>();
        containerBuilder.RegisterModule<ServiceModule>();

        // 注册 RBAC 数据种子服务
        containerBuilder.RegisterType<MyApiWeb.Api.Data.RbacDataSeeder>()
                       .AsSelf()
                       .InstancePerLifetimeScope();
    });

    // 添加服务到容器
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();

    // 配置 CORS - 分环境安全策略
    builder.Services.AddCors(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // 开发环境：允许本地前端开发服务器
            options.AddPolicy("DevelopmentPolicy", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:3000",    // Vue 开发服务器
                        "http://localhost:5173",    // Vite 开发服务器
                        "http://127.0.0.1:3000",
                        "http://127.0.0.1:5173"
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        }
        else
        {
            // 生产环境：严格的域名白名单
            var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                               ?? new[] { "https://yourdomain.com" };

            options.AddPolicy("ProductionPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        }
    });

    // 配置 JWT 认证
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])),
            ClockSkew = TimeSpan.Zero
        };

        // 可选：处理因令牌过期导致的认证失败事件，方便前端识别
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

    // 添加授权服务
    builder.Services.AddAuthorization();

    // 配置 Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "My API Web",
            Version = "v1",
            Description = "基于 .NET 9 + SqlSugar + Vue 3 的三层架构 Web API"
        });

        // 添加 JWT 认证支持
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
        });
    });

    var app = builder.Build();

    // 配置 HTTP 请求管道
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API Web v1");
            c.RoutePrefix = string.Empty; // 设置 Swagger UI 为默认页面
        });
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseHttpsRedirection();

    // 启用 CORS - 根据环境选择策略
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevelopmentPolicy");
    }
    else
    {
        app.UseCors("ProductionPolicy");
    }

    // 启用认证和授权
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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
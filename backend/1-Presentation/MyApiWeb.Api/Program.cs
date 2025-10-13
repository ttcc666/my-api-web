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

var builder = WebApplication.CreateBuilder(args);

// 使用 Autofac 替换默认 DI 容器
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// 配置 Autofac 容器
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // 注册自定义模块
    containerBuilder.RegisterModule<RepositoryModule>();
    containerBuilder.RegisterModule<ServiceModule>();
});

// 添加服务到容器
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
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

// 启用 CORS
app.UseCors("AllowAll");

// 启用认证和授权
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

DataSeeder.Seed(app);

app.Run();
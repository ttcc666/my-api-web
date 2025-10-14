# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是一个基于 **.NET 9** 的三层架构 Web API 后端项目,采用 **SqlSugar ORM**、**Autofac 依赖注入**、**JWT 认证**和 **RBAC 权限管理系统**。项目配套的前端位于 `../frontend` 目录,使用 **Vue 3 + TypeScript + Vite**。

## 技术栈

- **.NET 9** (Web API)
- **SqlSugar** 5.1.4.166 (ORM)
- **Autofac** 9.0.0 (依赖注入容器)
- **JWT Bearer Authentication** (身份认证)
- **Serilog** (结构化日志)
- **Swagger/OpenAPI** (API 文档)
- **SQL Server** (数据库)

## 项目架构

项目采用经典的**三层架构设计**,按照依赖关系从上到下分为:

```
1-Presentation (表现层)
  └── MyApiWeb.Api
      ├── Controllers/      # API控制器
      ├── Middlewares/      # 全局中间件(异常处理等)
      ├── Helpers/          # 辅助类(API响应封装等)
      ├── Data/             # 数据种子服务
      └── Program.cs        # 应用程序入口和配置

2-Business (业务层)
  ├── MyApiWeb.Models
  │   ├── Entities/         # 实体类(继承EntityBase)
  │   ├── DTOs/             # 数据传输对象
  │   ├── Exceptions/       # 自定义异常
  │   └── Interfaces/       # 跨层接口(如ICurrentUser)
  │
  └── MyApiWeb.Services
      ├── Interfaces/       # 服务接口
      ├── Implements/       # 服务实现
      └── Modules/          # Autofac模块注册

3-DataAccess (数据访问层)
  └── MyApiWeb.Repository
      ├── Interfaces/       # 仓储接口
      ├── Implements/       # 仓储实现
      ├── Modules/          # Autofac模块注册
      └── SqlSugarDbContext.cs  # SqlSugar数据库上下文
```

### 架构关键特性

1. **依赖注入**: 使用 Autofac 替换默认 DI 容器,通过 `Module` 模式注册服务
   - [RepositoryModule.cs](3-DataAccess/MyApiWeb.Repository/Modules/RepositoryModule.cs)
   - [ServiceModule.cs](2-Business/MyApiWeb.Services/Modules/ServiceModule.cs)

2. **数据库管理**: SqlSugar ORM + Code First
   - [SqlSugarDbContext.cs](3-DataAccess/MyApiWeb.Repository/SqlSugarDbContext.cs) 提供统一的数据库上下文
   - 自动创建数据库和表结构(通过 `appsettings.json` 中的 `EnableMigrations` 控制)
   - 自动审计功能(创建人、修改人、时间戳自动填充)

3. **认证授权**: JWT + RBAC
   - JWT Token 配置在 [Program.cs:109-184](1-Presentation/MyApiWeb.Api/Program.cs#L109-L184)
   - RBAC 实体: `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `UserPermission`
   - 数据种子服务在 [DataSeeder.cs](1-Presentation/MyApiWeb.Api/Data/DataSeeder.cs) 中初始化默认管理员账户(admin/123456)

4. **统一响应格式**: 所有 API 返回 `ApiResponse<T>` 格式
   - 通过 [ApiControllerBase.cs](1-Presentation/MyApiWeb.Api/Controllers/ApiControllerBase.cs) 提供统一的成功/错误响应方法
   - 通过 [GlobalExceptionMiddleware.cs](1-Presentation/MyApiWeb.Api/Middlewares/GlobalExceptionMiddleware.cs) 捕获并转换异常

5. **日志系统**: Serilog 结构化日志
   - 控制台输出 + 文件滚动存储
   - 日志文件存储在 `logs/` 目录,按天滚动,保留 30 天

## 核心命令

### 构建和运行

```bash
# 恢复依赖
dotnet restore backend.sln

# 构建整个解决方案
dotnet build backend.sln

# 构建特定配置
dotnet build backend.sln --configuration Release

# 运行 API 项目(开发环境)
dotnet run --project 1-Presentation/MyApiWeb.Api/MyApiWeb.Api.csproj

# 监听模式运行(自动重启)
dotnet watch --project 1-Presentation/MyApiWeb.Api/MyApiWeb.Api.csproj
```

### 清理

```bash
# 清理所有构建产物
dotnet clean backend.sln

# 清理并重新构建
dotnet clean backend.sln && dotnet build backend.sln
```

### 数据库管理

**注意**: 本项目使用 SqlSugar Code First,数据库结构通过实体类定义,不需要手动迁移。

- 数据库连接字符串配置在 [appsettings.json](1-Presentation/MyApiWeb.Api/appsettings.json) 的 `ConnectionStrings:DefaultConnection`
- 首次运行时,SqlSugar 会自动创建数据库和表(需 `DatabaseSettings:EnableMigrations=true`)
- 种子数据会在应用启动时自动插入(admin 用户、RBAC 基础数据)

## 重要配置说明

### appsettings.json 关键配置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApiWebDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "EnableMigrations": true  // 生产环境建议设为 false
  },
  "JwtSettings": {
    "Secret": "替换为安全的密钥!",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  },
  "CorsSettings": {
    "AllowedOrigins": ["https://yourdomain.com"]  // 生产环境域名白名单
  }
}
```

### 环境变量支持

- 可以通过环境变量覆盖配置(如 `ConnectionStrings__DefaultConnection`)
- JWT Secret **必须**在生产环境中通过环境变量或密钥管理服务设置

## 开发规范

### 添加新实体

1. 在 `2-Business/MyApiWeb.Models/Entities/` 创建实体类,继承 `EntityBase`
2. 在 [SqlSugarDbContext.cs](3-DataAccess/MyApiWeb.Repository/SqlSugarDbContext.cs) 的 `CreateTables()` 方法中添加 `Db.CodeFirst.InitTables(typeof(YourEntity))`
3. 重启应用,SqlSugar 会自动创建对应的表

### 添加新功能

1. **Repository 层**:
   - 如果通用 `Repository<T>` 不够用,在 `Interfaces/` 定义接口,在 `Implements/` 实现
   - 在 [RepositoryModule.cs](3-DataAccess/MyApiWeb.Repository/Modules/RepositoryModule.cs) 注册

2. **Service 层**:
   - 在 `Interfaces/` 定义服务接口(如 `IXxxService`)
   - 在 `Implements/` 实现业务逻辑
   - 在 [ServiceModule.cs](2-Business/MyApiWeb.Services/Modules/ServiceModule.cs) 注册为 `InstancePerLifetimeScope`

3. **Controller 层**:
   - 继承 `ApiControllerBase<TService, TEntity, TKey>` 获得统一的响应方法
   - 通过构造函数注入服务
   - 使用 `Success()`、`Error()` 方法返回统一格式

### 异常处理

- **业务逻辑异常**: 抛出 `DomainException` 或 `BusinessException`,会被自动转换为 200 状态码的错误响应
- **未知异常**: 会被 [GlobalExceptionMiddleware.cs](1-Presentation/MyApiWeb.Api/Middlewares/GlobalExceptionMiddleware.cs) 捕获并记录,返回统一的 500 错误

### 权限控制

- 在 Controller 或 Action 上使用 `[Authorize]` 标记需要认证
- 通过 `[Authorize(Policy = "RequirePermission:xxx")]` 实现细粒度权限控制
- 当前用户信息通过 `ICurrentUser` 服务获取

## 常见问题

### 数据库连接失败

- 检查 SQL Server 是否运行
- 验证 [appsettings.json](1-Presentation/MyApiWeb.Api/appsettings.json) 中的连接字符串
- 确保 SQL Server 已启用 TCP/IP 协议

### JWT Token 验证失败

- 检查 Token 是否已过期(默认 30 分钟)
- 验证 `JwtSettings:Secret` 在前后端是否一致
- 查看响应头中的 `Token-Expired` 标记

### Autofac 注册问题

- 确保在对应的 `Module.cs` 中注册了服务
- 检查生命周期(建议使用 `InstancePerLifetimeScope`)
- 注意 `SqlSugarDbContext` 是 `SingleInstance` 单例

## API 访问

- **本地开发**: `https://localhost:5001` 或 `http://localhost:5000`
- **Swagger UI**: 开发环境下访问根路径 `/` 即可
- **默认管理员**: `admin` / `123456` (首次启动自动创建)

## 前端集成

前端项目位于 `../frontend` 目录,通过以下方式与后端集成:

- **开发环境**: 前端 Vite 开发服务器(端口 5173 或 3000)通过 CORS 访问后端 API
- **生产环境**: 前端构建后通过 Nginx 反向代理到后端 API,或单独部署并配置 CORS 白名单

CORS 配置在 [Program.cs:72-107](1-Presentation/MyApiWeb.Api/Program.cs#L72-L107),开发环境和生产环境使用不同的策略。

## 重要文件速查

- **应用入口**: [Program.cs](1-Presentation/MyApiWeb.Api/Program.cs)
- **数据库上下文**: [SqlSugarDbContext.cs](3-DataAccess/MyApiWeb.Repository/SqlSugarDbContext.cs)
- **全局异常处理**: [GlobalExceptionMiddleware.cs](1-Presentation/MyApiWeb.Api/Middlewares/GlobalExceptionMiddleware.cs)
- **API 基类**: [ApiControllerBase.cs](1-Presentation/MyApiWeb.Api/Controllers/ApiControllerBase.cs)
- **依赖注入配置**:
  - [RepositoryModule.cs](3-DataAccess/MyApiWeb.Repository/Modules/RepositoryModule.cs)
  - [ServiceModule.cs](2-Business/MyApiWeb.Services/Modules/ServiceModule.cs)
- **数据种子**: [DataSeeder.cs](1-Presentation/MyApiWeb.Api/Data/DataSeeder.cs)

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## 项目概述

这是一个基于 .NET 9 和 Vue 3 的现代化前后端分离 Web 应用程序，实现了完整的 RBAC 权限管理、实时通信（SignalR）、消息总线（CAP）、定时任务（Quartz）等企业级功能。

**技术栈概览：**
- **后端**: .NET 9 Web API, SqlSugar ORM, Autofac DI, JWT 认证, SignalR, CAP, Quartz, Serilog
- **前端**: Vue 3, TypeScript, Vite, Pinia, Vue Router, Ant Design Vue, Axios, SignalR Client
- **数据库**: SQL Server

---

## 项目结构

### 后端架构（三层 + 基础设施）

```
backend/
├── backend.sln                           # Visual Studio 解决方案文件
├── 0-Infrastructure/                     # 基础设施层
│   └── MyApiWeb.Infrastructure/          
│       ├── Configuration/                # 服务扩展方法（模块化配置）
│       ├── Data/                         # 数据种子和初始化
│       ├── Middlewares/                  # 全局中间件
│       ├── Jobs/                         # Quartz 定时任务
│       └── Cap/                          # CAP 事务集成
│
├── 1-Presentation/                       # 表示层
│   └── MyApiWeb.Api/
│       ├── Program.cs                    # 应用程序入口点
│       ├── Controllers/                  # API 控制器
│       ├── appsettings.json              # 主配置文件
│       └── appsettings/                  # 模块化配置目录
│           ├── database.json             # 数据库配置
│           ├── jwt.json                  # JWT 认证
│           ├── cors.json                 # CORS 跨域
│           ├── cap.json                  # CAP 消息总线
│           ├── serilog.json              # 日志配置
│           ├── signalr.json              # SignalR 配置
│           └── onlineuser.json           # 在线用户配置
│
├── 2-Business/                           # 业务逻辑层
│   ├── MyApiWeb.Models/                  # 实体模型和 DTO
│   │   ├── Entities/                     # 数据库实体（User, Role, Permission, Menu 等）
│   │   ├── DTOs/                         # 数据传输对象
│   │   └── Interfaces/                   # 业务接口
│   │
│   └── MyApiWeb.Services/                # 业务服务实现
│       ├── Implements/                   # 服务实现类
│       ├── Interfaces/                   # 服务接口
│       ├── Modules/ServiceModule.cs      # Autofac 服务注册模块
│       └── Subscribers/                  # CAP 事件订阅者
│
└── 3-DataAccess/                         # 数据访问层
    └── MyApiWeb.Repository/
        ├── Implements/                   # 仓储实现
        │   ├── SqlSugarDbContext.cs      # SqlSugar 数据库上下文
        │   └── Repository.cs             # 通用仓储实现
        ├── Interfaces/                   # 仓储接口
        └── Modules/RepositoryModule.cs   # Autofac 仓储注册模块
```

### 前端架构

```
frontend/
├── src/
│   ├── api/                              # API 请求封装
│   ├── components/                       # 可复用组件
│   │   └── navigation/                   # 导航相关组件
│   ├── composables/                      # Vue 组合式函数
│   ├── config/                           # 配置文件
│   ├── directives/                       # 自定义指令
│   ├── layouts/                          # 布局组件
│   │   └── MainLayout.vue                # 主应用布局
│   ├── plugins/                          # 插件
│   ├── router/                           # 路由配置
│   │   └── index.ts                      # 路由定义和动态路由注册
│   ├── services/                         # 业务服务
│   ├── stores/                           # Pinia 状态管理
│   │   ├── auth.ts                       # 认证状态
│   │   ├── user.ts                       # 用户信息
│   │   ├── permission.ts                 # 权限管理
│   │   ├── menu.ts                       # 菜单数据
│   │   ├── onlineUser.ts                 # 在线用户（SignalR）
│   │   ├── tabs.ts                       # 标签页管理
│   │   └── theme.ts                      # 主题配置
│   ├── types/                            # TypeScript 类型定义
│   ├── utils/                            # 工具函数
│   ├── views/                            # 页面视图
│   │   ├── admin/                        # 管理页面
│   │   └── profile/                      # 用户中心
│   ├── App.vue                           # 根组件
│   └── main.ts                           # 应用入口
│
├── vite.config.ts                        # Vite 配置
├── package.json                          # 依赖管理
└── tsconfig.json                         # TypeScript 配置
```

---

## 核心架构模式

### 后端依赖注入模式（Autofac）

项目使用 **Autofac** 作为依赖注入容器，通过模块化方式注册服务：

1. **`RepositoryModule`** (`backend/3-DataAccess/MyApiWeb.Repository/Modules/RepositoryModule.cs`)
   - 注册 `SqlSugarDbContext`（单例）
   - 注册泛型 `Repository<T>` 为 `IRepository<T>`（作用域）

2. **`ServiceModule`** (`backend/2-Business/MyApiWeb.Services/Modules/ServiceModule.cs`)
   - 注册所有业务服务实现（UserService, RoleService, PermissionService 等）
   - 注册 CAP 事件订阅者

3. **额外服务注册** (`Program.cs`)
   - 数据种子服务（RbacDataSeeder, MenuDataSeeder）

**扩展新服务的步骤：**
1. 在对应层创建接口和实现类
2. 在相应的 Autofac 模块中注册（`ServiceModule.cs` 或 `RepositoryModule.cs`）
3. 通过构造函数注入使用

### 模块化配置系统

配置文件按**功能模块**拆分，位于 `backend/1-Presentation/MyApiWeb.Api/appsettings/` 目录：
- `database.json` - 数据库连接字符串
- `jwt.json` - JWT 认证配置
- `cors.json` - CORS 跨域策略
- `cap.json` - CAP 消息总线配置
- `serilog.json` - 日志配置
- `signalr.json` - SignalR 配置
- `onlineuser.json` - 在线用户管理配置

**配置加载逻辑**（`Program.cs`）：
```csharp
// 1. 加载基础配置: appsettings/{module}.json
// 2. 加载环境配置: appsettings/{module}.{Environment}.json（会覆盖基础配置）
```

**配置扩展方法**位于 `backend/0-Infrastructure/MyApiWeb.Infrastructure/Configuration/`，每个功能都有独立的扩展方法（如 `AddJwtAuthentication`, `AddCapMessageBus` 等）。

### 前端动态路由与权限控制

**路由生成机制** (`frontend/src/router/index.ts`):
1. **静态路由**: 登录、注册、404、403 等公共页面在 `constantRoutes` 中定义
2. **动态路由**: 基于后端返回的**菜单数据**（`MenuDto[]`）动态生成并注册到 `MainLayout` 的 `children` 中
3. **视图组件映射**: 通过 `import.meta.glob('@/views/**/*.vue')` 自动扫描所有视图，根据菜单的 `routeName` 匹配对应组件

**权限控制流程** (`router.beforeEach`):
1. 检查用户登录状态
2. 已登录用户：
   - 加载用户权限（`permissionStore.loadUserPermissions()`）
   - 加载菜单数据（`menuStore.ensureLoaded()`）
   - 注册动态路由（`registerDynamicMenuRoutes()`）
   - 检查页面权限（`permissionStore.hasPermission()`）
3. 未登录用户：重定向到登录页

**状态持久化**: 使用 `pinia-plugin-persistedstate` 持久化 `authStore` 和 `userStore`，避免刷新页面后丢失登录状态。

---

## 常用开发命令

### 后端

```bash
# 进入后端 API 项目目录
cd backend/1-Presentation/MyApiWeb.Api

# 恢复 NuGet 包
dotnet restore

# 编译项目
dotnet build

# 运行开发服务器（默认 http://localhost:5000）
dotnet run

# 发布生产版本
dotnet publish -c Release -o ./publish

# 在解决方案根目录编译整个解决方案
cd backend
dotnet build backend.sln
```

**数据库相关**:
- 项目使用 **SqlSugar CodeFirst** 模式，首次运行会自动创建数据库表
- 数据种子会在应用启动时自动执行（`DataSeeder.Seed(app)` in `Program.cs`）
- 数据库连接字符串位于 `appsettings/database.json`

### 前端

```bash
# 进入前端项目目录
cd frontend

# 安装依赖
npm install

# 启动开发服务器（http://localhost:3000）
npm run dev

# 类型检查
npm run type-check

# 构建生产版本
npm run build

# 预览生产构建
npm run preview

# 代码检查和自动修复
npm run lint

# 代码格式化
npm run format

# 运行单元测试
npm run test:unit
```

---

## 关键技术实现

### JWT 双 Token 认证机制

**实现位置**: `backend/2-Business/MyApiWeb.Services/Implements/TokenService.cs`

- **Access Token**: 短生命周期（默认 1 小时），用于 API 请求认证
- **Refresh Token**: 长生命周期（默认 7 天），存储在数据库 `RefreshToken` 表中，用于刷新 Access Token
- **刷新流程**: 前端检测到 Access Token 过期（401 响应）后，使用 Refresh Token 调用 `/api/token/refresh` 获取新的 Access Token

### SignalR 实时通信

**后端配置**:
- Hub 实现: `backend/1-Presentation/MyApiWeb.Api/Controllers/ChatHub.cs`
- 端点映射: `app.MapHub<ChatHub>("/hubs/chat")` in `Program.cs`
- JWT 认证支持: `AddSignalRWithJwtSupport()` 扩展方法

**前端集成**:
- SignalR 客户端: `@microsoft/signalr`
- 连接管理: `frontend/src/stores/onlineUser.ts` 中的 `useOnlineUserStore`
- 在线用户实时统计: 连接/断开事件触发数据刷新

### CAP 消息总线与 SqlSugar 事务集成

**配置位置**:
- `backend/1-Presentation/MyApiWeb.Api/appsettings/cap.json`
- `backend/0-Infrastructure/MyApiWeb.Infrastructure/Configuration/CapServiceExtensions.cs`

**事务集成**:
- 自定义事务实现: `backend/0-Infrastructure/MyApiWeb.Infrastructure/Cap/SqlSugarCapTransaction.cs`
- 使用方式: 在服务中通过 `ICapPublisher` 发布事件，在 `Subscribers/` 目录创建订阅者

**详细文档**: `backend/doc/CAP_SqlSugar_Integration_Guide.md`

### Quartz.NET 定时任务

**配置位置**: `backend/0-Infrastructure/MyApiWeb.Infrastructure/Configuration/QuartzServiceExtensions.cs`

**现有任务**:
- `OnlineUserCleanupJob`: 定期清理超时的在线用户记录（默认每 5 分钟执行一次）

**添加新任务**:
1. 在 `backend/0-Infrastructure/MyApiWeb.Infrastructure/Jobs/` 创建实现 `IJob` 的类
2. 在 `QuartzServiceExtensions.cs` 的 `AddQuartzWithJobs()` 方法中注册任务

### Serilog 结构化日志

**配置文件**: `backend/1-Presentation/MyApiWeb.Api/appsettings/serilog.json`

**日志输出**:
- 控制台: 彩色格式化输出
- 文件: `logs/log-{Date}.txt`（按天滚动）

**使用方式**:
```csharp
Log.Information("用户 {UserId} 执行了 {Action}", userId, action);
Log.Error(ex, "处理请求时发生错误");
```

### 全局异常处理

**中间件**: `backend/0-Infrastructure/MyApiWeb.Infrastructure/Middlewares/GlobalExceptionMiddleware.cs`

- 捕获所有未处理的异常
- 返回统一的错误响应格式（`ApiResult<T>`）
- 记录详细的错误日志

---

## 开发规范与最佳实践

### 后端开发规范

1. **控制器**:
   - 所有 API 控制器继承自 `ApiControllerBase`
   - 使用 `[ApiController]` 和 `[Route("api/[controller]")]` 特性
   - 返回类型统一使用 `ApiResult<T>` 包装（通过 `ApiResultHelper` 创建）

2. **服务层**:
   - 业务逻辑必须在 `Services` 层实现，控制器仅负责参数验证和结果转换
   - 服务接口定义在 `MyApiWeb.Services/Interfaces/`
   - 服务实现定义在 `MyApiWeb.Services/Implements/`

3. **数据访问**:
   - 优先使用泛型 `IRepository<T>` 进行 CRUD 操作
   - 复杂查询可以在服务层直接注入 `SqlSugarDbContext` 使用原生 SqlSugar API

4. **实体定义**:
   - 数据库实体继承自 `EntityBase`（提供 `Id`, `CreatedAt`, `UpdatedAt` 等基础字段）
   - 使用 SqlSugar 特性（`[SugarColumn]`, `[SugarTable]`）配置表映射

5. **依赖注入**:
   - 新增服务必须在对应的 Autofac 模块中注册
   - 使用构造函数注入，避免使用 `ServiceLocator` 模式

### 前端开发规范

1. **组件命名**:
   - 组件文件使用 PascalCase（如 `UserProfile.vue`）
   - 组件名称与文件名保持一致

2. **Store 使用**:
   - Store 命名遵循 `use[Name]Store` 模式（如 `useAuthStore`）
   - 需要持久化的 Store 使用 `persist: true` 选项

3. **API 调用**:
   - 所有 API 请求封装在 `src/api/` 目录
   - 使用统一的 axios 实例（自动处理 token、错误、响应拦截）

4. **类型定义**:
   - 接口类型定义在 `src/types/api.ts`
   - 组件 props 和 emit 必须定义类型
   - 避免使用 `any`，必要时使用 `unknown`

5. **路由和权限**:
   - 新增页面视图放在 `src/views/` 目录
   - 文件名的 **小写形式**（忽略 `-`, `_`, `/`）必须与后端菜单的 `routeName` 匹配
   - 需要权限控制的页面在 `meta` 中添加 `permission` 字段

6. **样式规范**:
   - 优先使用 Ant Design Vue 组件
   - 组件使用 `<style scoped>` 避免样式污染
   - 主题色和间距使用 CSS 变量

---

## 测试

### 后端测试
- 项目当前**没有**单元测试项目（未来可在 `backend/tests/` 创建）
- 推荐测试框架: xUnit + Moq + FluentAssertions

### 前端测试
- 测试框架: Vitest + Vue Test Utils
- 测试文件位于 `frontend/src/__tests__/` 或组件旁的 `.spec.ts` 文件
- 运行测试: `npm run test:unit`

---

## 常见开发任务

### 添加新的 API 端点
1. 在 `MyApiWeb.Models/DTOs/` 创建请求/响应 DTO
2. 在 `MyApiWeb.Services/Interfaces/` 创建服务接口
3. 在 `MyApiWeb.Services/Implements/` 实现服务逻辑
4. 在 `ServiceModule.cs` 注册服务
5. 在 `MyApiWeb.Api/Controllers/` 创建或更新控制器

### 添加新的数据库实体
1. 在 `MyApiWeb.Models/Entities/` 创建实体类（继承 `EntityBase`）
2. 使用 SqlSugar 特性配置表映射和列属性
3. 重启应用，SqlSugar 会自动创建表（CodeFirst 模式）
4. 如需初始数据，在 `MyApiWeb.Infrastructure/Data/SeedConfigurations/` 创建种子配置

### 添加新的前端页面
1. 在 `src/views/` 创建 `.vue` 文件（如 `MyPage.vue`）
2. 在后端添加对应的菜单记录（`Menu` 表），设置 `routeName` 为 `MyPage`（匹配文件名）
3. 设置菜单的 `routePath`（如 `/my-page`）和 `permissionCode`（如有权限要求）
4. 重启前端应用，路由会自动注册

### 修改模块化配置
1. 找到对应的配置文件（如 `appsettings/jwt.json`）
2. 修改配置值
3. 如需环境特定配置，创建 `{module}.{Environment}.json`（如 `jwt.Development.json`）
4. 重启应用生效（配置支持 `reloadOnChange: true`）

---

## 项目文档

详细文档位于以下目录：
- `backend/doc/` - 后端技术文档
  - `configuration/` - 配置系统详细说明
  - `CAP_SqlSugar_Integration_Guide.md` - CAP 消息总线集成指南
- `frontend/doc/` - 前端开发文档（基础 README）
- `README.md` - 项目快速开始指南
- `AGENTS.md` - 代码库和 AI 协作指南

---

## 环境要求

- **.NET 9 SDK**
- **Node.js 20.19+ 或 22.12+**
- **SQL Server** (本地或远程实例)
- **推荐 IDE**:
  - 后端: Visual Studio 2022 / JetBrains Rider / VS Code (C# Dev Kit)
  - 前端: VS Code (Volar 扩展)

---

## 注意事项

1. **首次运行**:
   - 确保 SQL Server 运行正常
   - 修改 `appsettings/database.json` 中的连接字符串
   - 后端首次启动会自动创建数据库和初始数据

2. **开发环境**:
   - 后端默认端口: `5000`（可在 `Properties/launchSettings.json` 修改）
   - 前端默认端口: `3000`（可在 `vite.config.ts` 修改）
   - 前端通过 Vite 代理将 `/api` 请求转发到后端

3. **日志查看**:
   - 后端日志: `backend/1-Presentation/MyApiWeb.Api/logs/`
   - 结构化日志便于排查问题

4. **数据库迁移**:
   - 项目使用 SqlSugar CodeFirst，修改实体后重启应用会自动同步表结构
   - 生产环境建议关闭自动同步，使用手动迁移脚本

5. **安全配置**:
   - `appsettings/jwt.json` 中的 `SecretKey` 必须在生产环境更换为强密钥
   - 敏感配置不要提交到 Git（使用 User Secrets 或环境变量）

---

这份文档提供了项目的核心架构和开发指南。如需更详细的文档，请参阅 `backend/doc/` 和 `frontend/doc/` 目录。

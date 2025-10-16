# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**My API Web** is a modern full-stack web application built with .NET 9 backend and Vue 3 frontend, featuring a comprehensive RBAC (Role-Based Access Control) system with real-time capabilities via SignalR.

---

## Development Commands

### Backend (.NET 9)

```bash
# Navigate to API project
cd backend/1-Presentation/MyApiWeb.Api

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application (starts on http://localhost:5000)
dotnet run

# Run tests
dotnet test

# Publish for production
dotnet publish -c Release -o ./publish
```

### Frontend (Vue 3 + TypeScript)

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Start development server (http://localhost:3000)
npm run dev

# Type checking only
npm run type-check

# Build for production
npm run build

# Preview production build
npm run preview

# Full build (type-check + build)
npm run build  # Already includes type-check via run-p
```

---

## Architecture Overview

### Backend Architecture

#### Four-Layer Architecture

```
backend/
├── 0-Infrastructure/           # Infrastructure layer
│   └── MyApiWeb.Infrastructure/
│       ├── Configuration/      # Service registrations, middleware setup
│       ├── Data/              # Database initialization, seed data
│       │   └── SeedConfigurations/  # Menu, RBAC seed configurations
│       └── Jobs/              # Quartz.NET scheduled jobs
├── 1-Presentation/            # Presentation layer
│   └── MyApiWeb.Api/
│       ├── Controllers/       # API controllers
│       ├── Hubs/             # SignalR hubs
│       └── appsettings/      # Modular configuration files
├── 2-Business/               # Business logic layer
│   ├── MyApiWeb.Models/
│   │   └── Entities/         # Entity models (by module)
│   │       ├── Common/       # Base entities, shared models
│   │       ├── System/       # User, Role, Permission, Menu
│   │       ├── Auth/         # Token, RefreshToken
│   │       └── Hub/          # OnlineUser
│   └── MyApiWeb.Services/
│       ├── Implements/       # Service implementations (by module)
│       │   ├── Common/, System/, Auth/, Hub/
│       └── Interfaces/       # Service interfaces (by module)
│           ├── Common/, System/, Auth/, Hub/
└── 3-DataAccess/             # Data access layer
    └── MyApiWeb.Repository/
        └── BaseRepository.cs # Generic SqlSugar repository
```

#### Database Naming Conventions

**CRITICAL**: This project follows strict database naming conventions:

- **Table Prefixes**:
  - `Sys_` for system tables (Users, Roles, Permissions, Menus)
  - `Auth_` for authentication tables (Tokens, RefreshTokens)
  - `Hub_` for real-time communication tables (OnlineUsers)

- **Column Prefix**:
  - ALL columns use `F_` prefix (e.g., `F_Id`, `F_Username`, `F_Email`)

- **Entity Base Class**:
  - All entities inherit from `EntityBase` (in `Models/Entities/Common/`)
  - Provides standard fields: `F_Id`, `F_CreationTime`, `F_CreatorId`, `F_LastModificationTime`, `F_LastModifierId`
  - Includes 10 extension fields: `F_Extend1-5` (string), `F_ExtendInt1-3` (int), `F_ExtendDate1-2` (DateTime)

Example entity:
```csharp
[SugarTable("Sys_Users")]
public class User : EntityBase
{
    [SugarColumn(ColumnName = "F_Username", Length = 50, IsNullable = false)]
    public string Username { get; set; }
}
```

#### Module Organization

Business logic is organized by functional modules:

- **Common**: Shared utilities, base classes, email service
- **System**: User management, role management, permission management, menu management
- **Auth**: Token management, authentication
- **Hub**: Online user management, SignalR real-time features

Service namespaces follow pattern:
- `MyApiWeb.Services.Interfaces.{Module}.I{Service}Service`
- `MyApiWeb.Services.Implements.{Module}.{Service}Service`

#### Dependency Injection

- Uses **Autofac** as the DI container
- Service registration is configured in `Infrastructure/Configuration/AutofacConfiguration.cs`
- Scoped lifetime for services, transient for repositories

#### Configuration

Uses modular configuration files in `appsettings/`:
- `database.json` - SqlSugar database configuration
- `jwt.json` - JWT authentication settings
- `cors.json` - CORS policy configuration
- `cap.json` - CAP distributed transaction bus
- `serilog.json` - Structured logging configuration
- `onlineuser.json` - Online user cleanup settings

Environment-specific overrides: `{module}.{Environment}.json`

---

### Frontend Architecture

#### Modular Organization

**CRITICAL**: Frontend follows a strict module-based organization pattern aligned with backend modules:

```
frontend/src/
├── api/modules/              # API clients (by module)
│   ├── common/
│   ├── system/              # users.ts, roles.ts, permissions.ts, menus.ts, device.ts
│   ├── auth/                # auth.ts, token.ts
│   ├── hub/                 # onlineUsers.ts
│   └── index.ts             # Unified exports
├── stores/modules/          # Pinia stores (by module)
│   ├── common/
│   ├── system/              # user.ts, permission.ts, menu.ts
│   ├── auth/                # auth.ts
│   ├── hub/                 # onlineUser.ts
│   └── index.ts             # Unified exports
├── types/modules/           # TypeScript types (by module)
│   ├── common/              # ApiResponse, ApiError
│   ├── system/              # user.ts, role.ts, permission.ts, menu.ts
│   ├── auth/                # token.ts, auth.ts
│   ├── hub/                 # onlineUser.ts
│   └── index.ts             # Unified exports
└── views/                   # Vue components (by module)
    ├── auth/                # Login.vue, Register.vue
    ├── system/              # UserManagement.vue, RoleManagement.vue, MenuManagement.vue
    ├── hub/                 # OnlineUserManagement.vue
    ├── dashboard/           # Home.vue, SystemMonitor.vue
    └── common/              # UserProfile.vue, NotFoundView.vue, ForbiddenView.vue
```

#### Import Patterns

**Recommended**: Use modular imports for new code:
```typescript
import { UserDto } from '@/types/modules/system/user'
import { UsersApi } from '@/api/modules/system/users'
import { useUserStore } from '@/stores/modules/system/user'
```

**Backward Compatible**: Legacy flat imports still work:
```typescript
import { UserDto } from '@/types/api'
import { UsersApi } from '@/api'
```

#### Dynamic Routing System

The application uses a **backend-driven menu system**:

1. **Menu Configuration** (Backend):
   - Menus are seeded in `MenuSeedConfig.cs`
   - Each menu has: `RoutePath`, `RouteName`, `PermissionCode`
   - Menu structure matches frontend view locations

2. **Route Resolution** (Frontend):
   - Router dynamically generates routes from backend menu data
   - Uses `import.meta.glob('@/views/**/*.vue')` for component resolution
   - Component resolution is case-insensitive and normalizes hyphens/underscores

3. **Route Naming Convention**:
   - Menu `RouteName` must match Vue file name (e.g., `UserManagement` → `UserManagement.vue`)
   - Menu `RoutePath` follows module pattern (e.g., `/system/users`, `/hub/online-users`)

**CRITICAL**: When adding new pages:
1. Create Vue component in appropriate module folder (e.g., `views/system/NewPage.vue`)
2. Update `MenuSeedConfig.cs` with new menu entry
3. Ensure `RouteName` matches component filename
4. Ensure `RoutePath` matches module structure (`/module-name/page-name`)

#### State Management

Uses **Pinia** with these key stores:

- **authStore** (`stores/modules/auth/auth.ts`):
  - Manages authentication state, tokens, current user
  - Persisted to localStorage
  - Handles login/logout/token refresh

- **permissionStore** (`stores/modules/system/permission.ts`):
  - Manages user permissions
  - Provides permission checking functions
  - Persisted to localStorage

- **menuStore** (`stores/modules/system/menu.ts`):
  - Fetches and caches menu structure from backend
  - Generates dynamic routes

- **userStore** (`stores/modules/system/user.ts`):
  - Manages user list, CRUD operations
  - Not persisted

- **onlineUserStore** (`stores/modules/hub/onlineUser.ts`):
  - Manages online users list and statistics
  - Integrates with SignalR for real-time updates

**Note**: To avoid circular dependencies, `onlineUserStore` uses dynamic import for `authStore`:
```typescript
const { useAuthStore } = await import('@/stores/modules/auth/auth')
```

#### Permission System

Permissions are checked via directive:
```vue
<a-button v-permission="'system:user:create'">Create User</a-button>
```

Permission codes follow pattern: `{module}:{entity}:{action}`
- Examples: `system:user:view`, `system:role:create`, `system:menu:delete`

#### SignalR Integration

Real-time features use SignalR connection (`/hubs/chat`):
- Connection lifecycle managed in `stores/modules/hub/onlineUser.ts`
- Automatic reconnection on auth state changes
- Heartbeat mechanism for connection health

---

## Key Patterns and Practices

### Backend Patterns

1. **Service Pattern**:
   - All business logic in service classes
   - Controllers are thin, delegate to services
   - Services use repository pattern for data access

2. **Repository Pattern**:
   - Generic `BaseRepository<T>` for CRUD operations
   - Uses SqlSugar ORM with async/await

3. **DTO Pattern**:
   - Separate DTOs for Create, Update, Response operations
   - AutoMapper not used - manual mapping in services

4. **Background Jobs**:
   - Quartz.NET for scheduled tasks
   - Example: `OnlineUserCleanupJob` removes stale connections

### Frontend Patterns

1. **Composition API**:
   - All Vue 3 components use `<script setup>` syntax
   - TypeScript with explicit type annotations

2. **Composables**:
   - Reusable logic extracted to `composables/`
   - Example: `useSystemMonitor.ts` for real-time system metrics

3. **API Layer**:
   - Class-based API clients extending `BaseApi`
   - Centralized error handling in Axios interceptors

4. **Type Safety**:
   - All API responses typed with DTOs
   - Strict TypeScript configuration

---

## 数据库架构规范 ⭐

**重要变更（2025-01-16）**：项目已实施数据库命名规范优化

### 表名规范

所有数据库表按**模块**分类，使用统一前缀：

| 模块前缀 | 说明 | 示例表名 |
|---------|------|---------|
| `Sys_` | 系统模块（用户、角色、权限、菜单等） | `Sys_Users`, `Sys_Roles`, `Sys_Permissions` |
| `Auth_` | 认证模块（令牌管理等） | `Auth_RefreshTokens` |
| `Hub_` | 实时通信模块（SignalR 相关） | `Hub_OnlineUsers` |

### 字段名规范

所有数据库列名统一使用 **`F_` 前缀**，例如：
- `F_Id` - 主键
- `F_Username` - 用户名
- `F_Email` - 邮箱
- `F_CreationTime` - 创建时间

**C# 属性名保持不变**（使用 SqlSugar ColumnName 映射）：
```csharp
// C# 代码中仍使用友好的属性名
var user = new User {
    Username = "admin",    // C# 属性名
    Email = "admin@example.com"
};
// 自动映射到数据库列：F_Username, F_Email
```

### 扩展字段

所有继承 `EntityBase` 的实体都包含 **10 个扩展字段**：

| 字段类型 | 字段名 | 数据库列名 | 用途 |
|---------|--------|-----------|------|
| 字符串 (500) | `Extend1` ~ `Extend5` | `F_Extend1` ~ `F_Extend5` | 存储文本、JSON、配置 |
| 整数 | `ExtendInt1` ~ `ExtendInt3` | `F_ExtendInt1` ~ `F_ExtendInt3` | 存储数值、状态码 |
| 日期时间 | `ExtendDate1` ~ `ExtendDate2` | `F_ExtendDate1` ~ `F_ExtendDate2` | 存储时间戳 |

**使用示例**：
```csharp
// 存储用户配置
user.Extend1 = JsonSerializer.Serialize(new { Theme = "dark", Language = "zh-CN" });

// 记录业务状态
role.ExtendInt1 = 1; // 1=待审核, 2=已批准
role.ExtendDate1 = DateTimeOffset.Now; // 状态变更时间
```

**详细文档**：`backend/doc/DATABASE_MIGRATION_GUIDE.md`

---

## Database and Migrations

This project uses **SqlSugar CodeFirst** approach:（使用新的表名规范）

- Entity changes automatically create/update tables on startup
- Seed data runs on application start (idempotent)
- No separate migration files

To modify database:
1. Update entity class in `Models/Entities/{Module}/`
2. Follow naming conventions (`F_` prefix, module table prefix)
3. Restart application - SqlSugar handles schema updates

---

## 相关文档

- **数据库迁移指南**: `backend/doc/DATABASE_MIGRATION_GUIDE.md` ⭐
- **配置文档中心**: `backend/doc/configuration/INDEX.md`
- **CAP 集成指南**: `backend/doc/CAP_SqlSugar_Integration_Guide.md`
- **项目快速开始**: `README.md`
- **AI 协作指南**: `AGENTS.md`

这份文档提供了项目的核心架构和开发指南。如需更详细的文档，请参阅上述相关文档链接。
## Testing Strategy

### Backend Testing
- Unit tests for services
- Integration tests for repositories
- Run: `dotnet test` from solution root

### Frontend Testing
- Type checking: `npm run type-check`
- Build validation serves as integration test
- Run: `npm run build`

---

## Authentication Flow

1. User submits credentials to `/api/auth/login`
2. Backend validates, returns `accessToken` + `refreshToken`
3. Frontend stores tokens in localStorage via `authStore`
4. Axios interceptor adds `Authorization: Bearer {token}` to requests
5. On 401 error, automatic token refresh via `/api/token/refresh`
6. On refresh failure, redirect to login

JWT token includes claims: `userId`, `username`, `permissions`, `roles`

---

## Common Issues and Solutions

### Backend Issues

1. **Service not found in DI container**:
   - Check service is registered in `AutofacConfiguration.cs`
   - Verify namespace matches module structure

2. **Database connection fails**:
   - Verify SQL Server is running
   - Check connection string in `appsettings/database.json`
   - Ensure `TrustServerCertificate=true` for local SQL Server

3. **Menu routes not working**:
   - Verify `RoutePath` and `RouteName` in `MenuSeedConfig.cs`
   - Ensure frontend view exists at expected location
   - Check menu is enabled (`IsEnabled = true`)

### Frontend Issues

1. **Type errors after adding new types**:
   - Run `npm run type-check` to identify issues
   - Ensure types are exported from module `index.ts`
   - Check imports use correct module path

2. **Dynamic routes not loading**:
   - Check menu data is fetched successfully
   - Verify component name matches `RouteName` (case-insensitive)
   - Inspect browser console for resolution errors

3. **SignalR connection fails**:
   - Verify backend hub is running at `/hubs/chat`
   - Check JWT token is valid
   - Inspect Network tab for WebSocket connection

4. **Circular dependency warnings**:
   - Use dynamic imports for stores that depend on each other
   - Example: `const { useAuthStore } = await import('@/stores/modules/auth/auth')`

---

## Code Style Guidelines

### Backend (C#)
- Use PascalCase for classes, methods, properties
- Use camelCase for local variables
- Async methods must have `Async` suffix
- Services must have interface + implementation
- Controllers return `IActionResult` with appropriate status codes

### Frontend (TypeScript/Vue)
- Use PascalCase for component names
- Use camelCase for variables, functions
- Use kebab-case for file names (except Vue components)
- Explicit return types for functions
- Props and emits must be typed

---

## Git Workflow

Current branch: `main`
Main branch for PRs: `main`

Recent development history shows focus on:
- Backend/frontend modularization by business domain
- Online user management with SignalR
- System monitoring dashboard
- RBAC seed data refactoring

---

## Project-Specific Notes

1. **Menu Seed Data is Critical**:
   - Any frontend route changes MUST be reflected in `MenuSeedConfig.cs`
   - Menu `RoutePath` determines navigation
   - Menu `RouteName` determines which Vue component loads
   - Menu `PermissionCode` controls access

2. **Module Alignment**:
   - Backend modules (Common, System, Auth, Hub) must align with frontend modules
   - When adding a new module, update all layers: Entities, Services, API, Stores, Types, Views

3. **Extension Fields**:
   - All entities have 10 extension fields for future flexibility
   - Use these for quick additions without schema changes
   - Document usage in entity comments

4. **SqlSugar CodeFirst**:
   - Schema updates happen automatically on startup
   - Be careful with column renames (may cause data loss)
   - Test schema changes in development first

# 模块化配置文件说明

## 📁 配置文件结构

```
appsettings/
├── README.md              # 本文档
├── database.json          # 数据库连接和迁移配置
├── jwt.json              # JWT 身份认证配置
├── cors.json             # CORS 跨域配置
├── cap.json              # CAP 消息总线配置
└── serilog.json          # Serilog 日志配置
```

## 🎯 设计目标

将原本集中在 `appsettings.json` 中的所有配置按功能模块拆分，带来以下好处：

1. **易于维护** - 每个模块的配置独立管理，修改时不会影响其他模块
2. **职责清晰** - 配置文件命名直观，快速定位需要修改的配置
3. **团队协作** - 不同团队成员可以独立修改各自负责模块的配置，减少冲突
4. **环境隔离** - 方便为不同环境创建特定配置文件

## 📋 配置文件说明

### 1. database.json - 数据库配置
包含数据库连接字符串和 SqlSugar Code First 相关配置：
- `ConnectionStrings` - 数据库连接字符串
- `DatabaseSettings` - 迁移、种子数据等管理配置

**适用场景：** 不同环境使用不同的数据库连接

### 2. jwt.json - JWT 认证配置
包含 JWT Token 生成和验证的相关配置：
- `JwtSettings.Secret` - JWT 签名密钥（生产环境必须使用环境变量）
- `JwtSettings.Issuer/Audience` - 签发者和受众
- Token 过期时间配置

**适用场景：** 开发环境和生产环境使用不同的密钥和过期时间

### 3. cors.json - CORS 配置
包含跨域资源共享的配置：
- `CorsSettings.AllowedOrigins` - 允许的前端域名白名单

**适用场景：** 不同环境的前端域名不同

### 4. cap.json - CAP 消息总线配置
包含分布式事务和消息总线的配置：
- 存储类型（InMemory, SqlServer, MySql 等）
- 传输类型（InMemory, RabbitMQ, Kafka 等）
- 重试策略和连接配置

**适用场景：** 开发环境使用 InMemory，生产环境使用 RabbitMQ/Kafka

### 5. serilog.json - 日志配置
包含 Serilog 结构化日志的配置：
- 日志级别配置
- 日志输出目标（控制台、文件、远程服务等）
- 日志增强器配置

**适用场景：** 不同环境使用不同的日志级别和输出目标

## 🌍 环境特定配置

### 创建环境配置文件

为不同环境创建特定配置文件，命名规则：`{模块}.{环境}.json`

```
appsettings/
├── database.json                    # 默认配置
├── database.Development.json        # 开发环境覆盖
├── database.Production.json         # 生产环境覆盖
├── jwt.json
├── jwt.Production.json              # 生产环境 JWT 配置
└── ...
```

### 示例：创建生产环境数据库配置

创建 `database.Production.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=MyApiWebDb;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true;Encrypt=true;"
  },
  "DatabaseSettings": {
    "EnableMigrations": false,
    "EnableDataSeeding": false,
    "ForceReseedOnStartup": false
  }
}
```

### 加载环境配置

在 `Program.cs` 中添加环境特定配置加载：

```csharp
var environment = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile("appsettings/database.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings/database.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings/jwt.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings/jwt.{environment}.json", optional: true, reloadOnChange: true)
    // ... 其他配置文件
```

## 🔒 安全最佳实践

### 1. 敏感信息管理

**❌ 不要做：**
```json
{
  "JwtSettings": {
    "Secret": "my-secret-key-123456"  // 硬编码在配置文件中
  }
}
```

**✅ 应该做：**

#### 方式 1: 使用环境变量
```json
{
  "JwtSettings": {
    "Secret": "${JWT_SECRET}"  // 引用环境变量
  }
}
```

#### 方式 2: 使用 User Secrets (开发环境)
```bash
dotnet user-secrets set "JwtSettings:Secret" "your-development-secret"
```

#### 方式 3: 使用 Azure Key Vault (生产环境)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### 2. 配置文件权限

- ✅ 将 `*.Production.json` 添加到 `.gitignore`
- ✅ 生产环境配置通过 CI/CD 管道注入
- ✅ 使用配置加密工具保护敏感配置

## 📊 配置优先级

配置的加载顺序（后加载的会覆盖先加载的）：

```
1. appsettings.json (主配置文件，基础配置)
2. appsettings/database.json (模块配置)
3. appsettings/database.{Environment}.json (环境特定配置)
4. User Secrets (开发环境)
5. 环境变量
6. 命令行参数
```

### 示例：配置覆盖

假设有以下配置：

**appsettings/database.json:**
```json
{
  "DatabaseSettings": {
    "EnableMigrations": true,
    "EnableDataSeeding": true
  }
}
```

**appsettings/database.Production.json:**
```json
{
  "DatabaseSettings": {
    "EnableMigrations": false  // 仅覆盖此项
  }
}
```

**环境变量:**
```
DatabaseSettings__EnableDataSeeding=false
```

**最终生产环境的配置结果：**
```json
{
  "DatabaseSettings": {
    "EnableMigrations": false,      // 来自 database.Production.json
    "EnableDataSeeding": false      // 来自环境变量
  }
}
```

## 🔧 常见使用场景

### 场景 1: 本地开发连接测试数据库

修改 `appsettings/database.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApiWebDb_Test;..."
  }
}
```

### 场景 2: 启用详细日志调试问题

修改 `appsettings/serilog.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"  // 改为 Debug 级别
    }
  }
}
```

### 场景 3: 切换到 RabbitMQ 消息队列

修改 `appsettings/cap.json`:
```json
{
  "CAP": {
    "StorageType": "SqlServer",
    "TransportType": "RabbitMQ",
    "Transport": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest"
    }
  }
}
```

## 📝 维护建议

1. **定期审查** - 每个版本发布前审查所有配置文件，确保配置正确
2. **文档同步** - 配置变更时同步更新本文档
3. **版本控制** - 除敏感配置外，所有配置文件都应纳入版本控制
4. **配置验证** - 使用配置类和数据注解验证配置的有效性
5. **监控告警** - 生产环境配置变更应有审计日志和告警

## 🚀 快速开始

### 1. 修改数据库连接
编辑 `appsettings/database.json`，修改连接字符串

### 2. 配置 CORS 域名
编辑 `appsettings/cors.json`，添加允许的前端域名

### 3. 设置 JWT 密钥
开发环境：
```bash
dotnet user-secrets set "JwtSettings:Secret" "your-dev-secret-key-at-least-32-chars"
```

生产环境：设置环境变量 `JwtSettings__Secret`

### 4. 验证配置
运行应用并检查启动日志：
```
[Information] 已加载模块化配置文件
[Information] 启动应用程序
```

## 📞 问题反馈

如有配置相关问题，请联系团队或提交 Issue。
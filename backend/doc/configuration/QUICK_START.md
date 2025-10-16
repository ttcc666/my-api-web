# 快速开始指南 - 模块化配置

## 🎯 5 分钟快速配置

本指南帮助你快速理解和使用模块化配置结构。

## 📁 配置文件位置

```
MyApiWeb.Api/
├── appsettings.json                      # 主配置（基础设置）
└── appsettings/                          # 模块化配置目录
    ├── database.json                     # 数据库配置
    ├── jwt.json                          # JWT 认证配置
    ├── cors.json                         # CORS 跨域配置
    ├── cap.json                          # CAP 消息总线配置
    └── serilog.json                      # Serilog 日志配置
```

## 🚀 开始使用

### 步骤 1: 配置数据库连接 (必需)

编辑 `appsettings/database.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApiWebDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**常见配置：**

```json
// Windows 身份验证（开发环境）
"DefaultConnection": "Server=localhost;Database=MyApiWebDb;Trusted_Connection=true;TrustServerCertificate=true;"

// SQL Server 身份验证
"DefaultConnection": "Server=localhost;Database=MyApiWebDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"

// Azure SQL Database
"DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=MyApiWebDb;User Id=your-user@your-server;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;"
```

### 步骤 2: 配置 JWT 密钥 (必需)

⚠️ **开发环境推荐使用 User Secrets：**

```bash
# 在项目目录运行
cd 1-Presentation/MyApiWeb.Api
dotnet user-secrets set "JwtSettings:Secret" "your-development-secret-key-must-be-at-least-32-characters-long"
```

**或者**直接编辑 `appsettings/jwt.json`（不推荐用于生产）：

```json
{
  "JwtSettings": {
    "Secret": "A_VERY_LONG_AND_SECURE_SECRET_KEY_REPLACE_IT_LATER_!@#$%"
  }
}
```

### 步骤 3: 配置 CORS (推荐)

如果你的前端运行在不同的端口或域名，编辑 `appsettings/cors.json`：

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:3000",        // React 开发服务器
      "http://localhost:5173",        // Vite 开发服务器
      "http://localhost:4200"         // Angular 开发服务器
    ]
  }
}
```

### 步骤 4: 运行应用

```bash
dotnet run --project 1-Presentation/MyApiWeb.Api/MyApiWeb.Api.csproj
```

检查启动日志，确认配置已加载：

```
[Information] 当前环境: Development
[Information] 已加载模块配置: database
[Information] 已加载模块配置: jwt
[Information] 已加载模块配置: cors
[Information] 已加载模块配置: cap
[Information] 已加载模块配置: serilog
[Information] 所有模块化配置文件加载完成
```

## 🔧 常见配置场景

### 场景 1: 启用数据库迁移和种子数据

编辑 `appsettings/database.json`：

```json
{
  "DatabaseSettings": {
    "EnableMigrations": true,
    "EnableDataSeeding": true
  }
}
```

### 场景 2: 修改 JWT Token 有效期

编辑 `appsettings/jwt.json`：

```json
{
  "JwtSettings": {
    "AccessTokenExpirationMinutes": 60,    // 1 小时
    "RefreshTokenExpirationDays": 30       // 30 天
  }
}
```

### 场景 3: 切换到 RabbitMQ 消息队列

编辑 `appsettings/cap.json`：

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

### 场景 4: 调整日志级别

编辑 `appsettings/serilog.json`：

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"    // Trace, Debug, Information, Warning, Error, Critical
    }
  }
}
```

## 🌍 环境特定配置

### 创建开发环境专用配置

创建 `appsettings/database.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApiWebDb_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "EnableMigrations": true,
    "EnableDataSeeding": true
  }
}
```

此文件会自动覆盖 `database.json` 中的设置（仅在 Development 环境）。

### 创建生产环境配置

**⚠️ 重要：生产环境配置不要提交到 Git！**

1. 复制示例文件：
```bash
cp appsettings/database.Production.json.example appsettings/database.Production.json
cp appsettings/jwt.Production.json.example appsettings/jwt.Production.json
```

2. 编辑文件，填入真实的生产配置

3. 确认 `.gitignore` 已排除这些文件：
```gitignore
**/appsettings/database.Production.json
**/appsettings/jwt.Production.json
```

## ✅ 验证配置

### 方法 1: 检查启动日志

运行应用，确认看到配置加载日志。

### 方法 2: 创建测试端点

```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HealthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(new
        {
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            DatabaseConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")),
            JwtIssuer = _configuration["JwtSettings:Issuer"],
            LogLevel = _configuration["Serilog:MinimumLevel:Default"]
        });
    }
}
```

访问 `GET /api/health/config` 验证配置。

## 📊 配置优先级

当相同的配置键存在于多个地方时，优先级如下（后者覆盖前者）：

```
1. appsettings.json
2. appsettings/module.json
3. appsettings/module.{Environment}.json  ← 环境特定
4. User Secrets (dotnet user-secrets)
5. 环境变量                              ← 最高优先级
6. 命令行参数
```

**示例：**

```json
// database.json
{ "DatabaseSettings": { "EnableMigrations": false } }

// database.Development.json
{ "DatabaseSettings": { "EnableMigrations": true } }  ← 开发环境使用此值

// 环境变量
DatabaseSettings__EnableMigrations=false  ← 最终使用此值
```

## 🔒 安全最佳实践

### ✅ 推荐做法

1. **开发环境：** 使用 User Secrets
```bash
dotnet user-secrets set "JwtSettings:Secret" "your-dev-secret"
```

2. **生产环境：** 使用环境变量
```bash
export JwtSettings__Secret="your-production-secret"
```

3. **云服务：** 使用密钥管理服务
- Azure Key Vault
- AWS Secrets Manager
- Google Cloud Secret Manager

### ❌ 禁止做法

- ❌ 在配置文件中硬编码生产环境密码
- ❌ 将生产配置文件提交到 Git
- ❌ 在日志中输出敏感配置

## 🆘 常见问题

### Q: 配置文件找不到？

**A:** 检查文件路径和名称：
- 文件必须在 `appsettings/` 文件夹下
- 文件名必须完全匹配（大小写敏感）
- JSON 格式必须有效（使用 JSON 验证器检查）

### Q: 配置没有生效？

**A:** 检查优先级：
1. 确认没有环境变量覆盖了配置
2. 检查环境特定配置文件（如 `database.Development.json`）
3. 查看应用启动日志

### Q: 如何重置配置？

**A:** 删除环境特定配置文件：
```bash
rm appsettings/*.Development.json
rm appsettings/*.Production.json
```

然后重新编辑基础配置文件。

## 📚 相关文档

- [README.md](./README.md) - 详细配置说明
- [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) - 迁移指南
- [ASP.NET Core Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)

## 🎉 完成！

现在你已经掌握了模块化配置的基本用法。如需深入了解，请查阅 README.md 文档。

---

**最后更新：** 2024-01-16  
**文档版本：** 1.0
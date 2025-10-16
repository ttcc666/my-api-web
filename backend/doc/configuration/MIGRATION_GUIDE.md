# 配置文件模块化迁移指南

## 📖 概述

本指南帮助你从原有的单一 `appsettings.json` 配置文件迁移到模块化配置结构。

## ✅ 迁移前后对比

### 迁移前
```
MyApiWeb.Api/
├── appsettings.json (200+ 行，所有配置混在一起)
└── appsettings.Development.json
```

### 迁移后
```
MyApiWeb.Api/
├── appsettings.json (简化后，仅保留基础配置)
├── appsettings/
│   ├── README.md (配置说明文档)
│   ├── MIGRATION_GUIDE.md (本文档)
│   ├── database.json (数据库配置)
│   ├── jwt.json (JWT 认证配置)
│   ├── cors.json (CORS 跨域配置)
│   ├── cap.json (CAP 消息总线配置)
│   ├── serilog.json (日志配置)
│   ├── database.Production.json.example (生产环境示例)
│   └── jwt.Production.json.example (生产环境示例)
└── Program.cs (已更新配置加载逻辑)
```

## 🚀 迁移步骤

### 步骤 1: 验证迁移完成

迁移工作已自动完成，你需要验证：

#### 1.1 检查模块配置文件是否存在
```bash
ls appsettings/
```

应该看到以下文件：
- ✅ database.json
- ✅ jwt.json
- ✅ cors.json
- ✅ cap.json
- ✅ serilog.json
- ✅ README.md
- ✅ MIGRATION_GUIDE.md

#### 1.2 检查 Program.cs 是否更新
打开 `Program.cs`，确认包含以下代码：

```csharp
// 加载模块化配置文件
var environment = builder.Environment.EnvironmentName;
Log.Information("当前环境: {Environment}", environment);

// 定义需要加载的模块配置文件
var configModules = new[] { "database", "jwt", "cors", "cap", "serilog" };

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
```

### 步骤 2: 测试应用程序

#### 2.1 构建项目
```bash
dotnet build
```

确保没有编译错误。

#### 2.2 运行应用程序
```bash
dotnet run
```

#### 2.3 检查启动日志
应该看到类似输出：
```
[Information] 当前环境: Development
[Information] 已加载模块配置: database
[Information] 已加载模块配置: jwt
[Information] 已加载模块配置: cors
[Information] 已加载模块配置: cap
[Information] 已加载模块配置: serilog
[Information] 所有模块化配置文件加载完成
[Information] 启动应用程序
[Information] 应用程序启动完成
```

#### 2.4 验证配置值
创建一个测试端点验证配置是否正确加载：

```csharp
// Controllers/ConfigTestController.cs
[ApiController]
[Route("api/[controller]")]
public class ConfigTestController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigTestController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("check")]
    public IActionResult CheckConfig()
    {
        return Ok(new
        {
            DatabaseConnection = _configuration.GetConnectionString("DefaultConnection")?.Substring(0, 20) + "...",
            JwtIssuer = _configuration["JwtSettings:Issuer"],
            CorsOriginsCount = _configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()?.Length ?? 0,
            CapStorageType = _configuration["CAP:StorageType"],
            SerilogLevel = _configuration["Serilog:MinimumLevel:Default"]
        });
    }
}
```

访问 `https://localhost:5001/api/configtest/check` 验证配置。

### 步骤 3: 配置环境特定设置

#### 3.1 开发环境（可选）
如果需要本地开发专用配置，创建 `appsettings/database.Development.json`：

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

#### 3.2 生产环境
**重要：不要在代码仓库中创建生产配置文件！**

在部署时通过以下方式之一提供生产配置：

**方式 1: 环境变量**
```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=prod-server;..."
export JwtSettings__Secret="your-production-secret-key"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=prod-server;..."
$env:JwtSettings__Secret="your-production-secret-key"
```

**方式 2: 创建生产配置文件（仅在服务器上）**
```bash
# 在生产服务器上
cp appsettings/database.Production.json.example appsettings/database.Production.json
# 编辑并填入真实的生产配置
nano appsettings/database.Production.json
```

**方式 3: Azure App Service 配置**
在 Azure Portal 中直接配置应用程序设置。

### 步骤 4: 更新 CI/CD 管道

#### GitHub Actions 示例
```yaml
# .github/workflows/deploy.yml
- name: Deploy to Production
  run: |
    # 方式 1: 从 Secrets 创建配置文件
    echo '${{ secrets.PRODUCTION_DATABASE_CONFIG }}' > appsettings/database.Production.json
    echo '${{ secrets.PRODUCTION_JWT_CONFIG }}' > appsettings/jwt.Production.json
    
    # 方式 2: 使用环境变量
    export ConnectionStrings__DefaultConnection="${{ secrets.DB_CONNECTION }}"
    export JwtSettings__Secret="${{ secrets.JWT_SECRET }}"
    
    dotnet publish -c Release
```

#### Azure DevOps 示例
```yaml
# azure-pipelines.yml
- task: FileTransform@1
  inputs:
    folderPath: '$(System.DefaultWorkingDirectory)'
    fileType: 'json'
    targetFiles: 'appsettings/database.Production.json'
```

### 步骤 5: 清理工作

#### 5.1 备份原配置文件（可选）
```bash
# 如果你想保留原始配置作为参考
cp appsettings.json appsettings.json.backup
```

#### 5.2 验证 .gitignore
确认 `.gitignore` 包含以下规则：
```gitignore
# 排除所有生产环境配置
**/appsettings.Production.json
**/appsettings/database.Production.json
**/appsettings/jwt.Production.json
**/appsettings/cors.Production.json
**/appsettings/cap.Production.json
**/appsettings/serilog.Production.json
```

#### 5.3 提交代码
```bash
git add appsettings/
git add appsettings.json
git add Program.cs
git add .gitignore
git commit -m "重构: 将配置文件模块化

- 将单一配置文件拆分为按功能模块组织的独立文件
- 添加环境特定配置支持
- 更新配置加载逻辑
- 添加生产环境配置示例
- 更新 .gitignore 排除敏感配置文件"
git push
```

## 🔧 常见问题

### Q1: 应用启动时找不到配置
**症状：** 应用启动失败，提示配置键不存在

**解决：**
1. 检查配置文件路径是否正确（应该是 `appsettings/database.json`）
2. 验证 JSON 格式是否有效（使用 JSON 验证器）
3. 确认 Program.cs 中的配置加载代码已正确更新

### Q2: 配置被意外覆盖
**症状：** 某些配置值不是预期的

**解决：**
检查配置优先级：
1. appsettings.json（基础）
2. appsettings/module.json（模块基础）
3. appsettings/module.{Environment}.json（环境特定）
4. User Secrets（开发环境）
5. 环境变量（最高优先级）

### Q3: 如何在代码中访问配置
**答：** 配置访问方式不变

```csharp
// 方式 1: 直接注入 IConfiguration
public class MyService
{
    private readonly IConfiguration _configuration;
    
    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void DoSomething()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var jwtSecret = _configuration["JwtSettings:Secret"];
    }
}

// 方式 2: 使用 Options Pattern (推荐)
public class MyService
{
    private readonly JwtSettings _jwtSettings;
    
    public MyService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
}
```

### Q4: 如何添加新的配置模块
**答：**
1. 在 `appsettings/` 文件夹中创建新的 JSON 文件，例如 `redis.json`
2. 在 Program.cs 的 `configModules` 数组中添加模块名：
```csharp
var configModules = new[] { "database", "jwt", "cors", "cap", "serilog", "redis" };
```
3. 重启应用程序

### Q5: 生产环境如何管理敏感配置
**答：** 推荐以下安全实践

**不推荐 ❌**
- 在配置文件中硬编码敏感信息
- 将生产配置文件提交到代码仓库

**推荐 ✅**
- 使用环境变量
- 使用 Azure Key Vault / AWS Secrets Manager
- 通过 CI/CD 管道在部署时注入配置
- 使用加密的配置文件（仅在运行时解密）

### Q6: 如何回滚到旧的配置方式
**答：** 如果需要回滚：

1. 恢复原始的 `appsettings.json`（从备份或 Git 历史）
2. 删除或注释 Program.cs 中的模块化配置加载代码
3. 重新构建和部署

## 📊 配置验证清单

部署前请完成以下检查：

- [ ] 所有模块配置文件已创建
- [ ] Program.cs 配置加载逻辑已更新
- [ ] 应用程序可以正常启动
- [ ] 所有功能正常工作（数据库连接、JWT 认证等）
- [ ] .gitignore 已更新，排除生产配置
- [ ] 生产环境配置已准备好（环境变量或密钥管理服务）
- [ ] CI/CD 管道已更新
- [ ] 团队成员已被告知配置结构变更

## 📚 相关文档

- [README.md](./README.md) - 配置文件详细说明
- [ASP.NET Core Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Configuration Provider](https://docs.microsoft.com/aspnet/core/security/key-vault-configuration)

## 🆘 需要帮助？

如果在迁移过程中遇到问题：

1. 检查应用程序日志（`logs/` 文件夹）
2. 查阅 [README.md](./README.md) 配置说明
3. 联系团队技术负责人
4. 提交 Issue 到项目仓库

---

**迁移完成时间：** 2024-01-16  
**文档版本：** 1.0  
**维护者：** 开发团队
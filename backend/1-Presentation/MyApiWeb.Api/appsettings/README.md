# 模块化配置文件

本目录包含应用程序的模块化配置文件。

## 📁 配置文件

- `database.json` - 数据库连接和迁移配置
- `jwt.json` - JWT 身份认证配置
- `cors.json` - CORS 跨域配置
- `cap.json` - CAP 消息总线配置
- `serilog.json` - Serilog 日志配置

## 📖 完整文档

详细的配置说明、使用指南和最佳实践请查看：

- **快速开始指南**: [`/backend/doc/configuration/QUICK_START.md`](../../../doc/configuration/QUICK_START.md)
- **详细配置说明**: [`/backend/doc/configuration/README.md`](../../../doc/configuration/README.md)
- **配置迁移指南**: [`/backend/doc/configuration/MIGRATION_GUIDE.md`](../../../doc/configuration/MIGRATION_GUIDE.md)

## 🚀 快速配置

### 1. 配置数据库连接
编辑 `database.json` 修改连接字符串

### 2. 配置 JWT 密钥
```bash
dotnet user-secrets set "JwtSettings:Secret" "your-secret-key-at-least-32-characters"
```

### 3. 运行应用
```bash
dotnet run
```

## 🔗 相关链接

- [项目文档目录](../../../doc/)
- [CAP 集成指南](../../../doc/CAP_SqlSugar_Integration_Guide.md)
# 配置文档索引

欢迎来到 MyApiWeb 项目配置文档中心。

## 📚 文档列表

### 核心文档

| 文档 | 描述 | 适用对象 |
|------|------|----------|
| [快速开始指南](./QUICK_START.md) | 5分钟快速配置和运行应用 | 新手开发者 |
| [配置详细说明](./README.md) | 完整的配置说明、最佳实践和安全指南 | 所有开发者 |
| [配置迁移指南](./MIGRATION_GUIDE.md) | 从单一配置迁移到模块化配置 | 运维人员、团队负责人 |

## 🎯 根据需求选择文档

### 我是新手，想快速开始
👉 阅读 [快速开始指南](./QUICK_START.md)

包含：
- 最小化配置步骤
- 常见配置场景
- 快速验证方法

### 我需要了解详细的配置选项
👉 阅读 [配置详细说明](./README.md)

包含：
- 每个配置文件的详细说明
- 环境特定配置
- 安全最佳实践
- 配置优先级
- 常见使用场景

### 我需要将现有项目迁移到模块化配置
👉 阅读 [配置迁移指南](./MIGRATION_GUIDE.md)

包含：
- 迁移前后对比
- 详细迁移步骤
- CI/CD 管道更新
- 常见问题解答

## 📁 配置文件位置

```
MyApiWeb.Api/
├── appsettings.json                      # 主配置（基础设置）
└── appsettings/                          # 模块化配置目录
    ├── database.json                     # 数据库配置
    ├── jwt.json                          # JWT 认证配置
    ├── cors.json                         # CORS 跨域配置
    ├── cap.json                          # CAP 消息总线配置
    ├── serilog.json                      # Serilog 日志配置
    ├── database.Production.json.example  # 生产环境示例
    └── jwt.Production.json.example       # 生产环境示例
```

## 🔧 配置模块概览

### 数据库配置 (`database.json`)
- 数据库连接字符串
- 自动迁移设置
- 数据种子配置

### JWT 配置 (`jwt.json`)
- JWT 签名密钥
- Token 有效期
- 签发者和受众

### CORS 配置 (`cors.json`)
- 允许的前端域名
- 跨域策略

### CAP 配置 (`cap.json`)
- 消息存储类型
- 传输方式
- 重试策略

### 日志配置 (`serilog.json`)
- 日志级别
- 输出目标
- 日志增强器

## 🚀 快速链接

### 常见任务

- [配置数据库连接](./QUICK_START.md#步骤-1-配置数据库连接-必需)
- [设置 JWT 密钥](./QUICK_START.md#步骤-2-配置-jwt-密钥-必需)
- [配置 CORS](./QUICK_START.md#步骤-3-配置-cors-推荐)
- [环境特定配置](./README.md#-环境特定配置)
- [安全最佳实践](./README.md#-安全最佳实践)

### 故障排查

- [配置文件找不到？](./QUICK_START.md#q-配置文件找不到)
- [配置没有生效？](./QUICK_START.md#q-配置没有生效)
- [如何重置配置？](./QUICK_START.md#q-如何重置配置)

## 📖 相关文档

- [CAP 与 SqlSugar 集成指南](../CAP_SqlSugar_Integration_Guide.md)
- [项目文档根目录](../)

## 🆘 获取帮助

如果在配置过程中遇到问题：

1. 查看 [快速开始指南](./QUICK_START.md) 的常见问题部分
2. 查看 [详细配置说明](./README.md) 的故障排查章节
3. 查看应用日志文件（`logs/` 目录）
4. 联系团队技术负责人
5. 在项目仓库提交 Issue

---

**文档版本**: 1.0  
**最后更新**: 2024-01-16  
**维护者**: 开发团队
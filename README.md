# My API Web 项目

基于 .NET 9 和 Vue 3 的现代化前后端分离 Web 应用。

## 🏗️ 项目架构

### 后端技术栈
- **.NET 9 Web API** - 现代化后端框架
- **SqlSugar ORM** - 高性能.NET ORM框架
- **SQL Server** - 关系型数据库
- **Autofac** - 依赖注入容器
- **JWT Bearer 认证** - 无状态认证
- **Swagger** - API文档生成
- **三层架构** - 清晰的代码分层

### 前端技术栈
- **Vue 3** - 渐进式JavaScript框架
- **TypeScript** - 类型安全
- **Vite** - 快速构建工具
- **Pinia** - 状态管理
- **Vue Router** - 路由管理
- **Axios** - HTTP客户端

## 📁 项目结构

```
my-api-web/
├── backend/                          # 后端项目（三层架构）
│   ├── 1-Presentation/               # 表示层
│   │   └── MyApiWeb.Api/             # Web API 项目
│   ├── 2-Business/                   # 业务逻辑层
│   │   ├── MyApiWeb.Services/        # 业务服务
│   │   └── MyApiWeb.Models/          # 实体模型
│   └── 3-DataAccess/                 # 数据访问层
│       └── MyApiWeb.Repository/      # 数据仓储
├── frontend/                         # 前端项目
│   ├── src/
│   │   ├── api/                      # API调用封装
│   │   ├── components/               # Vue组件
│   │   ├── stores/                   # Pinia状态管理
│   │   ├── types/                    # TypeScript类型定义
│   │   └── utils/                    # 工具函数
│   └── ...
└── docs/                             # 项目文档
```

## 🚀 快速开始

### 环境要求

- **.NET 9 SDK** - [下载地址](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [下载地址](https://nodejs.org/)
- **SQL Server** - 本地或云端实例
- **Git** - 版本控制

### 1. 克隆项目

```bash
git clone <repository-url>
cd my-api-web
```

### 2. 后端启动

```bash
# 进入后端项目目录
cd backend/1-Presentation/MyApiWeb.Api

# 恢复NuGet包
dotnet restore

# 更新数据库连接字符串（可选）
# 编辑 appsettings.json 中的 ConnectionStrings

# 编译项目
dotnet build

# 启动后端API
dotnet run
```

后端将在 `http://localhost:5000` 启动，Swagger文档可通过 `http://localhost:5000` 访问。

### 3. 前端启动

```bash
# 新开终端窗口，进入前端项目目录
cd frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

前端将在 `http://localhost:3000` 启动。

## 🔧 开发配置

### 后端配置

#### 数据库配置
在 `backend/1-Presentation/MyApiWeb.Api/appsettings.json` 中修改数据库连接：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApiWebDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### JWT配置
```json
{
  "JwtSettings": {
    "SecretKey": "你的密钥",
    "Issuer": "MyApiWeb",
    "Audience": "MyApiWebUsers",
    "ExpirationInMinutes": 60
  }
}
```

### 前端配置

#### 环境变量
在 `frontend/.env.development` 中配置开发环境：

```bash
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_TITLE=My API Web
```

## 📚 API 文档

### 用户管理 API

| 方法 | 端点 | 描述 | 认证 |
|------|------|------|------|
| POST | `/api/users/register` | 用户注册 | ❌ |
| POST | `/api/users/login` | 用户登录 | ❌ |
| GET | `/api/users/profile` | 获取当前用户信息 | ✅ |
| GET | `/api/users` | 获取所有用户 | ✅ |
| GET | `/api/users/{id}` | 根据ID获取用户 | ✅ |
| PUT | `/api/users/{id}` | 更新用户信息 | ✅ |
| DELETE | `/api/users/{id}` | 删除用户 | ✅ |

### 请求示例

#### 用户注册
```bash
POST /api/users/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "realName": "测试用户",
  "phone": "13800138000"
}
```

#### 用户登录
```bash
POST /api/users/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123"
}
```

## 🛠️ 开发脚本

### 后端脚本
```bash
# 编译项目
dotnet build

# 运行项目
dotnet run

# 运行测试
dotnet test

# 发布项目
dotnet publish -c Release
```

### 前端脚本
```bash
# 开发环境启动
npm run dev

# 构建生产版本
npm run build

# 预览构建结果
npm run preview

# 代码检查
npm run lint

# 格式化代码
npm run format

# 运行测试
npm run test
```

## 🚢 部署指南

### 后端部署

#### 1. IIS部署
```bash
# 发布项目
dotnet publish -c Release -o ./publish

# 将publish文件夹内容复制到IIS网站目录
# 配置应用程序池为.NET Core
# 设置正确的数据库连接字符串
```

#### 2. Docker部署
```dockerfile
# 创建Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MyApiWeb.Api.dll"]
```

### 前端部署

#### 1. 静态文件部署
```bash
# 构建生产版本
npm run build

# 将dist文件夹内容部署到Web服务器
# 配置反向代理指向后端API
```

#### 2. Nginx配置示例
```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        root /path/to/dist;
        index index.html;
        try_files $uri $uri/ /index.html;
    }
    
    location /api {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## 🐛 故障排除

### 常见问题

1. **数据库连接失败**
   - 检查SQL Server是否启动
   - 确认连接字符串正确
   - 检查数据库权限

2. **CORS错误**
   - 确认后端CORS配置正确
   - 检查前端API请求地址

3. **JWT认证失败**
   - 检查JWT密钥配置
   - 确认token未过期

4. **前端路由问题**
   - 检查Vue Router配置
   - 确认代理设置正确

## 🤝 贡献指南

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 📞 联系方式

- 项目地址: [GitHub Repository]
- 问题反馈: [GitHub Issues]
- 邮箱: your-email@example.com

---

## 🎯 下一步计划

- [ ] 添加用户角色权限管理
- [ ] 实现文件上传功能
- [ ] 添加数据导出功能
- [ ] 集成第三方登录
- [ ] 添加实时通知功能
- [ ] 移动端适配
- [ ] 多语言支持
- [ ] 性能监控和日志系统
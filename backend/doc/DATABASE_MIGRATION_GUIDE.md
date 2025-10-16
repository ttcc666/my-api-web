# 数据库结构优化迁移指南

## 📋 变更概述

本次数据库优化实施了以下三项重要变更：

1. **表名规范化**：为所有表添加模块前缀（`Sys_`, `Auth_`, `Hub_`）
2. **字段名规范化**：所有数据库列名添加 `F_` 前缀
3. **扩展字段增强**：为所有表添加 10 个自定义扩展字段

**变更日期**：2025-01-16
**影响级别**：🔴 破坏性变更（需要重建数据库）

---

## 🎯 变更详情

### 1. 表名变更映射

| 原表名 | 新表名 | 模块分类 |
|--------|--------|----------|
| `Users` | `Sys_Users` | 系统模块 |
| `Roles` | `Sys_Roles` | 系统模块 |
| `Permissions` | `Sys_Permissions` | 系统模块 |
| `Menus` | `Sys_Menus` | 系统模块 |
| `UserRoles` | `Sys_UserRoles` | 系统模块 |
| `RolePermissions` | `Sys_RolePermissions` | 系统模块 |
| `UserPermissions` | `Sys_UserPermissions` | 系统模块 |
| `SeedHistory` | `Sys_SeedHistory` | 系统模块 |
| `RefreshTokens` | `Auth_RefreshTokens` | 认证模块 |
| `OnlineUsers` | `Hub_OnlineUsers` | 实时通信模块 |

### 2. 字段名变更规则

所有列名添加 `F_` 前缀，例如：
- `Id` → `F_Id`
- `Username` → `F_Username`
- `Email` → `F_Email`
- `CreationTime` → `F_CreationTime`

**注意**：C# 代码中的属性名保持不变，仅数据库列名改变。

### 3. 新增扩展字段（所有继承 EntityBase 的表）

| 字段名 | 数据库列名 | 类型 | 长度/精度 | 可空 | 说明 |
|--------|-----------|------|----------|------|------|
| `Extend1` | `F_Extend1` | nvarchar | 500 | ✅ | 通用字符串扩展字段 |
| `Extend2` | `F_Extend2` | nvarchar | 500 | ✅ | 通用字符串扩展字段 |
| `Extend3` | `F_Extend3` | nvarchar | 500 | ✅ | 通用字符串扩展字段 |
| `Extend4` | `F_Extend4` | nvarchar | 500 | ✅ | 通用字符串扩展字段 |
| `Extend5` | `F_Extend5` | nvarchar | 500 | ✅ | 通用字符串扩展字段 |
| `ExtendInt1` | `F_ExtendInt1` | int | - | ✅ | 通用整数扩展字段 |
| `ExtendInt2` | `F_ExtendInt2` | int | - | ✅ | 通用整数扩展字段 |
| `ExtendInt3` | `F_ExtendInt3` | int | - | ✅ | 通用整数扩展字段 |
| `ExtendDate1` | `F_ExtendDate1` | datetimeoffset | - | ✅ | 通用日期扩展字段 |
| `ExtendDate2` | `F_ExtendDate2` | datetimeoffset | - | ✅ | 通用日期扩展字段 |

---

## 📊 表结构示例

### 优化前（Users 表）

```sql
CREATE TABLE [dbo].[Users] (
    [Id] nvarchar(36) PRIMARY KEY,
    [Username] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [RealName] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [IsActive] bit NOT NULL,
    [UpdatedTime] datetime2 NULL,
    [LastLoginTime] datetime2 NULL,
    [CreationTime] datetimeoffset NOT NULL,
    [CreatorId] uniqueidentifier NOT NULL,
    [LastModificationTime] datetimeoffset NULL,
    [LastModifierId] uniqueidentifier NULL
);
```

### 优化后（Sys_Users 表）

```sql
CREATE TABLE [dbo].[Sys_Users] (
    -- 基础字段
    [F_Id] nvarchar(36) PRIMARY KEY,
    [F_Username] nvarchar(50) NOT NULL,
    [F_Email] nvarchar(100) NOT NULL,
    [F_PasswordHash] nvarchar(255) NOT NULL,
    [F_RealName] nvarchar(100) NULL,
    [F_Phone] nvarchar(20) NULL,
    [F_IsActive] bit NOT NULL,
    [F_UpdatedTime] datetime2 NULL,
    [F_LastLoginTime] datetime2 NULL,
    
    -- EntityBase 字段
    [F_CreationTime] datetimeoffset NOT NULL,
    [F_CreatorId] uniqueidentifier NOT NULL,
    [F_LastModificationTime] datetimeoffset NULL,
    [F_LastModifierId] uniqueidentifier NULL,
    
    -- 扩展字段
    [F_Extend1] nvarchar(500) NULL,
    [F_Extend2] nvarchar(500) NULL,
    [F_Extend3] nvarchar(500) NULL,
    [F_Extend4] nvarchar(500) NULL,
    [F_Extend5] nvarchar(500) NULL,
    [F_ExtendInt1] int NULL,
    [F_ExtendInt2] int NULL,
    [F_ExtendInt3] int NULL,
    [F_ExtendDate1] datetimeoffset NULL,
    [F_ExtendDate2] datetimeoffset NULL
);
```

---

## 🚀 迁移步骤

### 方案选择

由于表名和字段名都发生了变更，推荐使用**删库重建方案**（适用于开发环境）。

#### 开发环境迁移（推荐）

1. **备份当前数据（可选）**
   ```sql
   -- 如果需要保留测试数据，执行数据导出
   -- 使用 SQL Server Management Studio 的"生成脚本"功能
   ```

2. **删除现有数据库**
   ```sql
   USE master;
   GO
   DROP DATABASE IF EXISTS [MyApiWebDb];
   GO
   ```

3. **确认配置**
   
   检查 `appsettings/database.json`，确保 `EnableMigrations` 为 `true`：
   ```json
   {
     "DatabaseSettings": {
       "EnableMigrations": true
     }
   }
   ```

4. **启动后端应用**
   ```bash
   cd backend/1-Presentation/MyApiWeb.Api
   dotnet run
   ```
   
   应用启动时会自动：
   - 创建数据库
   - 创建所有表（新表名和新字段结构）
   - 执行数据种子

5. **验证表结构**
   ```sql
   -- 检查表名
   SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_TYPE = 'BASE TABLE' 
   ORDER BY TABLE_NAME;
   
   -- 检查 Sys_Users 表的列名
   SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'Sys_Users'
   ORDER BY ORDINAL_POSITION;
   ```

6. **测试功能**
   - 访问 Swagger: `http://localhost:5000`
   - 测试用户注册/登录
   - 验证权限和菜单数据

#### 生产环境迁移（慎重）

⚠️ **警告**：生产环境迁移需要保留业务数据，操作更加复杂。

如果生产环境必须迁移，建议：

1. **完整备份数据库**
   ```sql
   BACKUP DATABASE [MyApiWebDb] 
   TO DISK = 'D:\Backup\MyApiWebDb_Before_Migration.bak'
   WITH FORMAT, COMPRESSION;
   ```

2. **创建数据迁移脚本**
   
   需要手动编写脚本，将旧表数据迁移到新表，例如：
   ```sql
   -- 创建新表
   -- （通过在测试环境运行应用自动生成，然后导出 DDL）
   
   -- 迁移数据
   INSERT INTO [Sys_Users] (
       [F_Id], [F_Username], [F_Email], [F_PasswordHash],
       [F_RealName], [F_Phone], [F_IsActive], [F_UpdatedTime],
       [F_LastLoginTime], [F_CreationTime], [F_CreatorId],
       [F_LastModificationTime], [F_LastModifierId]
   )
   SELECT 
       [Id], [Username], [Email], [PasswordHash],
       [RealName], [Phone], [IsActive], [UpdatedTime],
       [LastLoginTime], [CreationTime], [CreatorId],
       [LastModificationTime], [LastModifierId]
   FROM [Users];
   
   -- 删除旧表
   DROP TABLE [Users];
   
   -- 对所有其他表重复此过程
   ```

3. **建议**：考虑在生产环境保持当前表结构，仅在新项目中使用新规范。

---

## 💻 代码影响分析

### ✅ 无影响的部分

由于使用了 SqlSugar 的 `ColumnName` 映射，以下部分**完全不受影响**：

1. **业务服务层（Services）**：属性访问方式不变
   ```csharp
   var user = await _userService.GetByIdAsync(userId);
   Console.WriteLine(user.Username); // 属性名未变
   ```

2. **API 控制器（Controllers）**：DTO 映射不受影响
   ```csharp
   var userDto = new UserDto {
       Username = user.Username,  // 仍然使用友好的属性名
       Email = user.Email
   };
   ```

3. **前端代码**：通过 API 访问，完全隔离
   ```typescript
   const user = await api.getUser(userId);
   console.log(user.username); // API 返回的 DTO 不变
   ```

### ⚠️ 需要注意的部分

1. **原生 SQL 查询**：如果代码中使用了原生 SQL 字符串，需要更新列名
   ```csharp
   // ❌ 旧代码（会出错）
   var sql = "SELECT Id, Username FROM Users WHERE Email = @email";
   
   // ✅ 新代码
   var sql = "SELECT F_Id, F_Username FROM Sys_Users WHERE F_Email = @email";
   ```

2. **数据库视图/存储过程**：如果有自定义视图或存储过程引用了表名或列名，需要手动更新

3. **报表和 BI 工具**：外部数据分析工具可能需要重新配置数据源

---

## 🔧 扩展字段使用示例

### 场景1：存储用户配置（JSON）

```csharp
var user = await _userRepository.GetByIdAsync(userId);
user.Extend1 = JsonSerializer.Serialize(new {
    Theme = "dark",
    Language = "zh-CN",
    NotificationsEnabled = true
});
await _userRepository.UpdateAsync(user);
```

### 场景2：记录业务状态码

```csharp
var role = await _roleRepository.GetByIdAsync(roleId);
role.ExtendInt1 = 1; // 1=待审核, 2=已批准, 3=已拒绝
role.ExtendDate1 = DateTimeOffset.Now; // 状态变更时间
await _roleRepository.UpdateAsync(role);
```

### 场景3：临时标记和备注

```csharp
var permission = await _permissionRepository.GetByIdAsync(permissionId);
permission.Extend2 = "临时授权，30天后自动撤销";
permission.ExtendDate2 = DateTimeOffset.Now.AddDays(30); // 过期时间
await _permissionRepository.UpdateAsync(permission);
```

---

## ✅ 验证清单

迁移完成后，请逐项检查：

- [ ] 数据库中所有表名符合新规范（`Sys_`, `Auth_`, `Hub_` 前缀）
- [ ] 所有列名以 `F_` 开头
- [ ] 继承 EntityBase 的表都包含 10 个扩展字段
- [ ] 数据种子成功执行（检查 `Sys_SeedHistory` 表）
- [ ] API 接口正常响应（Swagger 测试）
- [ ] 用户注册/登录功能正常
- [ ] RBAC 权限控制功能正常
- [ ] 菜单数据正确加载
- [ ] SignalR 在线用户功能正常
- [ ] JWT Token 刷新机制正常

---

## 🐛 故障排查

### 问题1：应用启动时未自动创建表

**原因**：`EnableMigrations` 配置未启用

**解决方案**：
```json
// appsettings/database.json
{
  "DatabaseSettings": {
    "EnableMigrations": true
  }
}
```

### 问题2：数据种子执行失败

**现象**：应用启动后，表是空的

**解决方案**：
1. 检查日志文件 `logs/log-{Date}.txt`
2. 查看错误详情
3. 删除 `Sys_SeedHistory` 表记录，重启应用重新执行种子

### 问题3：API 返回 500 错误

**原因**：可能存在硬编码的 SQL 语句使用了旧表名

**解决方案**：
1. 查看详细错误日志
2. 搜索代码中的 SQL 字符串：`grep -r "FROM Users" backend/`
3. 更新为新表名和列名

### 问题4：前端无法加载菜单

**原因**：菜单数据种子可能未执行

**解决方案**：
```sql
-- 检查菜单数据
SELECT F_Code, F_Title, F_RoutePath FROM Sys_Menus ORDER BY F_Order;

-- 如果为空，检查种子历史
SELECT * FROM Sys_SeedHistory WHERE F_SeedName LIKE '%Menu%';
```

---

## 📞 技术支持

如遇到迁移问题，请：

1. 查看应用日志：`backend/1-Presentation/MyApiWeb.Api/logs/`
2. 检查数据库连接字符串配置
3. 确认 SQL Server 版本兼容性（建议 SQL Server 2019+）
4. 联系技术团队或在项目仓库提交 Issue

---

## 📚 相关文档

- [配置文档中心](./configuration/INDEX.md)
- [数据库配置说明](./configuration/README.md#-数据库配置-databasejson)
- [项目主文档](../README.md)
- [CLAUDE.md - 代码库指南](../../CLAUDE.md)

---

**文档版本**：1.0
**最后更新**：2025-01-16
**适用版本**：.NET 9.0 + SqlSugar 5.x

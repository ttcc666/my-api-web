# DotNetCore.CAP 与 SqlSugar 集成指南

## 目录
- [概述](#概述)
- [为什么需要自定义集成](#为什么需要自定义集成)
- [核心集成代码](#核心集成代码)
- [服务配置](#服务配置)
- [CAP Dashboard](#cap-dashboard)
- [实际使用示例](#实际使用示例)
- [异步支持说明](#异步支持说明)
- [最佳实践](#最佳实践)

---

## 概述

本指南详细介绍如何在 .NET 应用程序中集成 **DotNetCore.CAP** 消息总线与 **SqlSugar** ORM，实现可靠的事务性发件箱模式（Transactional Outbox Pattern）。

**事务性发件箱模式**确保数据库操作和消息发布在同一个事务中完成，从而保证数据一致性：
- ✅ 数据库操作成功 → 消息发布成功
- ✅ 数据库操作失败 → 消息不会发布
- ✅ 避免分布式事务中的数据不一致问题

---

## 为什么需要自定义集成

DotNetCore.CAP 原生支持以下 ORM：
- ✅ Entity Framework Core
- ✅ ADO.NET

但是，当使用第三方 ORM（如 **SqlSugar**）时，需要创建自定义事务包装器来实现与 CAP 的集成。

**自定义集成的核心目标：**
1. 将 SqlSugar 的事务与 CAP 的消息发布绑定在一起
2. 确保事务提交时，数据库更改和消息发布同时生效
3. 确保事务回滚时，消息不会被发布

---

## 核心集成代码

### 1. SqlSugar 事务包装器

创建文件：`MyApiWeb.Services/Cap/SqlSugarCapTransaction.cs`

```csharp
using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace MyApiWeb.Infrastructure.Cap;

/// <summary>
/// SqlSugar事务包装器,用于与DotNetCore.CAP集成
/// 实现事务性发件箱模式,确保数据库操作和消息发布在同一事务中
/// </summary>
public class SqlSugarCapTransaction : CapTransactionBase
{
    public SqlSugarCapTransaction(IDispatcher dispatcher, IAdo ado) : base(dispatcher)
    {
        Ado = ado;
        DbTransaction = ado.Transaction;
    }

    public IAdo Ado { get; set; }

    public override void Commit()
    {
        Ado.CommitTran();
        Flush();
    }

    public override async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Ado.CommitTranAsync();
        Flush();
    }

    public override void Dispose()
    {
        Ado.Dispose();
    }

    public override void Rollback()
    {
        Ado.RollbackTran();
    }

    public override async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Ado.RollbackTranAsync();
    }
}
```

### 2. 扩展方法

```csharp
public static class SqlSugarCapExtensions
{
    public static ICapTransaction BeginCapTransaction(
        this ISqlSugarClient sqlSugarClient, 
        ICapPublisher publisher, 
        bool autoCommit = false)
    {
        var dispatcher = publisher.ServiceProvider.GetRequiredService<IDispatcher>();
        sqlSugarClient.Ado.BeginTran();
        var transaction = new SqlSugarCapTransaction(dispatcher, sqlSugarClient.Ado)
        {
            AutoCommit = autoCommit
        };
        return publisher.Transaction.Value = transaction;
    }
}
```

---

## 服务配置

### 1. 添加 NuGet 包

在 `MyApiWeb.Infrastructure.csproj` 中添加：

```xml
<PackageReference Include="DotNetCore.CAP" Version="8.3.2" />
<PackageReference Include="DotNetCore.CAP.SqlServer" Version="8.3.2" />
<PackageReference Include="DotNetCore.CAP.InMemoryStorage" Version="8.3.2" />
<PackageReference Include="DotNetCore.CAP.RabbitMQ" Version="8.3.2" />
```

### 2. 配置 appsettings.json

```json
{
  "CAP": {
    "StorageType": "InMemory",
    "TransportType": "InMemory",
    "FailedRetryCount": 3,
    "FailedRetryInterval": 60,
    "ConsumerThreadCount": 1
  }
}
```

### 3. 创建 CAP 服务配置

创建文件：`MyApiWeb.Infrastructure/Configuration/CapServiceExtensions.cs`

```csharp
public static class CapServiceExtensions
{
    public static IServiceCollection AddCapMessageBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            ConfigureStorage(options, configuration);
            ConfigureTransport(options, configuration);

            options.FailedRetryCount = configuration.GetValue<int>("CAP:FailedRetryCount", 3);
            options.FailedRetryInterval = configuration.GetValue<int>("CAP:FailedRetryInterval", 60);
            options.ConsumerThreadCount = configuration.GetValue<int>("CAP:ConsumerThreadCount", 1);
            
            // 配置失败消息回调
            options.FailedThresholdCallback = failed =>
            {
                var logger = failed.ServiceProvider.GetRequiredService<ILogger<CapOptions>>();
                logger.LogError(
                    "CAP消息处理失败达到阈值: MessageType={MessageType}, Message={Message}",
                    failed.MessageType,
                    failed.Message.ToString()
                );
            };
        });

        return services;
    }
}
```

### 4. 在 Program.cs 中注册

```csharp
builder.Services.AddCapMessageBus(builder.Configuration);
```

---

## CAP Dashboard

CAP Dashboard 提供了一个可视化界面来监控消息的发布和消费情况。

### 访问 Dashboard

启动应用后，访问：`http://localhost:5000/cap` 或 `https://localhost:5001/cap`

### Dashboard 功能

- **Published Messages**: 查看已发布的消息列表
- **Received Messages**: 查看已接收的消息列表
- **Subscribers**: 查看所有订阅者
- **实时监控**: 查看消息处理状态（成功/失败/重试）

### 配置说明

Dashboard 已在 `CapServiceExtensions.cs` 中自动配置：

```csharp
options.UseDashboard();
```

**注意**: 生产环境建议添加身份验证保护 Dashboard 访问。

---

## 实际使用示例

### 1. 创建产品服务

```csharp
public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ICapPublisher _capPublisher;

    public ProductService(
        IRepository<Product> productRepository,
        ISqlSugarClient sqlSugarClient,
        ICapPublisher capPublisher)
    {
        _productRepository = productRepository;
        _sqlSugarClient = sqlSugarClient;
        _capPublisher = capPublisher;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        using var transaction = _sqlSugarClient.BeginCapTransaction(_capPublisher, autoCommit: false);
        
        try
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                Stock = request.Stock
            };

            await _productRepository.InsertAsync(product);
            
            await _capPublisher.PublishAsync("product.created", new
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price
            });

            await transaction.CommitAsync();

            return new ProductDto { Id = product.Id, Name = product.Name };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### 2. 创建消息订阅者

```csharp
public class ProductEventSubscriber : ICapSubscribe
{
    private readonly ILogger<ProductEventSubscriber> _logger;

    public ProductEventSubscriber(ILogger<ProductEventSubscriber> logger)
    {
        _logger = logger;
    }

    [CapSubscribe("product.created")]
    public Task HandleProductCreated(ProductCreatedEvent eventData)
    {
        _logger.LogInformation(
            "收到产品创建事件: ProductId={ProductId}, Name={Name}",
            eventData.ProductId,
            eventData.Name);

        return Task.CompletedTask;
    }
}
```

### 3. 创建失败消息订阅者

```csharp
public class CapFailedMessageSubscriber : ICapSubscribe
{
    private readonly ILogger<CapFailedMessageSubscriber> _logger;

    public CapFailedMessageSubscriber(ILogger<CapFailedMessageSubscriber> logger)
    {
        _logger = logger;
    }

    [CapSubscribe("cap.failed.message")]
    public Task HandleFailedMessage(FailedMessageContext context)
    {
        _logger.LogError(
            "CAP消息处理失败: Topic={Topic}, MessageId={MessageId}",
            context.Topic,
            context.MessageId
        );

        return Task.CompletedTask;
    }
}
```

---

## 异步支持说明

在当前实现中，`CommitAsync()` 和 `RollbackAsync()` 方法在大多数场景下是**安全可用**的。

**最佳实践：**
1. ✅ 在单个请求范围内使用
2. ✅ 确保事务在同一个异步方法中完成
3. ⚠️ 避免跨请求的长事务
4. ⚠️ 测试关键场景

---

## 最佳实践

### 1. 事务范围控制

```csharp
using var transaction = _sqlSugarClient.BeginCapTransaction(_capPublisher);
try
{
    // 所有数据库操作和消息发布
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 2. 失败消息处理

**FailedThresholdCallback**：全局失败回调
**Failed Message Subscription**：订阅失败消息进行补偿

配置失败重试参数：

```json
{
  "CAP": {
    "FailedRetryCount": 3,
    "FailedRetryInterval": 60
  }
}
```

### 3. 消息幂等性

```csharp
[CapSubscribe("product.created")]
public async Task HandleProductCreated(ProductCreatedEvent eventData)
{
    if (await _cache.ExistsAsync($"processed:{eventData.ProductId}"))
        return;
    
    await ProcessEvent(eventData);
    await _cache.SetAsync($"processed:{eventData.ProductId}", true);
}
```

---

## 总结

通过本指南，您已经学会了：

1. ✅ 实现 SqlSugar 与 CAP 的自定义集成
2. ✅ 配置 CAP 服务和消息传输
3. ✅ 在业务中使用事务性发件箱模式
4. ✅ 处理失败消息和实现补偿逻辑

现在您可以在项目中安全地使用 CAP 与 SqlSugar 的集成，实现可靠的分布式事务处理！
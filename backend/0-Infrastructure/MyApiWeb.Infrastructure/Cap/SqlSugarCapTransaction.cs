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
    /// <summary>
    /// 初始化SqlSugar CAP事务
    /// </summary>
    /// <param name="dispatcher">CAP消息分发器</param>
    /// <param name="ado">SqlSugar ADO对象</param>
    public SqlSugarCapTransaction(IDispatcher dispatcher, IAdo ado) : base(dispatcher)
    {
        Ado = ado;
        DbTransaction = ado.Transaction;
    }

    /// <summary>
    /// SqlSugar ADO对象
    /// </summary>
    public IAdo Ado { get; set; }

    /// <summary>
    /// 提交事务(同步)
    /// </summary>
    public override void Commit()
    {
        Ado.CommitTran();
        Flush();
    }

    /// <summary>
    /// 提交事务(异步)
    /// </summary>
    public override async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Ado.CommitTranAsync();
        Flush();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public override void Dispose()
    {
        Ado.Dispose();
    }

    /// <summary>
    /// 回滚事务(同步)
    /// </summary>
    public override void Rollback()
    {
        Ado.RollbackTran();
    }

    /// <summary>
    /// 回滚事务(异步)
    /// </summary>
    public override async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Ado.RollbackTranAsync();
    }
}

/// <summary>
/// SqlSugar CAP事务扩展方法
/// </summary>
public static class SqlSugarCapExtensions
{
    /// <summary>
    /// 开始一个CAP事务,用于SqlSugar
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar客户端</param>
    /// <param name="publisher">CAP消息发布器</param>
    /// <param name="autoCommit">是否自动提交事务</param>
    /// <returns>CAP事务对象</returns>
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
        publisher.Transaction = transaction;
        return transaction;
    }
}
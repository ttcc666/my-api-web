using System;

namespace MyApiWeb.Models.Interfaces
{
    /// <summary>
    /// 当前登录用户上下文接口
    /// </summary>
    /// <remarks>
    /// 用于在业务逻辑层和数据访问层中获取当前请求的登录用户信息。
    /// 通常从 JWT Token 的 Claims 中解析用户 ID,并通过依赖注入提供给各个服务使用。
    /// </remarks>
    public interface ICurrentUser
    {
        /// <summary>
        /// 获取当前登录用户的唯一标识 (GUID)
        /// </summary>
        /// <value>
        /// 用户 ID (Guid?): 如果用户已认证则返回用户 ID,否则返回 null
        /// </value>
        /// <remarks>
        /// - 在已认证的请求中: 从 JWT Token 的 ClaimTypes.NameIdentifier 解析
        /// - 在匿名请求中: 返回 null
        /// - 用于实现审计功能 (CreatedBy/UpdatedBy 自动填充)
        /// </remarks>
        Guid? Id { get; }
    }
}
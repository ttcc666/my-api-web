using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MyApiWeb.Infrastructure.Middlewares;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// 中间件管道配置扩展
    /// </summary>
    public static class MiddlewarePipelineExtensions
    {
        /// <summary>
        /// 配置自定义中间件管道
        /// </summary>
        public static WebApplication UseCustomMiddlewarePipeline(this WebApplication app)
        {
            var environment = app.Environment;

            // Swagger 文档 (仅开发环境)
            if (environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API Web v1");
                    c.RoutePrefix = string.Empty; // 设置 Swagger UI 为默认页面
                });
            }

            // 全局异常处理中间件 (必须在最前面)
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // HTTPS 重定向
            app.UseHttpsRedirection();

            // CORS (根据环境选择策略)
            if (environment.IsDevelopment())
            {
                app.UseCors("DevelopmentPolicy");
            }
            else
            {
                app.UseCors("ProductionPolicy");
            }

            // 认证和授权
            app.UseAuthentication();
            app.UseAuthorization();

            // 控制器路由
            app.MapControllers();

            return app;
        }
    }
}
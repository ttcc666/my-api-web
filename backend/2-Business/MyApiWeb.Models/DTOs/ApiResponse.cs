namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// API 统一响应格式
    /// </summary>
    /// <typeparam name="T">响应数据的类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 业务状态码 (200: 成功, 500: 失败, etc.)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public T? Data { get; set; }

        public ApiResponse(bool success, int code, string message, T? data)
        {
            Success = success;
            Code = code;
            Message = message;
            Data = data;
        }
    }

    /// <summary>
    /// API 统一响应格式 (无数据)
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 业务状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }

        public ApiResponse(bool success, int code, string message)
        {
            Success = success;
            Code = code;
            Message = message;
        }
    }
}
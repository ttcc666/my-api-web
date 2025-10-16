using System.Text.Json.Serialization;

namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// API 统一响应格式
    /// </summary>
    /// <typeparam name="T">响应数据的类型</typeparam>
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public ApiResponse(bool success, int code, string message, T? data = default)
        {
            Success = success;
            Code = code;
            Message = message;
            Data = data;
        }
    }
}
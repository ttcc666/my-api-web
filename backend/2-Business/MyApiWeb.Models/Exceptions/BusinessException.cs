using System;

namespace MyApiWeb.Models.Exceptions
{
    /// <summary>
    /// 自定义业务异常
    /// </summary>
    public class BusinessException : Exception
    {
        public int Code { get; set; }

        public BusinessException(string message, int code = 400) : base(message)
        {
            Code = code;
        }

        public BusinessException(string message, Exception innerException, int code = 400) : base(message, innerException)
        {
            Code = code;
        }
    }
}
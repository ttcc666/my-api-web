namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 设备信息 DTO
    /// </summary>
    public class DeviceInfoDto
    {
        /// <summary>
        /// 操作系统
        /// </summary>
        public string OS { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OSVersion { get; set; } = string.Empty;

        /// <summary>
        /// 机器名称
        /// </summary>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// 处理器数量
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 系统运行时间（毫秒）
        /// </summary>
        public long TickCount { get; set; }

        /// <summary>
        /// .NET 版本
        /// </summary>
        public string DotNetVersion { get; set; } = string.Empty;
    }
}

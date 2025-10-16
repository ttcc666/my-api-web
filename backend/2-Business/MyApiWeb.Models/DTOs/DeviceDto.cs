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

        /// <summary>
        /// 系统架构
        /// </summary>
        public string Architecture { get; set; } = string.Empty;

        /// <summary>
        /// 进程架构
        /// </summary>
        public string ProcessArchitecture { get; set; } = string.Empty;

        /// <summary>
        /// 总物理内存（MB）
        /// </summary>
        public long TotalMemoryMB { get; set; }

        /// <summary>
        /// 可用物理内存（MB）
        /// </summary>
        public long AvailableMemoryMB { get; set; }

        /// <summary>
        /// 当前进程工作集内存（MB）
        /// </summary>
        public long ProcessMemoryMB { get; set; }

        /// <summary>
        /// CPU 使用率（%）
        /// </summary>
        public double CpuUsage { get; set; }

        /// <summary>
        /// 系统目录
        /// </summary>
        public string SystemDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 当前工作目录
        /// </summary>
        public string CurrentDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 是否64位操作系统
        /// </summary>
        public bool Is64BitOS { get; set; }

        /// <summary>
        /// 是否64位进程
        /// </summary>
        public bool Is64BitProcess { get; set; }
    }
}
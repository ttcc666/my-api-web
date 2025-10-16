using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces;
using System.Runtime.InteropServices;

namespace MyApiWeb.Services.Implements
{
    /// <summary>
    /// 设备服务实现
    /// </summary>
    public class DeviceService : IDeviceService
    {
        public DeviceInfoDto GetDeviceInfo()
        {
            return new DeviceInfoDto
            {
                OS = RuntimeInformation.OSDescription,
                OSVersion = Environment.OSVersion.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                TickCount = Environment.TickCount64,
                DotNetVersion = RuntimeInformation.FrameworkDescription
            };
        }
    }
}

using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;
using MyApiWeb.Services.Interfaces.System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyApiWeb.Services.Implements.System
{
    /// <summary>
    /// 设备服务实现
    /// </summary>
    public class DeviceService : IDeviceService
    {
        public DeviceInfoDto GetDeviceInfo()
        {
            var process = Process.GetCurrentProcess();
            var gcMemoryInfo = GC.GetGCMemoryInfo();

            return new DeviceInfoDto
            {
                OS = RuntimeInformation.OSDescription,
                OSVersion = Environment.OSVersion.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                TickCount = Environment.TickCount64,
                DotNetVersion = RuntimeInformation.FrameworkDescription,
                Architecture = RuntimeInformation.OSArchitecture.ToString(),
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                TotalMemoryMB = gcMemoryInfo.TotalAvailableMemoryBytes / 1024 / 1024,
                AvailableMemoryMB = (gcMemoryInfo.TotalAvailableMemoryBytes - gcMemoryInfo.MemoryLoadBytes) / 1024 / 1024,
                ProcessMemoryMB = process.WorkingSet64 / 1024 / 1024,
                CpuUsage = Math.Round(process.TotalProcessorTime.TotalMilliseconds / Environment.TickCount64 * 100, 2),
                SystemDirectory = Environment.SystemDirectory,
                CurrentDirectory = Environment.CurrentDirectory,
                Is64BitOS = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess
            };
        }
    }
}

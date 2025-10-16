using MyApiWeb.Models.DTOs;

namespace MyApiWeb.Services.Interfaces
{
    /// <summary>
    /// 设备服务接口
    /// </summary>
    public interface IDeviceService
    {
        DeviceInfoDto GetDeviceInfo();
    }
}
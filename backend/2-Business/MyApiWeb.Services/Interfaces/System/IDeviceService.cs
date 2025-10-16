using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;

namespace MyApiWeb.Services.Interfaces.System
{
    /// <summary>
    /// 设备服务接口
    /// </summary>
    public interface IDeviceService
    {
        DeviceInfoDto GetDeviceInfo();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 设备信息控制器
    /// </summary>
    [AllowAnonymous]
    public class DeviceController : ApiControllerBase<IDeviceService, object, string>
    {
        public DeviceController(ILogger<DeviceController> logger, IDeviceService service)
            : base(logger, service)
        {
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <returns>设备信息</returns>
        [HttpGet("info")]
        public IActionResult GetDeviceInfo()
        {
            var deviceInfo = _service.GetDeviceInfo();
            return Success(deviceInfo, "获取设备信息成功");
        }
    }
}

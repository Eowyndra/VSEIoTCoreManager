using Microsoft.AspNetCore.Mvc;
using VSEIoTCoreServer.WebApp.Services;
using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.WebApp.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IIoTCoreService _iotCoreService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(IDeviceConfigurationService deviceConfigurationService, IIoTCoreService iotCoreService, ILoggerFactory loggerFactory)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            _iotCoreService = iotCoreService ?? throw new ArgumentNullException(nameof(iotCoreService));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<DeviceController>();
        }

        [ProducesResponseType(200, Type = typeof(List<DeviceConfigurationViewModel>))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var deviceConfigurations = await _deviceConfigurationService.GetAll();

                return Ok(deviceConfigurations);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error:" + e.Message);
                return StatusCode(500);
            }
        }

        [ProducesResponseType(200, Type = typeof(IStatus))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet("device/{id}/status")]
        public async Task<IActionResult> GetStatus(int id)
        {
            ActionResult result;
            try
            {
                var status = await _iotCoreService.Status(id);

                result = Ok(status);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error:" + e.Message);
                result = StatusCode(500);

            }
            return result;
        }


    }
}

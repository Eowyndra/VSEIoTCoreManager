using Microsoft.AspNetCore.Mvc;
using VSEIoTCoreServer.Services;
using VSEIoTCoreServer.ViewModels;

namespace VSEIoTCoreServer.Controllers
{
    [Route("api/v1/[controller]")]
    public class DeviceController : Controller
    {
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(IDeviceConfigurationService deviceConfigurationService, ILoggerFactory loggerFactory)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
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


    }
}

// ----------------------------------------------------------------------------
// Filename: DeviceController.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Controllers
{
    using System.Collections.Concurrent;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using VSEIoTCoreServer.CommonUtils;
    using VSEIoTCoreServer.DAL.Models.Enums;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;

    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly IIoTCoreService _iotCoreService;
        private readonly IGlobalIoTCoreService _globalIoTCoreService;
        private readonly IoTCoreOptions _iotCoreOptions;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(
            IDeviceConfigurationService deviceConfigurationService,
            IIoTCoreService iotCoreService,
            IGlobalIoTCoreService globalIoTCoreService,
            ILoggerFactory loggerFactory,
            IOptions<IoTCoreOptions> iotCoreOptions)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            _iotCoreService = iotCoreService ?? throw new ArgumentNullException(nameof(iotCoreService));
            _globalIoTCoreService = globalIoTCoreService ?? throw new ArgumentNullException(nameof(globalIoTCoreService));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<DeviceController>();
            var options = iotCoreOptions ?? throw new ArgumentNullException(nameof(iotCoreOptions));
            _iotCoreOptions = options.Value;
        }

        [ProducesResponseType(200, Type = typeof(List<DeviceConfigurationViewModel>))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Get DeviceStatus and IoTStatus for each configured device
                var deviceConfigurations = new ConcurrentBag<DeviceConfigurationViewModel>(await _deviceConfigurationService.GetAll());
                Parallel.ForEach(deviceConfigurations, (device, cancelationToken) =>
                {
                    device.IoTStatus = _iotCoreService.GetIoTStatus(device.Id);
                });

                return Ok(deviceConfigurations);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error:" + e.Message);
                return StatusCode(500);
            }
        }

        [ProducesResponseType(200, Type = typeof(StatusViewModel))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(int id)
        {
            ActionResult result;
            try
            {
                var status = await _iotCoreService.Status(id);

                result = status != null ? Ok(status) : StatusCode(404);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error:" + e.Message);
                result = StatusCode(500);
            }

            return result;
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [HttpPost]
        public async Task<IActionResult> AddDevices([FromBody] List<AddDeviceViewModel> deviceModels)
        {
            // Validate model
            if (!ModelState.IsValid || deviceModels == null)
            {
                return UnprocessableEntity(ModelState);
            }

            StatusCodeResult result;
            try
            {
                foreach (var deviceModel in deviceModels)
                {
                    // Add new device to database
                    var addedDeviceViewModel = await _deviceConfigurationService.AddDevice(deviceModel);

                    // If global IoTCore instance is running, start and mirror new devices
                    var globalIoTCoreStatus = await _globalIoTCoreService.GetStatus();
                    if (globalIoTCoreStatus.Status != GlobalIoTCoreStatus.Stopped)
                    {
                        // Start VSEIoTCore for the added device
                        await _iotCoreService.Start(addedDeviceViewModel.Id);
                        var addedDeviceIsStarted = await IoTCoreUtils.WaitUntilVSEIoTCoreStarted(_iotCoreOptions.IoTCoreURI, addedDeviceViewModel.IoTCorePort);

                        if (addedDeviceIsStarted)
                        {
                            // Add VSEIoTCore to global IoTCore
                            await _globalIoTCoreService.AddMirror(addedDeviceViewModel);
                        }
                    }
                }

                result = Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error adding device: " + ex.Message);
                result = StatusCode(422);
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

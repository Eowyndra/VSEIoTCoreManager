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
    using VSEIoTCoreServer.WebApp.Models;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;

    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly ILogger<DeviceController> _logger;
        private readonly IIoTCoreServer _iotCoreServer;

        public DeviceController(IDeviceConfigurationService deviceConfigurationService, ILoggerFactory loggerFactory, IIoTCoreServer iotCoreServer)
        {
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<DeviceController>();
            _iotCoreServer = iotCoreServer ?? throw new ArgumentNullException(nameof(iotCoreServer));
        }

        [ProducesResponseType(200, Type = typeof(List<DeviceConfigurationViewModel>))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ActionResult result;
            try
            {
                // Get devices from database
                var dbDevices = new ConcurrentBag<DeviceConfigurationViewModel>(await _deviceConfigurationService.GetAll());
                var cachedDevices = await _iotCoreServer.GetDevices();

                // Get status for each configured device from cache
                foreach (var device in dbDevices)
                {
                    var cachedDevice = cachedDevices.FirstOrDefault(cd => cd.GetDeviceConfiguration().Id == device.Id);
                    if (cachedDevice != null)
                    {
                        device.DeviceStatus = cachedDevice.GetStatus().DeviceStatus;
                        device.IoTStatus = cachedDevice.GetStatus().IoTStatus;
                        device.VseType = cachedDevice.GetDeviceConfiguration().VseType;

                        await _deviceConfigurationService.UpdateDevice(device);
                    }
                }

                result = Ok(dbDevices);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error:" + e.Message);
                result = StatusCode(500);
            }

            return result;
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
                var cachedDevices = await _iotCoreServer.GetDevices();
                var cachedDevice = cachedDevices.FirstOrDefault(cd => cd.GetDeviceConfiguration().Id == id);

                var status = cachedDevice?.GetStatus();

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
        public async Task<IActionResult> AddDevices([FromBody] List<AddDeviceViewModel> addDevices)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            if (addDevices == null || addDevices.Count < 1)
            {
                return NoContent();
            }

            ActionResult result;
            try
            {
                var newDevices = new List<DeviceConfigurationViewModel>();

                foreach (var addDevice in addDevices)
                {
                    // Add devices to database
                    var newDevice = await _deviceConfigurationService.AddDevice(addDevice);
                    newDevices.Add(newDevice);
                }

                // Add devices to cache
                await _iotCoreServer.AddRange(newDevices);
                result = Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error adding devices: " + ex.Message);
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

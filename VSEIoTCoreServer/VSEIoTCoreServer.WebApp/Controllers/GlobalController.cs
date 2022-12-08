// ----------------------------------------------------------------------------
// Filename: GlobalController.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using VSEIoTCoreServer.WebApp.Models;
    using VSEIoTCoreServer.WebApp.Services;
    using VSEIoTCoreServer.WebApp.ViewModels;

    [ApiController]
    [Route("api/v1/[controller]")]
    public class GlobalController : ControllerBase
    {
        private readonly IIoTCoreServer _iotCoreServer;
        private readonly IGlobalConfigurationService _globalConfigurationService;
        private readonly ILogger<GlobalController> _logger;

        public GlobalController(IIoTCoreServer iotCoreServer, IGlobalConfigurationService globalConfigurationService, ILoggerFactory loggerFactory)
        {
            _iotCoreServer = iotCoreServer ?? throw new ArgumentNullException(nameof(iotCoreServer));
            _globalConfigurationService = globalConfigurationService ?? throw new ArgumentNullException(nameof(globalConfigurationService));

            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<GlobalController>();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            StatusCodeResult result;
            try
            {
                await _iotCoreServer.Start();
                result = Ok();
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError("Error starting global IoTCore instance: " + e.Message);
                result = StatusCode(409);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: " + e.Message);
                result = StatusCode(500);
            }

            return result;
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            StatusCodeResult result;
            try
            {
                await _iotCoreServer.Stop();
                result = Ok();
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError("Error stopping global IoTCore instance: " + e.Message);
                result = StatusCode(409);
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: " + e.Message);
                result = StatusCode(500);
            }

            return result;
        }

        [ProducesResponseType(200, Type = typeof(GlobalIoTCoreStatusViewModel))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            ActionResult result;
            try
            {
                var status = await _iotCoreServer.GetStatus();
                result = Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal Error: " + ex.Message);
                result = StatusCode(500);
            }

            return result;
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpPut("config")]
        public async Task<IActionResult> UpdateConfiguration([FromBody] GlobalConfigurationViewModel config)
        {
            ActionResult result;
            try
            {
                await _globalConfigurationService.UpdateConfig(config);
                result = Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal Error: " + ex.Message);
                result = StatusCode(500);
            }

            return result;
        }

        [ProducesResponseType(200, Type = typeof(GlobalConfigurationViewModel))]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            ActionResult result;
            try
            {
                var config = await _globalConfigurationService.GetConfig();
                result = Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal Error: " + ex.Message);
                result = StatusCode(500);
            }

            return result;
        }
    }
}

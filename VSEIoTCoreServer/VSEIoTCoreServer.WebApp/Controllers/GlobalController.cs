﻿using Microsoft.AspNetCore.Mvc;
using VSEIoTCoreServer.WebApp.Services;
using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.WebApp.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GlobalController : ControllerBase
    {
        private readonly IGlobalIoTCoreService _globalIoTCoreService;
        private readonly ILogger<GlobalController> _logger;

        public GlobalController(IGlobalIoTCoreService globalIoTCoreService, ILoggerFactory loggerFactory)
        {
            _globalIoTCoreService = globalIoTCoreService ?? throw new ArgumentNullException(nameof(globalIoTCoreService));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<GlobalController>();
        }


        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            StatusCodeResult result;
            try
            {
                await _globalIoTCoreService.Start();

                result = Ok();
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
        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            StatusCodeResult result;
            try
            {
                await _globalIoTCoreService.Stop();

                result = Ok();
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
                var status = await _globalIoTCoreService.GetStatus();

                result = Ok(status);
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
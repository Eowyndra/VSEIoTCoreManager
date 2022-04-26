// ----------------------------------------------------------------------------
// Filename: DeviceConfigurationService.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.Services
{
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using VSEIoTCoreServer.DAL;
    using VSEIoTCoreServer.DAL.Models;
    using VSEIoTCoreServer.WebApp.ViewModels;

    public class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly SQLiteDbContext _context;
        private readonly ILogger<DeviceConfigurationService> _logger;

        public DeviceConfigurationService(
            IMapper mapper,
            SQLiteDbContext context,
            ILoggerFactory loggerFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<DeviceConfigurationService>();
        }

        public async Task<List<DeviceConfigurationViewModel>> GetAll()
        {
            var deviceConfigurations = new List<DeviceConfiguration>();

            try
            {
                _logger.LogInformation("Reading device configurations...");
                var result = await _context.DeviceConfigurations.ToListAsync();
                _logger.LogInformation("Successfully read device configurations");
                deviceConfigurations.AddRange(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading device configurations: {ex.Message}");
                throw;
            }

            return _mapper.Map<List<DeviceConfiguration>, List<DeviceConfigurationViewModel>>(deviceConfigurations);
        }

        public async Task<DeviceConfigurationViewModel> GetById(int deviceId)
        {
            DeviceConfiguration? deviceConfiguration = null;
            try
            {
                _logger.LogInformation($"Reading configuration of device {deviceId}");
                deviceConfiguration = await _context.DeviceConfigurations.FirstOrDefaultAsync(device => device.Id == deviceId);
                if (deviceConfiguration == null)
                {
                    throw new KeyNotFoundException();
                }

                _logger.LogInformation($"Successfully read device configuration of device {deviceId}");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Device {deviceId} not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading device configuration of device {deviceId}: {ex.Message}");
                throw;
            }

            return _mapper.Map<DeviceConfigurationViewModel>(deviceConfiguration);
        }

        public async Task<DeviceConfigurationViewModel> AddDevice(AddDeviceViewModel deviceModel)
        {
            if (deviceModel == null)
            {
                throw new ArgumentNullException(nameof(deviceModel));
            }

            var dbDevice = _mapper.Map<DeviceConfiguration>(deviceModel);

            // Make sure that the device has a name
            await CheckDeviceName(dbDevice);

            // Make sure that the device does not already exist in the database
            await CheckIfDeviceAlreadyExists(dbDevice);

            // Make sure the assigned IoTCore Port is still available
            await IsIoTCorePortAvailable(dbDevice);

            // Add the new device to the database
            try
            {
                var entity = _context.DeviceConfigurations.Add(dbDevice);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully added device {dbDevice.VseIpAddress}:{dbDevice.VsePort}");
                dbDevice = entity.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding device configuration: {ex.Message}");
                throw;
            }

            return _mapper.Map<DeviceConfigurationViewModel>(dbDevice);
        }

        public async Task UpdateDevice(DeviceConfigurationViewModel deviceModel)
        {
            var dbDevice = _mapper.Map<DeviceConfiguration>(deviceModel);

            try
            {
                var record = await _context.DeviceConfigurations.FirstOrDefaultAsync(dev => dev.Id == dbDevice.Id);
                if (record != null)
                {
                    record.VseType = dbDevice.VseType;
                    record.VseIpAddress = dbDevice.VseIpAddress;
                    record.VsePort = dbDevice.VsePort;
                    record.IoTCorePort = dbDevice.IoTCorePort;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated device configuration");
                }
                else
                {
                    _logger.LogError("Device configuration not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating device configuration: {ex.Message}");
                throw;
            }
        }

        private async Task CheckDeviceName(DeviceConfiguration device)
        {
            if (string.IsNullOrEmpty(device.Name))
            {
                device.Name = await GetDefaultVseName();
            }
        }

        private async Task<string> GetDefaultVseName()
        {
            var number = 1;
            var newName = "Device_" + number.ToString("D3");
            var nameAlreadyUsed = await CheckIfNameAlreadyUsed(newName);

            while (nameAlreadyUsed && number <= 999)
            {
                number++;
                newName = "Device_" + number.ToString("D3");
                nameAlreadyUsed = await CheckIfNameAlreadyUsed(newName);
            }

            return newName;
        }

        private async Task<bool> CheckIfNameAlreadyUsed(string name)
        {
            var nameAlreadyUsed = await _context.DeviceConfigurations.FirstOrDefaultAsync(device => device.Name == name);
            return nameAlreadyUsed != null;
        }

        private async Task IsIoTCorePortAvailable(DeviceConfiguration deviceToBeAdded)
        {
            var iotCorePortAlreadyUsed = await _context.DeviceConfigurations.FirstOrDefaultAsync(device =>
                device.IoTCorePort == deviceToBeAdded.IoTCorePort);

            if (iotCorePortAlreadyUsed != null)
            {
                _logger.LogError("Error adding device configuration");
                throw new ArgumentException("IoTCore port already being used!");
            }
        }

        private async Task CheckIfDeviceAlreadyExists(DeviceConfiguration deviceToBeAdded)
        {
            var alreadExists = await _context.DeviceConfigurations.FirstOrDefaultAsync(device =>
                device.VseIpAddress == deviceToBeAdded.VseIpAddress &&
                device.VsePort == deviceToBeAdded.VsePort);

            if (alreadExists != null)
            {
                _logger.LogError("Error adding device configuration");
                throw new ArgumentException("Device already exists!");
            }
        }
    }
}

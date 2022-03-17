using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.WebApp.ViewModels;

namespace VSEIoTCoreServer.WebApp.Services
{
    public class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly SQLiteDbContext _context;
        private readonly ILogger<DeviceConfigurationService> _logger;

        public DeviceConfigurationService(IMapper mapper, SQLiteDbContext context, ILoggerFactory loggerFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger<DeviceConfigurationService>();
        }
        public async Task<List<DeviceConfigurationViewModel>> GetAll()
        {
            List<DeviceConfiguration> deviceConfigurations = new List<DeviceConfiguration>();

            try
            {
                _logger.LogInformation("Reading all device configurations from DataBase... ");
                var result = await _context.DeviceConfigurations.ToListAsync();
                _logger.LogInformation("Successfully read all device configurations from DataBase!");
                deviceConfigurations.AddRange(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error reading device configurations from DataBase: " + ex.Message);
                throw;
            }

            return _mapper.Map<List<DeviceConfiguration>, List<DeviceConfigurationViewModel>>(deviceConfigurations);
        }

        public async Task<DeviceConfigurationViewModel> GetById(int deviceId)
        {
            DeviceConfiguration deviceConfiguration = null;
            try
            {
                _logger.LogInformation($"Reading device configuration for device with deviceId={deviceId} from DataBase... ");
                deviceConfiguration = await _context.DeviceConfigurations.FirstOrDefaultAsync(device => device.Id == deviceId);
                if (deviceConfiguration == null) throw new KeyNotFoundException();
                _logger.LogInformation($"Successfully read device configuration for device with deviceId={deviceId} from DataBase!");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Device with deviceId={deviceId} not found in DataBase: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading device configuration for device with deviceId={deviceId} from DataBase: " + ex.Message);
                throw;
            }

            return _mapper.Map<DeviceConfigurationViewModel>(deviceConfiguration);
        }
    }
}

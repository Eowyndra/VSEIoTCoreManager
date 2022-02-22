using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VSEIoTCoreServer.DAL;
using VSEIoTCoreServer.DAL.Models;
using VSEIoTCoreServer.ViewModels;

namespace VSEIoTCoreServer.Services
{
    public class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly SQLiteDbContext _context;

        public DeviceConfigurationService(IMapper mapper, SQLiteDbContext context)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<DeviceConfigurationViewModel>> GetAll()
        {
            return _mapper.Map<List<DeviceConfiguration>, List<DeviceConfigurationViewModel>>(await _context.DeviceConfigurations.ToListAsync());
        }

        public async Task<DeviceConfigurationViewModel> GetById(int deviceId)
        {
            var deviceConfig = await _context.DeviceConfigurations.FirstOrDefaultAsync(device => device.Id == deviceId);
            if (deviceConfig == null) throw new KeyNotFoundException();
            return _mapper.Map<DeviceConfigurationViewModel>(deviceConfig);
        }
    }
}

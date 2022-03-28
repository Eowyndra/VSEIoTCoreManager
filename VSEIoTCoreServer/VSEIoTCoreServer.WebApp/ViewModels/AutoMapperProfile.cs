using AutoMapper;
using VSEIoTCoreServer.DAL.Models;

namespace VSEIoTCoreServer.WebApp.ViewModels
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<DeviceConfiguration, DeviceConfigurationViewModel>();
            CreateMap<AddDeviceViewModel, DeviceConfiguration > ();
            CreateMap<DeviceConfigurationViewModel, DeviceConfiguration>();
            CreateMap<DeviceConfigurationViewModel, StatusViewModel>();
        }
    }
}

using AutoMapper;
using VSEIoTCoreServer.DAL.Models;

namespace VSEIoTCoreServer.WebApp.ViewModels
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<DeviceConfiguration, DeviceConfigurationViewModel>();
        }
    }
}

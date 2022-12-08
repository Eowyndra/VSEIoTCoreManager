// ----------------------------------------------------------------------------
// Filename: AutoMapperProfile.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.ViewModels
{
    using AutoMapper;
    using VSEIoTCoreServer.DAL.Models;

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // DTO to ViewModel
            CreateMap<DeviceConfiguration, DeviceConfigurationViewModel>();
            CreateMap<GlobalConfiguration, GlobalConfigurationViewModel>();

            // ViewModel to DTO
            CreateMap<DeviceConfigurationViewModel, DeviceConfiguration>();
            CreateMap<GlobalConfigurationViewModel, GlobalConfiguration>();
            CreateMap<AddDeviceViewModel, DeviceConfiguration>();

            // ViewModel to ViewModel
            CreateMap<DeviceConfigurationViewModel, StatusViewModel>();
        }
    }
}

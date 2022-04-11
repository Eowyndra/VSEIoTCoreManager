// ----------------------------------------------------------------------------
// Filename: DeviceConfiguration.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.DAL.Models
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceConfiguration
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public string VseType { get; set; }
        [Required]
        public string VseIpAddress { get; set; }
        [Required]
        [Range(1, 65535)]
        public int VsePort { get; set; }
        [Required]
        [Range(1, 65535)]
        public int IoTCorePort { get; set; }
    }
}

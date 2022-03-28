﻿using System.ComponentModel.DataAnnotations;

namespace VSEIoTCoreServer.DAL.Models
{
    public class DeviceConfiguration
    {
        [Required]
        public int Id { get; set; }
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

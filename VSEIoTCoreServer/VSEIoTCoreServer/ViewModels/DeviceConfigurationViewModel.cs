using System.ComponentModel.DataAnnotations;

namespace VSEIoTCoreServer.ViewModels
{
    public class DeviceConfigurationViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
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

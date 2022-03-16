using System.ComponentModel.DataAnnotations;
using VSEIoTCoreServer.DAL.Models.Enums;

namespace VSEIoTCoreServer.ViewModels
{
    public interface IStatus
    {
        IoTStatus IoTStatus { get; set; }
        DeviceStatus DeviceStatus { get; set; }
    }
    public class DeviceConfigurationViewModel : IStatus
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
        public IoTStatus IoTStatus { get; set; }
        public DeviceStatus DeviceStatus { get; set; }
    }
}

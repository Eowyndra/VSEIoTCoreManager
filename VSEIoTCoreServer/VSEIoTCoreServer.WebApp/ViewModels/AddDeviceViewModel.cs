using System.ComponentModel.DataAnnotations;

namespace VSEIoTCoreServer.WebApp.ViewModels
{
    public class AddDeviceViewModel
    {
        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "IP-Address must be valid.")]
        public string VseIpAddress { get; set; }
        [Required]
        [Range(1, 65535)]
        public int VsePort { get; set; }
        [Required]
        [Range(1, 65535)]
        public int IoTCorePort { get; set; }
    }
}

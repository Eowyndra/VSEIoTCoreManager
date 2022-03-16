using System.ComponentModel.DataAnnotations;
using VSEIoTCoreServer.DAL.Models.Enums;

namespace VSEIoTCoreServer.ViewModels
{
    public interface IGlobalIoTCoreStatus
    {
        GlobalIoTCoreStatus Status { get; set; }
    }
    public class GlobalIoTCoreStatusViewModel : IGlobalIoTCoreStatus
    {
        public GlobalIoTCoreStatus Status { get; set; }
    }
}

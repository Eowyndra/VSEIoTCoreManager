using VSEIoTCoreServer.DAL.Models.Enums;

namespace VSEIoTCoreServer.WebApp.ViewModels
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

namespace VSEIoTCoreServer.WebApp
{
    public class IoTCoreOptions
    {
        public const string IoTCoreSettings = "IoTCoreSettings";

        public string AdapterLocation { get; set; } = string.Empty;
        public string IoTCoreURI { get; set; } = string.Empty;
        public int GlobalIoTCorePort { get; set; } = 0;   
    }
}

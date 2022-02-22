namespace VSEIoTCoreServer
{
    public class IoTCoreOptions
    {
        public const string IoTCoreSettings = "IoTCoreSettings";

        public string AdapterLocation { get; set; } = String.Empty;
        public string IoTCoreURI { get; set; } = String.Empty;
    }
}

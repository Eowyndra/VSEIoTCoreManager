namespace VSEIoTCoreServer.CommonTestUtils
{
    public static partial class IoTCoreRoutes
    {
        public static string GetData() => $"/getdata";
        public static string SetData() => $"/setdata";
        public static string GetTree() => $"/gettree";
        public static string Remote(int remoteId) => $"/remote/{remoteId}";
        public static string Device() => $"/device";
        public static string Information() => $"/information";
        public static string Status() => $"/status";
        public static string Objects() => $"/objects";
        public static string Object(int objectId) => $"/object{objectId}";
        public static string Counters() => $"/counters";
        public static string Counter(int counterId) => $"/counter{counterId}";
    }
}


namespace VSEIoTCoreServer.CommonTestUtils
{
	public static partial class IoTCoreRoutes
	{
		public static string GetData(this string s) => s + GetData();
		public static string SetData(this string s) => s + SetData();
		public static string GetTree(this string s) => s + GetTree();
		public static string Remote(this string s, int remoteId) => s + Remote(remoteId);
		public static string Device(this string s) => s + Device();
		public static string Information(this string s) => s + Information();
		public static string Status(this string s) => s + Status();
		public static string Objects(this string s) => s + Objects();
		public static string Object(this string s, int objectId) => s + Object(objectId);
		public static string Counters(this string s) => s + Counters();
		public static string Counter(this string s, int counterId) => s + Counter(counterId);

	}
}
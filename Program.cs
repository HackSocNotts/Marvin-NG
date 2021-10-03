using System.Threading.Tasks;

namespace MarvinNG
{
	internal static class Program
	{
		private static async Task Main()
		{
			await new Bot().Run();
		}
	}
}
using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MarvinNG
{
	internal static class Program
	{
		public static IMongoCollection<BsonDocument> MembersCollection, DiscordCollection;

		public static ulong HelpChannel;
		public static ulong ServerId;
		public static ulong MemberId;

		private static async Task Main()
		{
			var mongoUrl = _getEnvOrFail("mongoUrl");
			var mongoCollection = _getEnvOrFail("mongoCollection");

			var client = new MongoClient(new MongoUrl(mongoUrl));
			var database = client.GetDatabase(mongoCollection);
			MembersCollection = database.GetCollection<BsonDocument>("members");
			DiscordCollection = database.GetCollection<BsonDocument>("members_discord");

			HelpChannel = Convert.ToUInt64(_getEnvOrFail("helpChannel"));
			ServerId = Convert.ToUInt64(_getEnvOrFail("server"));
			MemberId = Convert.ToUInt64(_getEnvOrFail("memberRole"));

			await new Bot(_getEnvOrFail("botToken")).Run();
		}

		private static string _getEnvOrFail(string var)
		{
			return Environment.GetEnvironmentVariable(var) ??
			       throw new Exception($"Environment variable '{var}' not supplied");
		}
	}
}
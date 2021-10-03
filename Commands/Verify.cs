using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MarvinNG.Commands
{
	public class _Verify : ModuleBase<SocketCommandContext>
	{
		private static readonly Regex GuildUserIdRegex = new(@"^\d+$");

		[Command("verify")]
		[Summary("Verifies A User")]
		public async Task Verify(uint studentId, string user = null)
		{
			Console.WriteLine($"Verify Called by {Context.User.Username} on id {studentId}");

			#region get user object

			var guildUser = GetGuildUser(user);

			Console.WriteLine($"Trying to verify {guildUser.Username}");

			#endregion

			#region Check Discord Account is not already verified

			Console.WriteLine(Convert.ToString(guildUser.Id));
			var xs = Program.DiscordCollection.Find($"{{ DiscordID: '{guildUser.Id}' }}");
			if (await xs.CountDocumentsAsync() != 0)
			{
				var x = xs.First();
				await Context.Message.ReplyAsync(
					$"User <@{guildUser.Id}> is Already verified with StudentID {x["ID"]}");
				return;
			}

			#endregion

			#region Check StudentID is not already verified

			var filter = Builders<BsonDocument>.Filter.Eq("ID", studentId);
			xs = Program.MembersCollection.Find(filter);
			if (await xs.CountDocumentsAsync() == 0)
			{
				await Context.Message.ReplyAsync(
					$"Cannot Find User with ID {studentId} amongst our records. Open a Ticket in <#{Program.HelpChannel}> to get this sorted");
				return;
			}

			var y = xs.First();
			var zs = Program.DiscordCollection.Find(filter);
			if (await zs.CountDocumentsAsync() != 0)
			{
				await Context.Message.ReplyAsync(
					$"Student ID {studentId} is already verified to a different account. Open a Ticket in <#{Program.HelpChannel}> to get this sorted");
				return;
			}

			#endregion

			#region Update

			var document = BsonDocument.Parse($"{{\"ID\":{studentId}, \"DiscordID\":{guildUser.Id} }}");
			var db = Program.DiscordCollection.InsertOneAsync(document);

			var r = guildUser.AddRoleAsync(Bot.MemberRole);
			await r;
			await db;

			#endregion

			await ReplyAsync($"Succesfully verified <@{guildUser.Id}> as {y["Name"]}");
		}

		[Command("lookup")]
		[Summary("Looks up A User")]
		public async Task Lookup(string user)
		{
			#region get user object

			var guildUser = GetGuildUser(user);

			Console.WriteLine($"Looking up user {guildUser.Username}");

			#endregion

			#region Check Discord Account is not already verified

			Console.WriteLine(Convert.ToString(guildUser.Id));
			var xs = Program.DiscordCollection.Find($"{{ DiscordID: '{guildUser.Id}' }}");
			if (await xs.CountDocumentsAsync() != 0)
			{
				var x = xs.First();
				await Context.Message.ReplyAsync($"User <@{guildUser.Id}> is verified with StudentID {x["ID"]}");
			}
			else
			{
				await Context.Message.ReplyAsync($"User <@{guildUser.Id}> is not verified on this server!");
			}

			#endregion
		}

		[Command("clear")]
		[Summary("Clears a User")]
		public async Task Clear(string user)
		{
			#region get user object
			
			var guildUser = GetGuildUser(user);

			#endregion

			#region remove memberRole

			if (guildUser.Roles.Contains(Bot.MemberRole))
				await guildUser.RemoveRoleAsync(Bot.MemberRole);

			#endregion

			var filter = Builders<BsonDocument>.Filter.Eq("DiscordID", guildUser.Id);

			#region Update

			await Program.DiscordCollection.DeleteOneAsync(filter);

			#endregion

			await ReplyAsync($"Successfully unverified <@{guildUser.Id}> ");
		}

		private SocketGuildUser GetGuildUser(string userIdString)
		{
			var user = Bot.Server.GetUser(Context.Message.Author.Id);
			if (userIdString != null && GuildUserIdRegex.Match(userIdString).Success)
				user = Bot.Server.GetUser(Convert.ToUInt64(userIdString)) ?? user;
			else
				Console.WriteLine($"Failed to find guild user with id {userIdString}");
			return user;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MarvinNG.Commands
{
	public class _Nuke : ModuleBase<SocketCommandContext>
	{
		[Command("nuke")]
		[Summary("Removes All User verifications")]
		public async Task Nuke()
		{
			var guildUser = Bot.Server.GetUser(Context.Message.Author.Id);
			if (guildUser.Roles.Any(r => r.Permissions.Administrator))
			{
				await Program.DiscordCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

				Console.WriteLine($"Nuking {Bot.MemberRole.Members.Count()} users from {Bot.Server.Name}");
				var t = new List<Task>();
				foreach (var m in Bot.MemberRole.Members)
				{
					t.Add(m.RemoveRoleAsync(Bot.MemberRole));
					// DM The User
					t.Add(m.SendMessageAsync(
						"Hi, We've Reset the Roles on our Discord, please re-verify with the command `verify {STUDENT ID}`"));
				}

				t.ForEach(async u => await u);

				await Context.Message.ReplyAsync("Successfully Nuked All Users");
			}
		}
	}
}
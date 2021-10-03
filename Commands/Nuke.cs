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
			var gUser = Bot.server.GetUser(Context.Message.Author.Id);
			var hasAdmin = gUser.Roles.Any(r => r.Permissions.Administrator);
			Console.WriteLine(hasAdmin);
			if (hasAdmin)
			{
				await Bot.discordCollection.DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

				Console.WriteLine(Bot.memberRole.Members.Count());
				var t = new List<Task>();
				foreach (var m in Bot.memberRole.Members)
				{
					t.Add(m.RemoveRoleAsync(Bot.memberRole));
					// DM The User
					t.Add(m.SendMessageAsync(
						"Hi, We've Reset the Roles on our Discord, please re-verify with the command `verify {STUDENT ID}`"));
				}

				foreach (var u in t)
				{
					await u;
				}

				await Context.Message.ReplyAsync("Successfully Nuked All Users");
			}
		}
	}
}
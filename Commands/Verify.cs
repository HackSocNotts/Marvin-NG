using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


using Discord;
using Discord.Commands;
using Discord.WebSocket;

using MongoDB.Driver;
using MongoDB.Bson;

namespace MarvinNG.Commands
{
    public class _Verify : ModuleBase<SocketCommandContext>
    {
        private static Regex regex = new Regex(@"\d+");

        [Command("verify")]
        [Summary("Verifies A User")]
        public async Task Verify(uint ID, string user = null)
        {
            Console.WriteLine($"Verify Called by {Context.User.Username} on id {ID}");

            #region get user object
            SocketGuildUser u;
            if (user != null)
            {
                var m = regex.Match(user);
                if (!m.Success)
                {
                    Console.WriteLine($"Finding user Failed");
                    return;
                }
                var id = Convert.ToUInt64(m.Value);
                u = Bot.server.GetUser(id);
            }
            else
            {
                u = Bot.server.GetUser(Context.Message.Author.Id);
            }
            Console.WriteLine($"Trying to Verify {u.Username}");
            #endregion

            #region Check Discord Account is not already verified
            Console.WriteLine(Convert.ToString(u.Id));
            var xs = Bot.collection.Find($"{{ DiscordID: '{u.Id}' }}");
            if (xs.CountDocuments() != 0)
            {
                var x = xs.First();
                await Context.Message.ReplyAsync($"User <@{u.Id}> is Already verified with StudentID {x["ID"].ToString()}");
                return;
            }
            #endregion

            #region Check StudentID is not already verified
            var filter = Builders<BsonDocument>.Filter.Eq("ID", ID);
            xs = Bot.collection.Find(filter);
            if (xs.CountDocuments() == 0)
            {
                await Context.Message.ReplyAsync($"Cannot Find User with ID {ID} amongst our records. Open a Ticket in <#{Bot.helpChannel}> to get this sorted");
                return;
            }

            var y = xs.First();
            if (!y["DiscordID"].IsBsonNull)
            {
                await Context.Message.ReplyAsync($"Student ID {ID} is already verified to a different account. Open a Ticket in <#{Bot.helpChannel}> to get this sorted");
                return;
            }
            #endregion

            #region Update
            var update = Builders<BsonDocument>.Update.Set("DiscordID", u.Id);
            Bot.collection.UpdateOne(filter, update);
            var r = u.AddRoleAsync(Bot.memberRole);
            await r;
            #endregion
            await ReplyAsync($"Succesfully verified <@{u.Id}> as {y["Name"].ToString()}");
        }

        [Command("lookup")]
        [Summary("Looks up A User")]
        public async Task Lookup(uint ID, string user = null)
        {
            #region get user object
            SocketGuildUser u;
            if (user != null)
            {
                var m = regex.Match(user);
                if (!m.Success)
                {
                    Console.WriteLine("Finding user Failed");
                    return;
                }
                var id = Convert.ToUInt64(m.Value);
                u = Bot.server.GetUser(id);
            }
            else
            {
                u = Bot.server.GetUser(Context.Message.Author.Id);
            }
            Console.WriteLine($"Looking up user {u.Username}");
            #endregion

            #region Check Discord Account is not already verified
            Console.WriteLine(Convert.ToString(u.Id));
            var xs = Bot.collection.Find($"{{ DiscordID: '{u.Id}' }}");
            if (xs.CountDocuments() != 0)
            {
                var x = xs.First();
                await Context.Message.ReplyAsync($"User <@{u.Id}> is verified with StudentID {x["ID"].ToString()}");
            }
            else
            {
                await Context.Message.ReplyAsync($"User <@{u.Id}> is not verified on this server!");
            }
            #endregion
        }

        [Command("clear")]
        [Summary("Clears A User")]
        public async Task Clear(string user)
        {
            #region get user object

            ulong uid;
            var m = regex.Match(user);
            if (!m.Success)
            {
                return;
            }
            uid = Convert.ToUInt64(m.Value);
            var u = Bot.server.GetUser(uid);
            #endregion

            #region remove memberRole
            if (u != null && u.Roles.Contains(Bot.memberRole))
                await u.RemoveRoleAsync(Bot.memberRole);
            #endregion

            var filter = Builders<BsonDocument>.Filter.Eq("DiscordID", uid);

            #region Update
            var update = Builders<BsonDocument>.Update.Set<string>("DiscordID", null);
            Bot.collection.UpdateOne(filter, update);

            #endregion
            await ReplyAsync($"Succesfully unverified <@{uid}> ");

        }
    }

}
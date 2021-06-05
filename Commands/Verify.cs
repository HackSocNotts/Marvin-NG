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


            #region get user object
            SocketGuildUser u;
            if (user != null)
            {
                var m = regex.Match(user);
                if (!m.Success)
                {
                    return;
                }
                var id = Convert.ToUInt64(m.Value);
                u = Bot.server.GetUser(id);
            }
            else
            {
                u = Bot.server.GetUser(Context.Message.Author.Id);
            }
            #endregion

            #region Check Discord Account is not already verified
            Console.WriteLine(Convert.ToString(u.Id));
            var xs = Bot.collection.Find($"{{ DiscordID: '{u.Id}' }}");
            if (xs.CountDocuments() != 0)
            {
                var x = xs.First();
                await Context.Message.ReplyAsync(String.Format("User <@{0}> is Already verified with StudentID {1}", u.Id, x["ID"].ToString()));
                return;
            }
            #endregion

            #region Check StudentID is not already verified
            var filter = Builders<BsonDocument>.Filter.Eq("ID", ID);
            xs = Bot.collection.Find(filter);
            if (xs.CountDocuments() == 0)
            {
                await Context.Message.ReplyAsync(String.Format("Cannot Find User with ID {0} amongst our records. Open a Ticket in <#{1}> to get this sorted", ID, Bot.helpChannel));
                return;
            }

            var y = xs.First();
            if (!y["DiscordID"].IsBsonNull)
            {
                await Context.Message.ReplyAsync(String.Format("Student ID {0} is already verified to a different account. Open a Ticket in <#{1}> to get this sorted", ID, Bot.helpChannel));
                return;
            }
            #endregion

            #region Update
            var update = Builders<BsonDocument>.Update.Set("DiscordID", u.Id);
            Bot.collection.UpdateOne(filter, update);
            var r = u.AddRoleAsync(Bot.memberRole);
            await r;
            #endregion
            await ReplyAsync(String.Format("Succesfully verified <@{0}> as {1}", u.Id, y["Name"].ToString()));
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
                    return;
                }
                var id = Convert.ToUInt64(m.Value);
                u = Bot.server.GetUser(id);
            }
            else
            {
                u = Bot.server.GetUser(Context.Message.Author.Id);
            }
            #endregion

            #region Check Discord Account is not already verified
            Console.WriteLine(Convert.ToString(u.Id));
            var xs = Bot.collection.Find($"{{ DiscordID: '{u.Id}' }}");
            if (xs.CountDocuments() != 0)
            {
                var x = xs.First();
                await Context.Message.ReplyAsync(String.Format("User <@{0}> is verified with StudentID {1}", u.Id, x["ID"].ToString()));
            }
            else
            {
                await Context.Message.ReplyAsync(String.Format("User <@{0}> is not verified on this server!", u.Id));
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
            var id = Convert.ToUInt64(uid);
            u = Bot.server.GetUser(id);
            #endregion

            #region remove memberRole
            if (u != null && u.Roles.Contains(Bot.memberRole))
                await u.RemoveRoleAsync(Bot.memberRole);
            #endregion

            #region remove user from datebase

            var filter = Builders<BsonDocument>.Filter.Eq("DiscordID", uid);

            #region Update
            var update = Builders<BsonDocument>.Update.Set<string>("DiscordID", null);
            Bot.collection.UpdateOne(filter, update);
            
            #endregion
            await ReplyAsync(String.Format("Succesfully unverified <@{0}> ", uid));
            
        }
    }

}
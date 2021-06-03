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
            Console.WriteLine( Convert.ToString(u.Id));
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
                await Context.Message.ReplyAsync(String.Format("Cannot Find User with ID {0} amongst our records. Open a Ticket in get-help to get this sorted", ID));
                return;
            }

            var y = xs.First();
            if (!y["DiscordID"].IsBsonNull)
            {
                await Context.Message.ReplyAsync(String.Format("Student ID {0} is already verified to a different account. Open a Ticket in get-help to get this sorted", ID));
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

    }

}
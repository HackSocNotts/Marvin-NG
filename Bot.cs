using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Rest;

using MongoDB.Driver;
using MongoDB.Bson;


namespace MarvinNG
{
    public class Bot
    {

        public Bot()
        {
            var conf = new DiscordSocketConfig();
            conf.AlwaysDownloadUsers = true;

            _client = new DiscordSocketClient(conf);
            _commands = new CommandService();

            string botToken = Environment.GetEnvironmentVariable("botToken");
            loginAwaiter = _client.LoginAsync(TokenType.Bot, botToken);
            commandAwaiter = InstallCommandsAsync();

            var mongoUrl = Environment.GetEnvironmentVariable("mongoUrl");
            var mongoCollection = Environment.GetEnvironmentVariable("mongoCollection");

            var client = new MongoClient(new MongoUrl(mongoUrl));
            var database = client.GetDatabase(mongoCollection);
            collection = database.GetCollection<BsonDocument>("members");

            helpChannel = Convert.ToUInt64(Environment.GetEnvironmentVariable("helpChannel"));
        }


        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _client.UserJoined += HandleMemberJoin;

            await _commands.AddModuleAsync<Commands._Nuke>(null);
            await _commands.AddModuleAsync<Commands._Verify>(null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            server = _client.GetGuild(Convert.ToUInt64(Environment.GetEnvironmentVariable("server")));
            Console.WriteLine($"Connected to {server.Name}");
            memberRole = server.GetRole(Convert.ToUInt64(Environment.GetEnvironmentVariable("memberRole")));

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || (message.Channel is IDMChannel)) ||
                message.Author.IsBot) return;


            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }

        private async Task HandleMemberJoin(SocketGuildUser user)
        {
            await user.SendMessageAsync($@"Hi <@{user.Id}> :wave:
Thanks for joining the HackSoc Discord Server!
To get started you need to verify your membership with me.
Please respond with verify followed by your student ID number (for example `verify 12312123`) so I can verify that you are a HackSoc member.
If you believe this is in error, or something goes wrong, please raise a ticket in <#{helpChannel}> for help.
Thanks,
The Committee");
        }


        private DiscordSocketClient _client;
        private CommandService _commands;
        private Task loginAwaiter;
        private Task commandAwaiter;
        public static IMongoCollection<BsonDocument> collection;
        public static SocketRole memberRole;
        public static SocketGuild server;

        public static ulong helpChannel;
        public async Task Run()
        {
            await loginAwaiter;
            await commandAwaiter;
            await _client.StartAsync();
            await Task.Delay(-1);
        }

    }
}
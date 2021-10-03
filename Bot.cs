using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MarvinNG.Commands;

namespace MarvinNG
{
	public class Bot
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly Task _loginAwaiter;
		private readonly Task _commandAwaiter;
		
		public static SocketRole MemberRole;
		public static SocketGuild Server;
		
		public Bot(string token)
		{
			var conf = new DiscordSocketConfig {AlwaysDownloadUsers = true};

			_client = new DiscordSocketClient(conf);
			_commands = new CommandService();

			_loginAwaiter = _client.LoginAsync(TokenType.Bot, token);
			_commandAwaiter = InstallCommandsAsync();
		}


		private async Task InstallCommandsAsync()
		{
			// Hook the MessageReceived event into our command handler
			_client.MessageReceived += HandleCommandAsync;
			_client.UserJoined += HandleMemberJoin;

			await _commands.AddModuleAsync<_Nuke>(null);
			await _commands.AddModuleAsync<_Verify>(null);
		}

		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			// Don't process the command if it was a system message
			if (messageParam is not SocketUserMessage message) return;

			Server = _client.GetGuild(Program.ServerId);
			Console.WriteLine($"Processing command from {Server.Name}");
			MemberRole = Server.GetRole(Program.MemberId);

			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if (message.Author.IsBot) return;
			if (message.Channel is not IDMChannel) return;

			// Execute the command with the command context we just
			// created, along with the service provider for precondition checks.
			await _commands.ExecuteAsync(new SocketCommandContext(_client, message), 0, null);
		}

		private async Task HandleMemberJoin(SocketGuildUser user)
		{
			await user.SendMessageAsync($@"Hi <@{user.Id}> :wave:
Thanks for joining the HackSoc Discord Server!
To get started you need to verify your membership with me.
Please respond with verify followed by your student ID number (for example `verify 12312123`) so I can verify that you are a HackSoc member.
If you believe this is in error, or something goes wrong, please raise a ticket in <#{Program.HelpChannel}> for help.
Thanks,
The Committee");
		}

		public async Task Run()
		{
			await _loginAwaiter;
			await _commandAwaiter;
			await _client.StartAsync();
			await Task.Delay(-1);
		}
	}
}
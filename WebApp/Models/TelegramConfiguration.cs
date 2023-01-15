namespace ProtecTelegram.Models
{
	public class TelegramConfiguration
	{
		public class TelegramOptions
		{
			public const string Telegram = "Telegram";

			public string BotUserName { get; set; } = String.Empty;
			public string BotClientToken { get; set; } = String.Empty;
			public string UsernameServiceUrl { get; set; } = String.Empty;
			public string InvitationValidServiceUrl { get; set; } = String.Empty;
		}
	}
}

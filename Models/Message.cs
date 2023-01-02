using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProtecTelegram.Models
{
	public class Message
	{
		public long TelegramId { get; set; }
		public string Text { get; set; }
	}


	public class SendMessageOutput
	{
		public int IdMessage { get; set; }
	}
}

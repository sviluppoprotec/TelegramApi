using ProtecTelegram.Models;

namespace ProtecTelegram.Telegram
{
	public interface ITelegramService
	{
		Task<long> GetTelegramId(string Username);
		Task<int> Send(long chatId, string message);
		Task<InvitationResponse> ValidateInvitaition(long chatId, string messageText);
	}
}

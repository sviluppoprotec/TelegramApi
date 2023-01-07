namespace ProtecTelegram.Telegram
{
	public interface ITelegramService
	{
		Task<long> GetTelegramId(string Username);
		Task<int> Send(long chatId, string message);
	}
}

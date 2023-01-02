namespace ProtecTelegram.Telegram
{
	public interface ITelegramService
	{
		Task<int> Send(long chatId, string message);
	}
}

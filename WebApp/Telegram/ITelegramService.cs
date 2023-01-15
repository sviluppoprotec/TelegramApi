﻿using ProtecTelegram.Models;

namespace ProtecTelegram.Telegram
{
	public interface ITelegramService
	{
		Task<long> GetTelegramId(string username);
		Task<UserOutput> GetUserName(UserInput input);
		Task<int> Send(long chatId, string message);
		Task<InvitationResponse> ValidateInvitaition(long chatId, string messageText);
		public Task<string> BuildInvitation(string username);
	}
}

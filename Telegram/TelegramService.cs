using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Threading;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using ProtecTelegram.DataLayer.Database;

namespace ProtecTelegram.Telegram
{
	public class TelegramService : ITelegramService
	{
		TelegramBotClient botClient = new TelegramBotClient("5041403364:AAGiDn_1dBV6Hx5kUPkSZ2Joh-mPITrqlRw");
		UnitOfWork uow;
		public TelegramService(UnitOfWork uow)
		{
			this.uow = uow;
		}

		public async Task<int> Send(long chatId, string message)
		{
			CancellationToken cancellationToken = new();

			Message sentMessage = await botClient.SendTextMessageAsync(
				 chatId: chatId,
				 text: "You said:\n" + message,
				 cancellationToken: cancellationToken);
			return sentMessage.MessageId;

		}


		public async Task<long> GetTelegramId(string Username)
		{
			var q = await uow.Query<TeleGramUserRel>().FirstOrDefaultAsync(x => x.Username == Username);
			if (q == null)
			{
				throw new ApplicationException("Username inesistente");
			}
			return q.IdTelegram;
		}

		public static string BuildInvitationLink(string botUserName, Guid token)
		{
			var uriBuilder = new UriBuilder()
			{
				Scheme = "https",
				Host = "t.me",
				Path = botUserName,
				Query = QueryString.Create("start", token.ToString()).ToString()
			};

			return uriBuilder.ToString();
		}
	}
}

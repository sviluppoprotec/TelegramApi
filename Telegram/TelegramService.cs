using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Threading;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using ProtecTelegram.DataLayer.Database;
using ProtecTelegram.Models;

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

		public async Task<InvitationResponse> ValidateInvitaition(long chatId, string messageText)
		{
			if (messageText.StartsWith("/start"))
			{
				string[] parts = messageText.Split(' ');

				if (parts.Length == 2)
				{
					Guid token = Guid.Empty;
					var t = Guid.TryParse(parts[1], out token);
					TeleGramInvitations invito = await this.uow.Query<TeleGramInvitations>().Where(i => i.Token == token).FirstOrDefaultAsync();
					if (invito != null)
					{
						TeleGramUserRel userTelegramRel = await this.uow.Query<TeleGramUserRel>().Where(r => r.IdTelegram == chatId).FirstOrDefaultAsync();
						if (userTelegramRel != null)
						{

							return new InvitationResponse()
							{
								Valid = false,
								Message = $"Il tuo utente {userTelegramRel.Username} è già associato ad un id Telegram. Se sei tu ingonara questo messaggio, altrimenti contatta l'assisteza"
							};
						}
						TeleGramUserRel userRelToTelegram = new TeleGramUserRel(uow)
						{
							Active = true,
							DateAdded = DateTime.UtcNow,
							IdTelegram = chatId,
							Username = invito.Username
						};
						userRelToTelegram.Save();
						uow.CommitChanges();
						return new InvitationResponse()
						{
							Valid = true,
							Message = "Invito accettato. Ora riceverai i tuoi messgi qui"
						};
					}
					else
					{
						return new InvitationResponse()
						{
							Valid = false,
							Message = "Token non valido o scaduto. Si prega di contattare l'assistenza"
						};
					}
				}
			}
			return new InvitationResponse()
			{
				Valid = false,
				Message = ""
			};
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

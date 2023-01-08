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
using static ProtecTelegram.Models.TelegramConfiguration;

namespace ProtecTelegram.Telegram
{
	public class TelegramService : ITelegramService
	{
		TelegramBotClient botClient;
		UnitOfWork uow;
		TelegramOptions telegramOptions;
		public TelegramService(UnitOfWork uow, IConfiguration configuration)
		{
			this.uow = uow;
			telegramOptions = new TelegramOptions();
			configuration.GetSection(TelegramOptions.Telegram).Bind(telegramOptions);
			botClient = new TelegramBotClient(telegramOptions.BotClientToken);
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
								Message = $"Ciao {userTelegramRel.Username}. Risulta un utente già associato al tuo id Telegram. Se sei tu ignora questo messaggio, altrimenti contatta l'assisteza"
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
							Message = $"Ciao {userTelegramRel.Username}. Il tuo invito accettato! Ora riceverai i tuoi messgi qui"
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

		public async Task<string> BuildInvitation(string username)
		{
			var token = Guid.NewGuid();

			TeleGramInvitations invito = await this.uow.Query<TeleGramInvitations>().Where(i => i.Username == username).FirstOrDefaultAsync();
			if (invito == null)
			{
				TeleGramInvitations newInvitation = new TeleGramInvitations(uow)
				{
					CreationDate = DateTime.Now,
					ExpirationDate = DateTime.Now.AddMonths(1),
					Username = username,
					Token = token
				};
				newInvitation.Save();
			}
			else
			{
				invito.Token = token;
				invito.ExpirationDate = DateTime.Now.AddMonths(1);
				invito.Save();
			}
			uow.CommitChanges();


			var uriBuilder = new UriBuilder()
			{
				Scheme = "https",
				Host = "t.me",
				Path = telegramOptions.BotUserName,
				Query = QueryString.Create("start", token.ToString()).ToString()
			};

			return uriBuilder.ToString();
		}
	}
}

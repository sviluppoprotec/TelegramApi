using DevExpress.Xpo.Logger.Transport;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using DevExpress.Xpo;
using System.Diagnostics;
using ProtecTelegram.DataLayer.Database;
using System.Net;

namespace ProtecTelegram.Telegram
{
	public class TelegramHandler : IHostedService
	{
		TelegramBotClient botClient = new TelegramBotClient("5041403364:AAGiDn_1dBV6Hx5kUPkSZ2Joh-mPITrqlRw");

		UnitOfWork uow;
		public TelegramHandler(UnitOfWork uow)
		{
			this.uow = new UnitOfWork();
		}

		async void StartReceiving()
		{
			using CancellationTokenSource cts = new();

			// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
			ReceiverOptions receiverOptions = new()
			{
				AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
			};

			botClient.StartReceiving(
				 updateHandler: HandleUpdateAsync,
				 pollingErrorHandler: HandlePollingErrorAsync,
				 receiverOptions: receiverOptions,
				 cancellationToken: cts.Token
			);

			var me = await botClient.GetMeAsync();
			Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

			Console.WriteLine($"Start listening for @{me.Username}");
		}

		async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			// Only process Message updates: https://core.telegram.org/bots/api#message
			if (update.Message is not { } message)
				return;
			// Only process text messages
			if (message.Text is not { } messageText)
				return;

			var chatId = message.Chat.Id;

			Debug.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

			if (messageText.StartsWith("/start"))
			{
				string[] parts = messageText.Split(' ');

				if (parts.Length == 2)
				{
					Guid token = Guid.Empty;
					var t = Guid.TryParse(parts[1], out token);
					TeleGramInvitations invito = uow.Query<TeleGramInvitations>().Where(i => i.Token == token).FirstOrDefault();
					if (invito != null)
					{
						TeleGramUserRel userRelToTelegram = new TeleGramUserRel(uow)
						{
							Active= true,
							DateAdded= DateTime.UtcNow,
							IdTelegram = chatId,
							Username= invito.Username
						};
						userRelToTelegram.Save();
						uow.CommitChanges();
					}
				}
			}

			if (messageText.StartsWith("/start"))
			{
				using var client = new HttpClient();

				var builder = new UriBuilder("https://localhost:7252/api/Invitation");
				builder.Query = $"chatId={chatId}&messageText={messageText}";
				var url = builder.ToString();
				var result = await client.GetAsync(url);
				if (result.StatusCode == HttpStatusCode.OK)
				{
					var res = await result.Content.ReadAsStringAsync();
					bool success = bool.Parse(res);

					if (success)
					{
						await SendMessage(chatId, "Sei stato registrato con successo", cancellationToken);
					}
					else
					{
						await SendMessage(chatId, "Token non valido", cancellationToken);
					}
				}
				else
				{
					await SendMessage(chatId, "Errore nel servizio di registrazione", cancellationToken);
				}
			}

			//// Echo received message text
			//Message sentMessage = await botClient.SendTextMessageAsync(
			//	 chatId: chatId,
			//	 text: "You said:\n" + messageText,
			//	 cancellationToken: cancellationToken);

			//Invio nel gruppo
			//Message sentMessage2 = await botClient.SendTextMessageAsync(
			// chatId: -865769596,
			// text: "You said:\n" + messageText,
			// cancellationToken: cancellationToken);

			//Message sentMessage3 = await botClient.SendTextMessageAsync(
			// chatId: 5072519275,
			// text: "Amorchick è il computer che ti scrive il messaggio: " + messageText,
			// cancellationToken: cancellationToken);
		}

		private async  Task<Message> SendMessage(long chatId, string text, CancellationToken cancellationToken)
		{
			return await botClient.SendTextMessageAsync(
				 chatId: chatId,
				 text: text,
				 cancellationToken: cancellationToken);
		}

		Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var ErrorMessage = exception switch
			{
				ApiRequestException apiRequestException
					 => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};

			Console.WriteLine(ErrorMessage);
			return Task.CompletedTask;
		}

		public void Start()
		{
			string inv = TelegramService.BuildInvitationLink("PranioBot", Guid.NewGuid());
			StartReceiving();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			Start();
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}

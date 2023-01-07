﻿using DevExpress.Xpo.Logger.Transport;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using DevExpress.Xpo;
using System.Diagnostics;

namespace ProtecTelegram.Telegram
{
	public class TelegramHandler: IHostedService
	{
		TelegramBotClient botClient = new TelegramBotClient("5041403364:AAGiDn_1dBV6Hx5kUPkSZ2Joh-mPITrqlRw");
		public TelegramHandler()
		{
			StartReceiving();
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
			Console.ReadLine();

			// Send cancellation request to stop bot
			cts.Cancel();

			ChatId id = new ChatId(1064816047);
			var t = await botClient.SendTextMessageAsync(id, "text message");



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

			//// Echo received message text
			//Message sentMessage = await botClient.SendTextMessageAsync(
			//	 chatId: chatId,
			//	 text: "You said:\n" + messageText,
			//	 cancellationToken: cancellationToken);

			//Invio nel gruppo
			Message sentMessage2 = await botClient.SendTextMessageAsync(
			 chatId: -865769596,
			 text: "You said:\n" + messageText,
			 cancellationToken: cancellationToken);

			//Message sentMessage3 = await botClient.SendTextMessageAsync(
			// chatId: 5072519275,
			// text: "Amorchick è il computer che ti scrive il messaggio: " + messageText,
			// cancellationToken: cancellationToken);
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

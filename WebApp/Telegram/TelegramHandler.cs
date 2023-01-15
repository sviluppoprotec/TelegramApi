﻿using DevExpress.Xpo.Logger.Transport;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using DevExpress.Xpo;
using System.Diagnostics;
using ProtecTelegram.DataLayer.Database;
using System.Net;
using Newtonsoft.Json;
using ProtecTelegram.Models;
using static ProtecTelegram.Models.TelegramConfiguration;
using System.Threading;

namespace ProtecTelegram.Telegram
{
	public class TelegramHandler : IHostedService
	{
		TelegramBotClient botClient;
		UnitOfWork uow;
		TelegramOptions telegramOptions;

		public TelegramHandler(IConfiguration configuration)
		{
			this.uow = new UnitOfWork();

			telegramOptions = new TelegramOptions();
			configuration.GetSection(TelegramOptions.Telegram).Bind(telegramOptions);
			botClient = new TelegramBotClient(telegramOptions.BotClientToken);
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
			Debug.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

			Debug.WriteLine($"Start listening for @{me.Username}");
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

			string userName = $"Utente {message.Chat.FirstName} {message.Chat.LastName}";

			try
			{
				using var client = new HttpClient();
				var builder = new UriBuilder(telegramOptions.UsernameServiceUrl);
				builder.Query = $"telegramId={chatId}";
				var url = builder.ToString();
				var result = await client.GetAsync(url);
				var userAppo = await result.Content.ReadAsStringAsync();
				userName = userAppo != string.Empty ? userAppo : userName;
			}
			catch (Exception ex)
			{
				await SendMessage(chatId, $"Errore nel servizio di recupero utente: {ex.Message}", cancellationToken);
			}

			if (messageText.StartsWith("/start"))
			{
				using var client2 = new HttpClient();

				var builder = new UriBuilder(telegramOptions.InvitationValidServiceUrl);
				builder.Query = $"chatId={chatId}&messageText={messageText}";
				var url = builder.ToString();
				try
				{
					var result = await client2.PostAsync(url, null);
					if (result.StatusCode == HttpStatusCode.OK)
					{
						var res = await result.Content.ReadAsStringAsync();
						var invitationResp = JsonConvert.DeserializeObject<InvitationResponse>(res);

						if (invitationResp.Valid)
						{
							await SendMessage(chatId, invitationResp.Message, cancellationToken);
						}
						else
						{
							await SendMessage(chatId, invitationResp.Message, cancellationToken);
						}
					}
					else
					{
						await SendMessage(chatId, "Errore nel servizio di registrazione", cancellationToken);
					}
				}
				catch (Exception ex)
				{
					await SendMessage(chatId, $"Errore nel servizio di registrazione: {ex.Message}", cancellationToken);
				}
			}

			//Invio nel gruppo
			//Message sentMessage2 = await botClient.SendTextMessageAsync(
			// chatId: -865769596,
			// text: $"{userName} ha detto:\n" + messageText,
			// cancellationToken: cancellationToken);

		}

		private async Task<Message> SendMessage(long chatId, string text, CancellationToken cancellationToken)
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

		public async Task Start()
		{
			StartReceiving();
			try
			{
				Message sentMessage2 = await botClient.SendTextMessageAsync(
				 chatId: -865769596,
				 text: "Servizio StartReceiving avviato");
			}
			catch
			{
			}
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await Start();
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{

			Message sentMessage2 = await botClient.SendTextMessageAsync(
			 chatId: -865769596,
			 text: "Servizio StartReceiving arresato");
		}
	}
}

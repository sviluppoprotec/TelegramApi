using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProtecTelegram.DataLayer.Database;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;
using Telegram.Bot.Types;
using ProtecTelegram.Telegram;
using DevExpress.Xpo;
using ProtecTelegram.Models;

namespace ProtecTelegram.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InvitationController : ControllerBase
	{
		/// <summary>
		/// Create an invitation link to log in in the telegram bot
		/// </summary>
		/// <param name="telegram"></param>
		/// <param name="username">User name in company database</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<string> Get([FromServices] ITelegramService telegram, string username)
		{
			return await telegram.BuildInvitation(username);
		}

		/// <summary>
		/// Validate the invitation link posted by thelegram when user uses it
		/// </summary>
		/// <param name="telegram"></param>
		/// <param name="chatId"></param>
		/// <param name="messageText"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<InvitationResponse> Post([FromServices] ITelegramService telegram, long chatId, string messageText)
		{
			return await telegram.ValidateInvitaition(chatId, messageText);
		}

	}
}

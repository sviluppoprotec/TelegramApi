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
		[HttpGet]
		public async Task<InvitationResponse> Get([FromServices] ITelegramService telegram, long chatId, string messageText)
		{
			return await telegram.ValidateInvitaition(chatId, messageText);
		}

	}
}

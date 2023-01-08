using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProtecTelegram.DataLayer.Database;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;
using Telegram.Bot.Types;
using ProtecTelegram.Telegram;
using DevExpress.Xpo;

namespace ProtecTelegram.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InvitationController : ControllerBase
	{
		[HttpGet]
		public async Task<bool> Get([FromServices] UnitOfWork uow, long chatId, string messageText)
		{
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
							Active = true,
							DateAdded = DateTime.UtcNow,
							IdTelegram = chatId,
							Username = invito.Username
						};
						userRelToTelegram.Save();
						uow.CommitChanges();
						return true;
					}
				}
			}
			return false;
		}

	}
}

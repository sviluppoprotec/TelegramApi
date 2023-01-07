using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProtecTelegram.Models;
using ProtecTelegram.Telegram;

namespace ProtecTelegram.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TelegramController : ControllerBase
	{
		[HttpPost]
		public async Task<SendMessageOutput> Post([FromServices] ITelegramService service, [FromBody] Message message)
		{
			// id andrea 5830412583;

			// ConnectionHelper.Connect(AutoCreateOption.DatabaseAndSchema);

			var r = await service.Send(await service.GetTelegramId(message.UserName), message.Text);
			return new SendMessageOutput
			{
				IdMessage = r
			};
		}
	}
}

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using ProtecTelegram.DataLayer.Database;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.Xpo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<ProtecTelegram.Telegram.ITelegramService, ProtecTelegram.Telegram.TelegramService>();
builder.Services.AddHostedService<ProtecTelegram.Telegram.TelegramHandler>();

builder.Services.AddControllers();

var s = builder.Services.AddXpoDefaultUnitOfWork(true, options => options
	.UseConnectionString(builder.Configuration.GetConnectionString("PC-PRANIOProtecTelegram"))
	.UseThreadSafeDataLayer(true)
	//.UseAutoCreationOption(DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema) // Remove this line if the database already exists
	.UseEntityTypes(typeof(TeleGramUserRel), typeof(TeleGramInvitations)) // Pass all of your persistent object types to this method.
);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QRCODE App"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

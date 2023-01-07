using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using ProtecTelegram.DataLayer.Database;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<ProtecTelegram.Telegram.ITelegramService, ProtecTelegram.Telegram.TelegramService>();
builder.Services.AddHostedService<ProtecTelegram.Telegram.TelegramHandler>();

builder.Services.AddControllers();

builder.Services.AddXpoDefaultUnitOfWork(true, options => options
						  .UseConnectionString(builder.Configuration.GetConnectionString("PC-PRANIOProtecTelegram"))
						  .UseThreadSafeDataLayer(true)                  
						  .UseAutoCreationOption(DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema) // Remove this line if the database already exists
						  .UseEntityTypes(typeof(TeleGramUserRel)) // Pass all of your persistent object types to this method.
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

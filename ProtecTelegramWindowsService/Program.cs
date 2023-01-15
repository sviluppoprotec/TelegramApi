using ProtecTelegramWindowsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

using IHost host = Host.CreateDefaultBuilder(args)
	 .UseWindowsService(options =>
	 {
		 options.ServiceName = ".NET Telegram Service";
	 })
	 .ConfigureServices(services =>
	 {
		 LoggerProviderOptions.RegisterProviderOptions<
			  EventLogSettings, EventLogLoggerProvider>(services);

		 services.AddSingleton<TelegramService>();
		 services.AddHostedService<Worker>();
	 })
	 .ConfigureLogging((context, logging) =>
	 {
		 // See: https://github.com/dotnet/runtime/issues/47303
		 logging.AddConfiguration(
			  context.Configuration.GetSection("Logging"));
	 })
	 .Build();

await host.RunAsync();
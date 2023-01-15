namespace ProtecTelegramWindowsService;

public sealed class Worker : BackgroundService
{
	private readonly ILogger<Worker> _logger;

	private readonly TelegramService _telegramService;

	public Worker(
		 TelegramService telegramService,
		 ILogger<Worker> logger)
	{
		(_telegramService, _logger) = (telegramService, logger);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{

		_telegramService.StartAsync(stoppingToken);
		while (!stoppingToken.IsCancellationRequested)
		{
			_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
			
			await Task.Delay(10000, stoppingToken);
		}
	}
}

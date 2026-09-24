using Microsoft.EntityFrameworkCore;

using ProductManagement.Infrastructure.Persistence;

namespace ProductManagement.MigrationService;

public sealed class MigrationWorker(
	IServiceScopeFactory serviceScopeFactory,
	IHostApplicationLifetime hostApplicationLifetime,
	ILogger<MigrationWorker> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.MigrationStarting();

		try
		{
			using var scope = serviceScopeFactory.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(() => context.Database.MigrateAsync(stoppingToken));

			logger.MigrationCompleted();
		}
		catch (Exception ex)
		{
			logger.MigrationFailed(ex);
			Environment.ExitCode = 1;
		}
		finally
		{
			hostApplicationLifetime.StopApplication();
		}
	}
}

internal static partial class MigrationWorkerLog
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Starting database migration.")]
	public static partial void MigrationStarting(this ILogger logger);

	[LoggerMessage(Level = LogLevel.Information, Message = "Database migration completed successfully.")]
	public static partial void MigrationCompleted(this ILogger logger);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Database migration failed.")]
	public static partial void MigrationFailed(this ILogger logger, Exception exception);
}
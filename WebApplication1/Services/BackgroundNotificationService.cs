using Database;
using Database.Dto;
using Database.Entities;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace WebApplication1.Services
{
    public class BackgroundNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SSEService _sseService;
        private readonly ILogger<BackgroundNotificationService> _logger;
        private readonly ConcurrentQueue<BackgroundNotificationJob> _jobQueue = new();

        public BackgroundNotificationService(
            IServiceScopeFactory scopeFactory,
            SSEService sseService,
            ILogger<BackgroundNotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _sseService = sseService;
            _logger = logger;
        }

        public void EnqueueJob(int userId, string fileName, string downloadUrl)
        {
            _jobQueue.Enqueue(new BackgroundNotificationJob
            {
                UserId = userId,
                FileName = fileName,
                DownloadUrl = downloadUrl,
                EnqueuedAt = DateTime.UtcNow
            });
            _logger.LogInformation("Enqueued background job for user {UserId}, file {FileName}", userId, fileName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundNotificationService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_jobQueue.TryDequeue(out var job))
                {
                    _ = ProcessJobAsync(job, stoppingToken);
                }
                await Task.Delay(500, stoppingToken);
            }
        }

        private async Task ProcessJobAsync(BackgroundNotificationJob job, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing background job for user {UserId}", job.UserId);

            try
            {
                _logger.LogInformation("Starting data generation for user {UserId}...", job.UserId);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                var message = $"Twoje dane są gotowe do pobrania: {job.FileName}. " +
                              $"Kliknij aby pobrać: {job.DownloadUrl}";

                var notification = new Notification
                {
                    UserId = job.UserId,
                    Message = message,
                    Created = DateTime.Now,
                    Read = false
                };

                context.Notifications.Add(notification);
                context.SaveChanges();

                // Reload to get generated ID
                notification = context.Notifications
                    .Where(n => n.UserId == job.UserId && n.Message == message && !n.Read)
                    .OrderByDescending(n => n.Created)
                    .FirstOrDefault();

                if (notification != null)
                {
                    var dto = mapper.Map<NotificationDto>(notification);
                    await _sseService.SendToUserAsync(dto, cancellationToken);
                    _logger.LogInformation("Notification sent to user {UserId}", job.UserId);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Background job for user {UserId} was cancelled.", job.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing background job for user {UserId}", job.UserId);
            }
        }
    }

    public class BackgroundNotificationJob
    {
        public int UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTime EnqueuedAt { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using WebServer.Data;

namespace WebServer.Services
{
    public class ClientCleanUpService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _timeoutPeriod = TimeSpan.FromSeconds(30); // Set the timeout period
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

        public ClientCleanUpService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Calculate the timeout threshold (DateTime to compare against LastUpdated)
                    var timeoutThreshold = DateTime.UtcNow.Add(-_timeoutPeriod);

                    // Perform the filtering on the database side (server-side)
                    var inactiveClients = await dbContext.Clients
                        .Where(c => c.LastUpdated < timeoutThreshold)  // Server-side comparison
                        .ToListAsync();

                    if (inactiveClients.Any())
                    {
                        dbContext.Clients.RemoveRange(inactiveClients);
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"Removed {inactiveClients.Count} inactive clients.");
                    }
                }

                await Task.Delay(_checkInterval, stoppingToken);  // Wait for the next check
            }
        }

    }
}

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ScheduledCronJob.ScheduledTask;


namespace ScheduledCronJob.Example
{
    public class ScheduledTask : IScheduledTask
    {
        private readonly ILogger<ScheduledTask> _logger;

        public ScheduledTask(ILogger<ScheduledTask> logger)
        {
            _logger = logger;
        }

        public string Schedule { get; } = "*/1 * * * *";

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            _logger.LogInformation("Run ScheduledTask");
        }
    }
}

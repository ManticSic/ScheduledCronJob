using System.Threading;
using System.Threading.Tasks;


namespace ScheduledCronJob.ScheduledTask
{
    public interface IScheduledTask
    {
        public string Schedule { get; }

        public Task ExecuteAsync(CancellationToken cancellationToken);
    }
}

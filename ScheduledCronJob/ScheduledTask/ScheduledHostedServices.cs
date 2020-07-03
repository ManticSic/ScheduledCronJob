using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NCrontab;


namespace ScheduledCronJob.ScheduledTask
{
    public class ScheduledHostedServices : BaseHostedService
    {
        private readonly IEnumerable<SchedulerTaskWrapper> _scheduledTasks;

        public ScheduledHostedServices(IEnumerable<IScheduledTask> scheduledTasks)
        {
            DateTime referenceTimeUtc = DateTime.UtcNow;

            _scheduledTasks = scheduledTasks.Select(scheduledTask =>
                                                        new SchedulerTaskWrapper(
                                                            CrontabSchedule.Parse(scheduledTask.Schedule),
                                                            scheduledTask,
                                                            referenceTimeUtc));
        }

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            TaskFactory taskFactory = new TaskFactory(TaskScheduler.Current);

            List<SchedulerTaskWrapper> tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun())
                                                                           .ToList();

            foreach(SchedulerTaskWrapper taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();

                await taskFactory.StartNew(async () =>
                                           {
                                               try
                                               {
                                                   await taskThatShouldRun.Task.ExecuteAsync(cancellationToken);
                                               }
                                               catch(Exception ex)
                                               {
                                                   UnobservedTaskExceptionEventArgs args =
                                                       new UnobservedTaskExceptionEventArgs(
                                                           ex as AggregateException ?? new AggregateException(ex));

                                                   UnobservedTaskException?.Invoke(this, args);

                                                   if(!args.Observed)
                                                   {
                                                       throw;
                                                   }
                                               }
                                           },
                                           cancellationToken);
            }
        }

        private class SchedulerTaskWrapper
        {
            public SchedulerTaskWrapper(CrontabSchedule schedule, IScheduledTask task, DateTime referenceTimeUtc)
            {
                Schedule       = schedule;
                Task           = task;
                NextRunTimeUtc = referenceTimeUtc;
            }

            [NotNull]
            public CrontabSchedule Schedule { get; }

            [NotNull]
            public IScheduledTask Task { get; }

            public DateTime LastRunTimeUtc { get; private set; }

            public DateTime NextRunTimeUtc { get; private set; }

            public void Increment()
            {
                LastRunTimeUtc = NextRunTimeUtc;
                NextRunTimeUtc = Schedule.GetNextOccurrence(NextRunTimeUtc);
            }

            public bool ShouldRun()
            {
                return NextRunTimeUtc < DateTime.UtcNow && LastRunTimeUtc != NextRunTimeUtc;
            }
        }
    }
}

# Scheduled Cron Job

This is a simple example how to implement scheduling of background tasks like `@Scheduled` in the Spring framework.

The interface `ScheduledCronJob.ScheduledTask.IScheduledTask` provides an unified interface for jobs which should be executed by the background service.   
The class `ScheduledCronJob.ScheduledTask.ScheduledHostedServices` holds an collection of all cron jobs and checks every second which tasks should be run.  
The abstract class `ScheduledCronJob.BaseHostedService` simplifies running background tasks and is not mandatory.  

This example uses an ASP.Net Core Application, but can be adapt for any .Net environment.

If you want to use it outside of ASP.Net simply create an Task containing a while-loop like `ScheduledCronJob.ScheduledTask.ScheduledHostedServices.ExecuteAsync(CancellationToken)`.

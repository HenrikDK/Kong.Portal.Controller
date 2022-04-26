using Kong.Portal.Controller.Reconciliation;

namespace Kong.Portal.Controller;

public class ServiceHost : IHostedService
{
    private readonly IReconciliationScheduler _reconciliationScheduler;
    private readonly ILogger<ServiceHost> _logger;
    private TimeSpan _delay = TimeSpan.FromMinutes(1);
    private List<Task> _tasks = new List<Task>();
    private KestrelMetricServer _server = new KestrelMetricServer(1402);

    public ServiceHost(IReconciliationScheduler reconciliationScheduler,
        ILogger<ServiceHost> logger)
    {
        _reconciliationScheduler = reconciliationScheduler;
        _logger = logger;

        _server.Start();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var scheduler = Task.Run(() => _reconciliationScheduler.RunOnInterval(cancellationToken, _delay)).ContinueWith(HandleTaskCancellation);
        _tasks.Add(scheduler);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Task.WaitAll(_tasks.ToArray(), TimeSpan.FromSeconds(15));
            
        return Task.CompletedTask;
    }

    private void HandleTaskCancellation(Task task)
    {
        if (!IsCancellationException(task.Exception))
        {
            _logger.LogError(task.Exception, "Service failed");

            throw task.Exception;
        }
    }

    private bool IsCancellationException(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return true;
        }
            
        if (exception is AggregateException)
        {
            var aggregate = (AggregateException) exception;

            return aggregate.InnerExceptions.Any(IsCancellationException);
        }

        return false;
    }
}
using Kong.Portal.Controller.Reconciliation.Cleanup;

namespace Kong.Portal.Controller.Reconciliation;

public interface IReconciliationScheduler
{
    void RunOnInterval(CancellationToken token, TimeSpan delay);
}
    
public class ReconciliationScheduler : IReconciliationScheduler
{
    private readonly IApiReconciliation _apiReconciliation;
    private readonly ICleanupClusterApis _cleanupClusterApis;
    private TimeSpan _delay = TimeSpan.FromMinutes(1);
    
    public ReconciliationScheduler(IApiReconciliation apiReconciliation, ICleanupClusterApis cleanupClusterApis)
    {
        _apiReconciliation = apiReconciliation;
        _cleanupClusterApis = cleanupClusterApis;
    }
    
    public void RunOnInterval(CancellationToken token, TimeSpan delay)
    {
        _delay = delay;
        var stopWatch = new Stopwatch();
        while (!token.IsCancellationRequested)
        {
            stopWatch.Start();
            
            _apiReconciliation.ProcessClusterApis();
            
            _cleanupClusterApis.CleanupApiData();

            WaitForNextExecution(token, stopWatch);
        }
    }

    private void WaitForNextExecution(CancellationToken token, Stopwatch stopWatch)
    {
        stopWatch.Stop();
        var elapsed = stopWatch.Elapsed;
        var calculated = _delay.Subtract(elapsed);
        stopWatch.Reset();

        if (elapsed < _delay)
        {
            Task.Delay(calculated, token).Wait(token);
        }
    }
}
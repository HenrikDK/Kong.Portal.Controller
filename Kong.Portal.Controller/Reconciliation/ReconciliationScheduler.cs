namespace Kong.Portal.Controller.Reconciliation;

public interface IReconciliationScheduler
{
    void RunOnInterval(CancellationToken token, TimeSpan delay);
}
    
public class ReconciliationScheduler : IReconciliationScheduler
{
    private readonly IApiReconciliation _apiReconciliation;
    private TimeSpan _delay = TimeSpan.FromMinutes(1);
    
    public ReconciliationScheduler(IApiReconciliation apiReconciliation)
    {
        _apiReconciliation = apiReconciliation;
    }
    
    public void RunOnInterval(CancellationToken token, TimeSpan delay)
    {
        _delay = delay;
        var stopWatch = new Stopwatch();
        while (!token.IsCancellationRequested)
        {
            stopWatch.Start();
            
            _apiReconciliation.ProcessClusterApis();

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
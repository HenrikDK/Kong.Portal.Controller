namespace Kong.Portal.Controller.Reconciliation;

public interface IApiReconciliation
{
    void ProcessClusterApis();
}
    
public class ApiReconciliation : IApiReconciliation
{
    private readonly ILogger<ApiReconciliation> _logger;

    public ApiReconciliation(ILogger<ApiReconciliation> logger)
    {
        _logger = logger;
    }
       
    public void ProcessClusterApis()
    {
        try
        {
            _logger.LogInformation($"Updating..");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing cluster apis");
        }
    }
}
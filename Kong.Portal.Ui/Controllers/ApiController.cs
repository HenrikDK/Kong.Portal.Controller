using Kong.Portal.Ui.Model;

namespace Kong.Portal.Ui.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private readonly ICachedSchemaRepository _cache;

    public ApiController(ICachedSchemaRepository cache)
    {
        _cache = cache;
    }
        
    [HttpGet("/swagger.json")]
    public ActionResult Get()
    {
        var json = _cache.GetSchema();
        if (json == null)
        {
            return NotFound();
        }
            
        return Content(json, "application/json");
    }
}
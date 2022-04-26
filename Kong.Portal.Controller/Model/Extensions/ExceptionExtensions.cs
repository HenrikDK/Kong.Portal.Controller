namespace Kong.Portal.Controller.Model.Extensions;

public static class ExceptionExtensions
{
    public static Exception WithContext(this Exception e, string field, object value)
    {
        e.Data.Add(field, value.ToString());
        return e;
    }
}
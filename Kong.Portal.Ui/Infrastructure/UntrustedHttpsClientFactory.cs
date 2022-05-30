namespace Kong.Portal.Ui.Infrastructure;

public class UntrustedHttpsClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler() {
        return new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
    }
}
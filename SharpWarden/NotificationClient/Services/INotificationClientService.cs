using SharpWarden.NotificationClient.Models;

namespace SharpWarden.NotificationClient.Services;

public interface INotificationClientService
{
    public const string BitWardenEUHostUrl = "wss://notifications.bitwarden.eu";

    event Func<PushNotificationBaseModel, Task> OnPushNotificationAsyncReceived;
    void ResetEventSubscribers();
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}
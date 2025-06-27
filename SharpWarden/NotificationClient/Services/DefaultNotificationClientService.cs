using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SharpWarden.NotificationClient.Models;

namespace SharpWarden.NotificationClient.Services;

public class DefaultNotificationClientService : INotificationClientService
{
    private readonly string _BaseUrl;
    private readonly string _AccessToken;
    private HubConnection _hubConnection;

    public DefaultNotificationClientService(string baseUrl, string accessToken)
    {
        _BaseUrl = baseUrl.TrimEnd('/');
        _AccessToken = accessToken;
    }

    public event Func<PushNotificationBaseModel, Task> OnPushNotificationAsyncReceived;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_BaseUrl}/hub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_AccessToken);
                options.DefaultTransferFormat = Microsoft.AspNetCore.Connections.TransferFormat.Binary;
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
            })
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePack.MessagePackSerializerOptions.Standard
                    .WithResolver(ContractlessStandardResolver.Instance)
                    .WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray);
            })
            .Build();

        _hubConnection.On<PushNotificationBaseModel>(
            "ReceiveMessage",
            async (message) =>
            {
                var payloadBytes = MessagePackSerializer.Serialize(message.Payload);

                switch (message.Type)
                {
                    case PushNotificationType.SyncCipherUpdate:
                    case PushNotificationType.SyncCipherCreate:
                    case PushNotificationType.SyncCipherDelete:
                    case PushNotificationType.SyncLoginDelete:
                        message = new PushNotificationModel<SyncCipherPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<SyncCipherPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;

                    case PushNotificationType.SyncFolderUpdate:
                    case PushNotificationType.SyncFolderCreate:
                    case PushNotificationType.SyncFolderDelete:
                        message = new PushNotificationModel<SyncFolderPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<SyncFolderPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;

                    case PushNotificationType.SyncCiphers:
                    case PushNotificationType.SyncVault:
                    case PushNotificationType.SyncOrganizations:
                    case PushNotificationType.SyncOrgKeys:
                    case PushNotificationType.SyncSettings:
                    case PushNotificationType.LogOut:
                    case PushNotificationType.PendingSecurityTasks:
                        message = new PushNotificationModel<UserPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<UserPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;

                    case PushNotificationType.SyncSendCreate:
                    case PushNotificationType.SyncSendUpdate:
                    case PushNotificationType.SyncSendDelete:
                        message = new PushNotificationModel<SyncSendPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<SyncSendPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;

                    case PushNotificationType.AuthRequestResponse:
                    case PushNotificationType.AuthRequest:
                        message = new PushNotificationModel<AuthRequestPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<AuthRequestPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;

                    case PushNotificationType.SyncOrganizationStatusChanged:
                    case PushNotificationType.SyncOrganizationCollectionSettingChanged:
                        message = new PushNotificationModel<OrganizationStatusPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<OrganizationStatusPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;

                    case PushNotificationType.Notification:
                    case PushNotificationType.NotificationStatus:
                        message = new PushNotificationModel<NotificationPushNotificationModel>
                        {
                            ContextId = message.ContextId,
                            Type = message.Type,
                            Payload = MessagePackSerializer.Deserialize<NotificationPushNotificationModel>(payloadBytes, new MessagePackSerializerOptions(ContractlessStandardResolver.Instance)),
                        };
                        break;
                }

                await OnPushNotificationAsyncReceived?.Invoke(message);
            });

        await _hubConnection.StartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
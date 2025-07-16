// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SharpWarden.NotificationClient.Models;
using SharpWarden.WebClient.Services;

namespace SharpWarden.NotificationClient.Services;

public class DefaultNotificationClientService : INotificationClientService, IDisposable
{
    private readonly string _baseUrl;
    private readonly IWebClientService _webClientService;
    private HubConnection _hubConnection;

    public DefaultNotificationClientService(
        IWebClientService webClientService,
        string baseUrl)
    {
        _webClientService = webClientService;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public event Func<PushNotificationBaseModel, Task> OnPushNotificationAsyncReceived;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/hub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_webClientService.GetWebSession().AccessToken);
                options.DefaultTransferFormat = Microsoft.AspNetCore.Connections.TransferFormat.Binary;
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
            })
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithResolver(ContractlessStandardResolver.Instance)
                    .WithCompression(MessagePackCompression.Lz4BlockArray);
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

                await (OnPushNotificationAsyncReceived?.Invoke(message)!).ConfigureAwait(false);
            });

        await _hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        ResetEventSubscribers();
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync().ConfigureAwait(false);
            await _hubConnection.DisposeAsync().ConfigureAwait(false);
        }
    }

    public void ResetEventSubscribers()
    {
        OnPushNotificationAsyncReceived = null;
    }

    public void Dispose()
    {
        ResetEventSubscribers();
        if (_hubConnection != null)
        {
            _hubConnection.StopAsync().GetAwaiter().GetResult();
            _hubConnection.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}

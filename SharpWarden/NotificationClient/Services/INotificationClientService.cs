// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

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
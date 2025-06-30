// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.NotificationClient.Models;

public class PushNotificationBaseModel
{
    public PushNotificationType Type { get; set; }
    public string ContextId { get; set; }
    public object Payload { get; set; }
}

public class PushNotificationModel<T> : PushNotificationBaseModel
{
    public new T Payload { get => (T)base.Payload; set => base.Payload = value; }
}
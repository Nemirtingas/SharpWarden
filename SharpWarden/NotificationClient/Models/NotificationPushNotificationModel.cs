// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.NotificationClient.Models;

public class NotificationPushNotificationModel
{
    public Guid Id { get; set; }
    public PushNotificationPriority Priority { get; set; }
    public bool Global { get; set; }
    public PushNotificationClientType ClientType { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? InstallationId { get; set; }
    public Guid? TaskId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime RevisionDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}
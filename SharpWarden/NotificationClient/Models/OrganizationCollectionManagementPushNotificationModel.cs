namespace SharpWarden.NotificationClient.Models;

public class OrganizationCollectionManagementPushNotificationModel
{
    public Guid OrganizationId { get; init; }
    public bool LimitCollectionCreation { get; init; }
    public bool LimitCollectionDeletion { get; init; }
    public bool LimitItemDeletion { get; init; }
}
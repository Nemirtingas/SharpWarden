namespace SharpWarden.NotificationClient.Models;

public class OrganizationStatusPushNotificationModel
{
    public Guid OrganizationId { get; set; }
    public bool Enabled { get; set; }
}
namespace SharpWarden.NotificationClient.Models;

public class SyncSendPushNotificationModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime RevisionDate { get; set; }
}
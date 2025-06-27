namespace SharpWarden.NotificationClient.Models;

public class SyncFolderPushNotificationModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime RevisionDate { get; set; }
}
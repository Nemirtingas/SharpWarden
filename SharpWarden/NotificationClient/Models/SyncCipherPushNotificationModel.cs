namespace SharpWarden.NotificationClient.Models;

public class SyncCipherPushNotificationModel
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrganizationId { get; set; }
    public IEnumerable<Guid> CollectionIds { get; set; }
    public DateTime RevisionDate { get; set; }
}
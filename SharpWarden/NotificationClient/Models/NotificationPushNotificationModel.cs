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
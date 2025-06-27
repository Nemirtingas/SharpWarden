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
namespace SharpWarden.NotificationClient;

public enum PushNotificationClientType : byte
{
    All = 0,
    Web = 1,
    Browser = 2,
    Desktop = 3,
    Mobile = 4,
    Cli = 5
}
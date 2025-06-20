namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public enum UriMatchType
{
    Domain = 0,
    Host = 1,
    BeginWith = 2,
    Exact = 3,
    Regex = 4,
    Never = 5,
}
namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public enum UrlMatchType
{
    Domain = 0,
    Host = 1,
    BeginWith = 2,
    Exact = 3,
    Regex = 4,
    Never = 5,
}
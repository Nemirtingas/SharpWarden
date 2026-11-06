using Newtonsoft.Json;
using SharpWarden.WebClient.Models;

namespace Tests;

/// <summary>
/// Offline tests of the cipher request payload.
/// </summary>
[TestClass]
public sealed class CipherRequestAPIModelTest
{
    [TestMethod]
    public void CipherRequestSerializesEncryptedForAndKey()
    {
        var userId = Guid.NewGuid();
        var json = JsonConvert.SerializeObject(new CipherItemSecureNoteRequestAPIModel
        {
            EncryptedFor = userId,
            Key = "2.key",
        });

        StringAssert.Contains(json, $"\"encryptedFor\":\"{userId}\"");
        StringAssert.Contains(json, "\"key\":\"2.key\"");
    }

    [TestMethod]
    public void CipherRequestSerializesArchivedDate()
    {
        var json = JsonConvert.SerializeObject(new CipherItemSecureNoteRequestAPIModel
        {
            ArchivedDate = new DateTime(2026, 11, 6, 12, 30, 0, DateTimeKind.Utc),
        });

        StringAssert.Contains(json, "\"archivedDate\":\"2026-11-06T12:30:00Z\"");
    }
}

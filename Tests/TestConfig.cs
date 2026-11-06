#nullable disable

using Newtonsoft.Json;

namespace Tests;

/// <summary>
/// Server and test data used by <see cref="SharpWardenTest"/>.
/// Loaded from the file given by SHARP_WARDEN_TEST_CONFIG, or TestConfig.json (the Bitwarden EU test account).
/// A fresh Vaultwarden can be seeded with Tests.Seed, which writes a matching configuration file.
/// </summary>
public sealed class TestConfig
{
    [JsonProperty("hostUrl")]
    public string HostUrl { get; set; }

    [JsonProperty("notificationUrl")]
    public string NotificationUrl { get; set; }

    /// <summary>
    /// Optional account credentials (only for throwaway servers). The SHARP_WARDEN_TEST_ACCOUNT_* environment variables take precedence.
    /// </summary>
    [JsonProperty("account")]
    public TestAccount Account { get; set; }

    [JsonProperty("userItemId")]
    public string UserItemId { get; set; }

    [JsonProperty("userFolderId")]
    public string UserFolderId { get; set; }

    [JsonProperty("userItemInFolderId")]
    public string UserItemInFolderId { get; set; }

    [JsonProperty("userItemLoginId")]
    public string UserItemLoginId { get; set; }

    [JsonProperty("userItemCardId")]
    public string UserItemCardId { get; set; }

    [JsonProperty("userItemIdentityId")]
    public string UserItemIdentityId { get; set; }

    [JsonProperty("userItemSecureNoteId")]
    public string UserItemSecureNoteId { get; set; }

    [JsonProperty("userItemSSHKeyId")]
    public string UserItemSSHKeyId { get; set; }

    [JsonProperty("userItemCustomFieldsId")]
    public string UserItemCustomFieldsId { get; set; }

    [JsonProperty("organizationId")]
    public string OrganizationId { get; set; }

    [JsonProperty("organizationCollectionId")]
    public string OrganizationCollectionId { get; set; }

    [JsonProperty("organizationItemId")]
    public string OrganizationItemId { get; set; }

    public sealed class TestAccount
    {
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public const string ConfigPathVariable = "SHARP_WARDEN_TEST_CONFIG";

    public static TestConfig Load()
    {
        var path = Environment.GetEnvironmentVariable(ConfigPathVariable);
        if (string.IsNullOrWhiteSpace(path))
            path = Path.Combine(AppContext.BaseDirectory, "TestConfig.json");

        return JsonConvert.DeserializeObject<TestConfig>(File.ReadAllText(path))
            ?? throw new InvalidDataException($"Invalid test configuration file '{path}'.");
    }

    public void Save(string path)
        => File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
}
